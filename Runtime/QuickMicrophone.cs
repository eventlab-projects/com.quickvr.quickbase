using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR 
{

    public class QuickMicrophone : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public float _amplifyFactor = 1;

        public enum SamplingRate
        {
            HZ_08k   = 8000, 
            HZ_16k  = 16000, 
            HZ_22k  = 22000, 
            HZ_44k  = 44100, 
        }
        public SamplingRate _samplingRate = SamplingRate.HZ_44k;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected bool _isInitialized = false;

        protected AudioClip _audioClip = null;          //The AudioClip that is being used for recording. 
        protected AudioClip _lastRecordedClip = null;   //The last recorded AudioClip

        protected int _sampleStart = 0;
        protected bool _isRecording = false;

        protected float[] _rawData = null;

        #endregion

        #region CONSTANTS

        protected const int MIC_TIME = 600;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual IEnumerator Start()
        {
            while (Microphone.devices.Length == 0)
            {
                yield return null;
            }
            _isInitialized = true;

            _rawData = new float[MIC_TIME * (int)_samplingRate];
            _audioClip = Microphone.Start(null, true, MIC_TIME, (int)_samplingRate);
        }

        #endregion

        #region GET AND SET

        public virtual bool IsInitialized()
        {
            return _isInitialized;
        }

        public virtual AudioClip GetAudioClip(out int currentSample)
        {
            //currentSample contains the current sample in the _audioClip where the microphone is recording. 
            currentSample = _isInitialized? Microphone.GetPosition("") : 0;

            return _audioClip;
        }

        public virtual void Record()
        {
            if (_isInitialized && !_isRecording)
            {
                _sampleStart = Microphone.GetPosition("");
                _isRecording = true;
            }
            else
            {
                Debug.LogError("[QuickMicrophone.Record]:" + "_isInitialized = " + _isInitialized + " _isRecording = " + _isRecording);
            }
        }

        public virtual AudioClip StopRecording()
        {
            if (_isInitialized && _isRecording)
            {
                _lastRecordedClip = GetRecordingClip("MicRecord", _sampleStart, Microphone.GetPosition(""));
                
                //int sampleEnd = Microphone.GetPosition("");
                //int numSamples = sampleEnd > _sampleStart ? sampleEnd - _sampleStart : (_audioClip.samples - _sampleStart) + sampleEnd;
                ////int numSamples = _audioClip.samples;
                
                //_audioClip.GetData(_rawData, 0);
                //float[] recordData = new float[numSamples];
                //for (int i = 0; i < numSamples; i++)
                //{
                //    int sampleID = (_sampleStart + i) % _audioClip.samples;
                //    recordData[i] = _rawData[sampleID] * _amplifyFactor;
                //}

                //_lastRecordedClip = AudioClip.Create("MicRecord", numSamples, 1, MIC_FREQUENCY, false);
                //_lastRecordedClip.SetData(recordData, 0);
            }
            
            _isRecording = false;

            return _lastRecordedClip;
        }

        public virtual float[] GetRecordingData(int sampleStart, int sampleEnd)
        {
            float[] result = null;

            if (_isInitialized)
            {
                int numSamples = sampleEnd > sampleStart ? sampleEnd - sampleStart : (_audioClip.samples - sampleStart) + sampleEnd;
                //int numSamples = _audioClip.samples;

                _audioClip.GetData(_rawData, 0);
                result = new float[numSamples];
                for (int i = 0; i < numSamples; i++)
                {
                    int sampleID = (_sampleStart + i) % _audioClip.samples;
                    result[i] = _rawData[sampleID] * _amplifyFactor;
                }
            }

            return result;
        }

        public virtual AudioClip GetRecordingClip(string recordName, int sampleStart, int sampleEnd)
        {
            AudioClip result = null;
            float[] recordingData = GetRecordingData(sampleStart, sampleEnd);
            
            if (recordingData != null)
            {
                result = AudioClip.Create(recordName, recordingData.Length, 1, (int)_samplingRate, false);
                result.SetData(recordingData, 0);
            }

            return result;
        }

        #endregion

    }

}
