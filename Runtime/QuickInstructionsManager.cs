using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickInstructionsManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public float _timePauseBetweenInstructions = 0.5f;

        public float _volume = 1.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected AudioSource _audioSource = null;
        protected bool _isPlaying = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _audioSource = gameObject.GetOrCreateComponent<AudioSource>();
            _audioSource.spatialBlend = 0.0f;
            _audioSource.volume = _volume;
            _audioSource.bypassListenerEffects = true;
        }

        #endregion

        #region GET AND SET

        public virtual void SetAudioSource(AudioSource aSource)
        {
            _audioSource = !aSource ? gameObject.GetComponent<AudioSource>() : aSource;
        }

        public virtual void Play(AudioClip instruction)
        {
            List<AudioClip> tmp = new List<AudioClip>();
            tmp.Add(instruction);

            Play(tmp);
        }

        public virtual void Play(List<AudioClip> instructions)
        {
            Stop();
            StartCoroutine(CoPlayInstructions(instructions));
        }

        public virtual void Stop()
        {
            StopAllCoroutines();
            _audioSource.Stop();
            _isPlaying = false;
        }

        public virtual bool IsPlaying()
        {
            return _isPlaying;
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoPlayInstructions(List<AudioClip> instructions)
        {
            _isPlaying = true;
            
            int numInstructions = instructions.Count;
            for (int i = 0; i < numInstructions; i++)
            {
                AudioClip clip = instructions[i];
                if (clip)
                {
                    yield return StartCoroutine(CoPlayInstruction(clip));
                    if (i < numInstructions - 1)
                    {
                        yield return new WaitForSeconds(_timePauseBetweenInstructions);
                    }
                }
            }
                        
            _isPlaying = false;
        }

        protected virtual IEnumerator CoPlayInstruction(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
            while (clip && _audioSource.isPlaying) yield return null;
            _audioSource.Stop();
        }

        #endregion

    }

}
