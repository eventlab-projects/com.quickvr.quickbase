using UnityEngine;
using System.Collections;

//Implementation of a simple Exponential Smoothing Model for different data types

#region FLOAT EXPONENTIAL SMOOTH IMPLEMENTATION

public class ExponentialSmoothFloat : BaseSmoothMethod<float> {
	
	#region CREATION AND DESTRUCTION
	
	public ExponentialSmoothFloat(float smoothFactor, int numSamples) : base(smoothFactor, numSamples) {
		
	}
	
	#endregion
		
	#region GET AND SET
	
	protected override float ComputeForecastValueImp(int sampleID) {
		if (sampleID == 0) return _samplesList[0];
		return (((1.0f - _smoothFactor) * _samplesList[sampleID]) + (_smoothFactor * _forecastValues[sampleID - 1]));
	}
	
	#endregion
	
}

#endregion

#region VECTOR3 EXPONENTIAL SMOOTH IMPLEMENTATION

public class ExponentialSmoothVector3 : BaseSmoothMethod<Vector3> {

	#region CREATION AND DESTRUCTION

	public ExponentialSmoothVector3(float smoothFactor, int numSamples) : base(smoothFactor, numSamples) {

	}

	#endregion

	#region GET AND SET

	protected override Vector3 ComputeForecastValueImp(int sampleID) {
		if (sampleID == 0) return _samplesList[0];
		return (Vector3.Lerp(_samplesList[sampleID], _forecastValues[sampleID - 1], _smoothFactor));
	}

	#endregion

}

#endregion

#region QUATERNION EXPONENTIAL SMOOTH IMPLEMENTATION

public class ExponentialSmoothQuaternion : BaseSmoothMethod<Quaternion> {
	
	#region CREATION AND DESTRUCTION
	
	public ExponentialSmoothQuaternion(float smoothFactor, int numSamples) : base(smoothFactor, numSamples) {
		
	}
	
	#endregion
	
	
	#region GET AND SET

	public override void AddSample(Quaternion q) {
		Vector4 v = new Vector4(q.x, q.y, q.z, q.w);
		if (!Mathf.Approximately(v.magnitude, 0.0f)) {
			base.AddSample(q);
		}
	}
	
	protected override Quaternion ComputeForecastValueImp(int sampleID) {
		if (sampleID == 0) return _samplesList[0];
		return (Quaternion.Lerp(_samplesList[sampleID], _forecastValues[sampleID - 1], _smoothFactor));
	}
	
	#endregion
	
}

#endregion