using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	/// <summary>
	/// Implementation of a Holt Double Exponential Smoothing filter. The double exponential
	/// smooths the curve and predicts.  There is also noise jitter removal. And maximum
	/// prediction bounds.  The parameters are commented in the Init function.
	/// 
	/// REFERENCE: https://github.com/achagani/Avateering-XNA/blob/master/Avateering/Filters/SkeletonJointsPositionDoubleExponentialFilter.cs
	/// 
	/// </summary>

	#region GENERIC DOUBLE EXPONENTIAL FILTER

	[System.Serializable]
	public abstract class DoubleExponentialFilter<T> {

		#region PUBLIC PARAMETERS
		
		[SerializeField] public float _smoothing = 0.5f;			//[0..1], lower values is closer to the raw data and more noisy. Will lag when too high
		[SerializeField] public float _correction = 0.5f;			//[0..1], higher values correct faster and feel more responsive. Can make things springy
		[SerializeField] public float _prediction = 0.5f;			//[0..n], how many time into the future we want to predict. Can over shoot when too high
		[SerializeField] public float _jitterRadius = 0.1f;			//The deviation distance in m that defines jitter. Can do too much smoothing when too high
		[SerializeField] public float _maxDeviationRadius = 0.1f;	//The maximum distance in m that filtered positions are allowed to deviate from raw data. Can snap back to noisy data when too high
		
		#endregion
		
		#region PROTECTED PARAMETERS
		
		protected T _rawValue;			
		protected T _filteredValue; 	
		protected T _trend;				
		protected uint _numSamples = 0;				
		
		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
			Reset();
		}

		public virtual void Reset() {
			_numSamples = 0;
		}

		#endregion

		#region GET AND SET

		public virtual T GetFilteredValue() {
			return _filteredValue;
		}

		protected abstract T GetIdentity();
		protected abstract T Addition(T lhs, T rhs);
		protected abstract T Difference(T lhs, T rhs);
		protected abstract T Interpolate(T from, T to, float t);
		protected abstract float Distance(T from, T to);
		protected abstract T ComputePrediction();

		#endregion

		#region UPDATE

		public virtual T Filter(T rawValue) {
			T prevFilteredValue = _filteredValue;
			T prevTrend = _trend;
			T prevRawValue = _rawValue;
			_rawValue = rawValue;
			
			// Initial start values
			if (_numSamples == 0) {
				_filteredValue = _rawValue;
				_trend = GetIdentity();
			}
			else if (_numSamples == 1) {
				_filteredValue = Interpolate(_rawValue, prevRawValue, 0.5f);
				_trend =  Interpolate(prevTrend, Difference(_filteredValue, prevFilteredValue), _correction);
			}
			else {              
				// First apply jitter filter
				float t = Distance(_rawValue, prevFilteredValue) / _jitterRadius;
				_filteredValue = Interpolate(prevFilteredValue, _rawValue, t);
				
				// Now the double exponential smoothing filter
				_filteredValue = Interpolate(_filteredValue, Addition(prevFilteredValue, prevTrend), _smoothing);
				_trend = Interpolate(prevTrend, Difference(_filteredValue, prevFilteredValue), _correction);
			}      
			_numSamples++;
			
			// Predict into the future to reduce latency
			T predictedValue = ComputePrediction();
			
			// Check that we are not too far away from raw data
			float dist = Distance(predictedValue, _rawValue);
			if (dist > _maxDeviationRadius) predictedValue = Interpolate(_rawValue, predictedValue, _maxDeviationRadius / dist);
			
			return predictedValue;
		}

		#endregion

	}

	#endregion

	#region VECTOR3 DOUBLE EXPONENTIAL FILTER

	[System.Serializable]
	public class DoubleExponentialFilterVector3 : DoubleExponentialFilter<Vector3> {

		#region GET AND SET

		protected override Vector3 GetIdentity() {
			return Vector3.zero;
		}

		protected override Vector3 Addition(Vector3 lhs, Vector3 rhs) {
			return lhs + rhs;
		}

		protected override Vector3 Difference(Vector3 lhs, Vector3 rhs) {
			return lhs - rhs;
		}

		protected override Vector3 Interpolate(Vector3 from, Vector3 to, float t) {
			return Vector3.Lerp(from, to, t);
		}

		protected override float Distance(Vector3 from, Vector3 to) {
			return Vector3.Distance(from, to);
		}

		protected override Vector3 ComputePrediction() {
			return _filteredValue + (_trend * _prediction);
		}
		
		#endregion
		
	}

	#endregion

	#region QUATERNION DOUBLE EXPONENTIAL FILTER

	[System.Serializable]
	public class DoubleExponentialFilterQuaternion : DoubleExponentialFilter<Quaternion> {

		#region GET AND SET

		protected override Quaternion GetIdentity() {
			return Quaternion.identity;
		}

		protected override Quaternion Addition(Quaternion lhs, Quaternion rhs) {
			return lhs * rhs;
		}

		protected override Quaternion Difference(Quaternion lhs, Quaternion rhs) {
			return Quaternion.Inverse(lhs) * rhs;
		}

		protected override Quaternion Interpolate(Quaternion from, Quaternion to, float t) {
			return Quaternion.Slerp(from, to, t);
		}

		protected override float Distance(Quaternion from, Quaternion to) {
			return Quaternion.Angle(from, to);
		}

		protected override Quaternion ComputePrediction() {
			return _filteredValue * Interpolate(Quaternion.identity, _trend, _prediction);
		}

		#endregion

	}

	#endregion
}