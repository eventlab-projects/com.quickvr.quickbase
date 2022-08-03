using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR 
{

    public class QuickMicrophone : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public float _amplifyFactor = 1;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected bool _isInitialized = false;

        protected AudioClip _audioClip = null;          //The AudioClip that is being used for recording. 
        protected AudioClip _lastRecordedClip = null;   //The last recorded AudioClip

        protected int _sampleStart = 0;
        protected bool _isRecording = false;

        protected float[] _rawData = new float[MIC_TIME * MIC_FREQUENCY];

        #endregion

        #region CONSTANTS

        protected const int MIC_FREQUENCY = 44100;
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

            _audioClip = Microphone.Start(null, true, MIC_TIME, MIC_FREQUENCY);
        }

        #endregion

        #region GET AND SET

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
        }

        public virtual AudioClip StopRecording()
        {
            if (_isInitialized && _isRecording && _audioClip)
            {
                int sampleEnd = Microphone.GetPosition("");
                int numSamples = sampleEnd > _sampleStart ? sampleEnd - _sampleStart : (_audioClip.samples - _sampleStart) + sampleEnd;
                //int numSamples = _audioClip.samples;
                
                _audioClip.GetData(_rawData, 0);
                float[] recordData = new float[numSamples];
                for (int i = 0; i < numSamples; i++)
                {
                    int sampleID = (_sampleStart + i) % _audioClip.samples;
                    recordData[i] = _rawData[sampleID] * _amplifyFactor;
                }

                _lastRecordedClip = AudioClip.Create("MicRecord", numSamples, 1, MIC_FREQUENCY, false);
                _lastRecordedClip.SetData(recordData, 0);
            }

            _isRecording = false;

            return _lastRecordedClip;
        }

        #endregion

    }

}
