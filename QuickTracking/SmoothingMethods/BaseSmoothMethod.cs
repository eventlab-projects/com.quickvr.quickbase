using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseSmoothMethod<T> {

	#region PROTECTED PARAMETERS

	protected float _smoothFactor = DEFAULT_SMOOTH_FACTOR;		//0.0f => No Smooth; 1.0f => Maximum Smooth
	protected int _maxNumSamples = DEFAULT_MAX_NUM_SAMPLES;
	protected Queue<T> _samples = new Queue<T>();
	protected List<T> _samplesList = null;
	protected List<T> _forecastValues = null;

	#endregion

	#region CONSTANTS

	public static float DEFAULT_SMOOTH_FACTOR = 0.5f;
	public static int DEFAULT_MAX_NUM_SAMPLES = 25;

	#endregion

	#region CREATION AND DESTRUCTION

	public BaseSmoothMethod(float smoothFactor, int numSamples) {
		_smoothFactor = smoothFactor;
		_maxNumSamples = numSamples;
	}

	#endregion

	#region GET AND SET

	protected virtual void ResizeSamples() {
		while (_samples.Count > _maxNumSamples) _samples.Dequeue();
	}

	public virtual void ResetSamples() {
		_samples.Clear();
	}

	public virtual void AddSample(T sample) {
		_samples.Enqueue(sample);
		ResizeSamples();
	}

	public virtual int GetNumSamples() {
		return _samples.Count;
	}

	public virtual float GetSmoothFactor() {
		return _smoothFactor;
	}

	public virtual void SetSmoothFactor(float s) {
		_smoothFactor = s;
	}

	public virtual void SetMaxNumSamples(int maxNumSamples) {
		_maxNumSamples = maxNumSamples;
		ResizeSamples();
	}

	public virtual bool ComputeForecastValue(ref T result) {
		//Computes the forecast value for the last sample
		return ComputeForecastValue(Mathf.Max(0, _samples.Count - 1), ref result);
	}

	public virtual bool ComputeForecastValue(int sampleID, ref T result) {
		//Computes the forecast value for a given sample
		_samplesList = new List<T>(_samples);
		if (sampleID >= _samples.Count) return false;

		_forecastValues = new List<T>();
		for (int i = 0; i <= sampleID; i++) {
			_forecastValues.Add(ComputeForecastValueImp(i));
		}
		result = _forecastValues[sampleID];
		return true;
	}

	protected abstract T ComputeForecastValueImp(int sampleID);

	#endregion

}