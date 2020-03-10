using UnityEngine;
using UnityEngine.Animations;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickRecorderHumanoid : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public Animator _animator = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected AnimationClipPlayable _clipPlayable;
        protected Coroutine _coRecorder = null;
        protected Dictionary<string, AnimationCurve> _animationCurves = new Dictionary<string, AnimationCurve>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            if (!_animator) _animator = GetComponent<Animator>();
        }

        protected virtual void InitAnimationCurves()
        {
            //Root translation
            _animationCurves["RootT.x"] = new AnimationCurve();
            _animationCurves["RootT.y"] = new AnimationCurve();
            _animationCurves["RootT.z"] = new AnimationCurve();

            //Root rotation
            _animationCurves["RootQ.x"] = new AnimationCurve();
            _animationCurves["RootQ.y"] = new AnimationCurve();
            _animationCurves["RootQ.z"] = new AnimationCurve();
            _animationCurves["RootQ.w"] = new AnimationCurve();

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                _animationCurves[muscleName] = new AnimationCurve();
            }
        }

        public virtual void Record()
        {
            Record(_animator);
        }

        public virtual void Record(Animator animator)
        {
            _animator = animator;

            if (animator)
            {
                _coRecorder = StartCoroutine(CoRecordAnimation());
            }
            else
            {
                Debug.LogError("QuickRecorderHumanoid: Animator is null!!!");
            }
        }

        public virtual AnimationClip StopRecording()
        {
            if (_coRecorder != null)
            {
                StopCoroutine(_coRecorder);
                _coRecorder = null;

                AnimationClip recordedAnimation = new AnimationClip();
                foreach (var pair in _animationCurves)
                {
                    recordedAnimation.SetCurve("", typeof(Animator), pair.Key, pair.Value);
                }

                return recordedAnimation;
            }

            return null;
        }

        #endregion

        #region GET AND SET

        public virtual bool IsRecording()
        {
            return _coRecorder != null;
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoRecordAnimation()
        {
            HumanPoseHandler poseHandler = new HumanPoseHandler(_animator.avatar, _animator.transform);
            HumanPose pose = new HumanPose();

            float time = 0.0f;
            InitAnimationCurves();

            while (true)
            {
                // Wrap the clip in a playable
                poseHandler.GetHumanPose(ref pose);
                for (int i = 0; i < pose.muscles.Length; i++)
                {
                    string muscleName = QuickHumanTrait.GetMuscleName(i);
                    AnimationCurve curve = _animationCurves[muscleName];
                    curve.AddKey(time, pose.muscles[i]);

                    _animationCurves["RootT.x"].AddKey(time, pose.bodyPosition.x);
                    _animationCurves["RootT.y"].AddKey(time, pose.bodyPosition.y);
                    _animationCurves["RootT.z"].AddKey(time, pose.bodyPosition.z);

                    _animationCurves["RootQ.x"].AddKey(time, pose.bodyRotation.x);
                    _animationCurves["RootQ.y"].AddKey(time, pose.bodyRotation.y);
                    _animationCurves["RootQ.z"].AddKey(time, pose.bodyRotation.z);
                    _animationCurves["RootQ.w"].AddKey(time, pose.bodyRotation.w);
                }
                time += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

    }
}
