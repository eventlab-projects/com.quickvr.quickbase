using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace QuickVR
{

    public class QuickAnimationPlayer : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public QuickAnimation _playAnimationClip = null;
        public bool _playLoop = false;
        public QuickAnimEvaluateMethod _playMode = QuickAnimEvaluateMethod.Interpolate;

        public int _recordFPS = 30;

        [Range(0.0f, 1.0f)]
        public float _weight = 1.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _recordTimeFrame = 0.0f;
        protected float _recordTimeStart = 0.0f;
        protected float _recordTimeLastFrame = 0.0f;
        
        protected float _playTimeStart = 0;
        protected float _playTimeEnd = Mathf.Infinity;
        protected float _playTimeCurrent = 0;
        protected float _playTimePrev = 0;

        protected Animator _animator = null;
        protected HumanPose _humanPose = new HumanPose();

        protected enum State
        {
            Idle, 
            Recording, 
            Playing,
        }
        protected State _state = State.Idle;

        protected QuickAnimation _recordedAnimation = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator = gameObject.GetOrCreateComponent<Animator>();
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPreCameraUpdate += UpdatePlayer;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPreCameraUpdate -= UpdatePlayer;
        }

        #endregion

        #region GET AND SET

        protected virtual void SetState(State newState)
        {
            _state = newState;
        }

        public virtual void Record()
        {
            if (enabled)
            {
                _recordedAnimation = new QuickAnimation(_animator);
                _recordTimeFrame = 1.0f / (float)_recordFPS;
                _recordTimeStart = Time.time;
                _recordTimeLastFrame = Time.time;
                SetState(State.Recording);
                UpdateStateRecording(0, false);
            }
        }

        public virtual QuickAnimation StopRecording()
        {
            if (_state == State.Recording)
            {
                UpdateStateRecording((_recordTimeLastFrame - _recordTimeStart) + Time.deltaTime, true);
                SetState(State.Idle);

                return _recordedAnimation;
            }

            return null;
        }

        public virtual QuickAnimation GetRecordedAnimation()
        {
            return _recordedAnimation;
        }

        public virtual void Play(QuickAnimation clip, float timeStart = 0, float timeEnd = Mathf.Infinity)
        {
            _playAnimationClip = clip;
            Play(timeStart, timeEnd);
        }

        public virtual void Play(float timeStart = 0, float timeEnd = Mathf.Infinity)
        {
            if (enabled && _playAnimationClip != null)
            {
                _playAnimationClip.SetEvaluateMethod(_playMode);
                _playTimeStart = _playTimeCurrent = _playTimePrev = timeStart;
                _playTimeEnd = Mathf.Min(timeEnd, _playAnimationClip.GetTimeLength());

                SetState(State.Playing);
            }
        }

        public virtual void Playback(float timeStart = 0, float timeEnd = Mathf.Infinity)
        {
            //Plays the last recorded animation
            Play(_recordedAnimation, timeStart, timeEnd);
        }

        public virtual bool IsIdle()
        {
            return _state == State.Idle;
        }

        public virtual bool IsPlaying()
        {
            return _state == State.Playing;
        }

        public virtual bool IsRecording()
        {
            return _state == State.Recording;
        }

        #endregion

        #region UPDATE

        protected virtual void UpdatePlayer()
        {
            if (_state == State.Recording)
            {
                float elapsedTime = Time.time - _recordTimeLastFrame;
                if (elapsedTime >= _recordTimeFrame)
                {
                    UpdateStateRecording(Time.time - _recordTimeStart, false);
                    _recordTimeLastFrame = Time.time;
                }
            }
            else if (_state == State.Playing)
            {
                UpdateStatePlaying();
            }
        }

        protected virtual void UpdateStateRecording(float time, bool forceAdd)
        {
            _recordedAnimation.AddKey(time, forceAdd);
        }

        protected virtual void UpdateStatePlaying()
        {
            //Update the Animation
            UpdateStatePlaying(_playTimePrev, _playTimeCurrent);
            _playTimePrev = _playTimeCurrent;
            _playTimeCurrent += Time.deltaTime;

            if (_playTimeCurrent >= _playTimeEnd)
            {
                if (_playLoop)
                {
                    _playTimeCurrent = _playTimeStart + Mathf.Repeat(_playTimeCurrent, _playTimeEnd);
                    _playTimePrev = _playTimeStart;
                }
                else
                {
                    UpdateStatePlaying(_playTimeEnd, _playTimeEnd);
                    SetState(State.Idle);
                }
            }
        }

        protected virtual void UpdateStatePlaying(float prevTime, float time)
        {
            Quaternion qOffset = _playAnimationClip.EvaluateTransformRotation(time) * Quaternion.Inverse(_playAnimationClip.EvaluateTransformRotation(prevTime));
            transform.rotation *= qOffset;

            Vector3 pOffset = _playAnimationClip.EvaluateTransformPosition(time) - _playAnimationClip.EvaluateTransformPosition(prevTime);
            qOffset = transform.rotation * Quaternion.Inverse(_playAnimationClip.EvaluateTransformRotation(0));
            transform.position += qOffset * pOffset;
            //transform.position = _playAnimationClip.EvaluateTransformPosition(time);
            //transform.rotation = _playAnimationClip.EvaluateTransformRotation(time);
            
            if (_animator.isHuman)
            {
                _playAnimationClip.EvaluateHumanPose(time, ref _humanPose);
                QuickHumanPoseHandler.SetHumanPose(_animator, ref _humanPose);
            }
        }

        #endregion

    }

}
