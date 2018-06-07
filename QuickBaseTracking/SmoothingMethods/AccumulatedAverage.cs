using UnityEngine;
using System.Collections;

public abstract class AccumulatedAverage<T> {

	#region PROTECTED PARAMETERS

	protected T _currentValue;		//The current accumulated value
	protected int _numSamples = 0;	//The current number of samples

	#endregion

	#region GET AND SET

	public virtual void AddSample(T sample) {
		if (_numSamples == 0) _currentValue = sample;
		else {
			float w = (float)_numSamples / (float)(_numSamples + 1);	//The weight of the currentValue in the average computation
			_currentValue = AddSampleImp(sample, w);
		}
		_numSamples++;
	}

	protected abstract T AddSampleImp(T sample, float w);

	public virtual void Reset() {
		_numSamples = 0;
	}

	public T GetCurrentValue() {
		return _currentValue;
	}

	public int GetNumSamples() {
		return _numSamples;
	}

	#endregion

}

public class AccumulatedAverageFloat : AccumulatedAverage<float> {

	protected override float AddSampleImp(float sample, float w) {
		return Mathf.Lerp(sample, _currentValue, w);
	}

}

public class AccumulatedAverageVector3 : AccumulatedAverage<Vector3> {

	protected override Vector3 AddSampleImp(Vector3 sample, float w) {
		return Vector3.Lerp(sample, _currentValue, w);
	}

}

public class AccumulatedAverageQuaternion : AccumulatedAverage<Quaternion> {

	protected override Quaternion AddSampleImp(Quaternion sample, float w) {
		return Quaternion.Lerp(sample, _currentValue, w);
	}

}
