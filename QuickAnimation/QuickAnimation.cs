using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickAnimation
    {

        #region PROTECTED ATTRIBUTES

        protected float _timeLength = 0;

        protected Dictionary<string, QuickAnimationCurve> _curves = new Dictionary<string, QuickAnimationCurve>();

        protected Animator _animator = null;
        protected HumanPose _pose = new HumanPose();

        #endregion

        #region CONSTANTS

        public const string CURVE_TRANSFORM_POSITION = "TransformPosition";
        public const string CURVE_TRANSFORM_ROTATION = "TransformRotation";
        public const string CURVE_BODY_POSITION = "BodyPosition";
        public const string CURVE_BODY_ROTATION = "BodyRotation";

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickAnimation(Animator animator)
        {
            _animator = animator;
        }

        #endregion

        #region GET AND SET

        public virtual QuickAnimationCurve GetAnimationCurve(string curveName)
        {
            if (!_curves.ContainsKey(curveName))
            {
                _curves[curveName] = new QuickAnimationCurve();
            }

            return _curves[curveName];
        }

        public virtual void AddKey(float time)
        {
            GetAnimationCurve(CURVE_TRANSFORM_POSITION).AddKey(time, _animator.transform.position);
            GetAnimationCurve(CURVE_TRANSFORM_ROTATION).AddKey(time, _animator.transform.rotation);

            if (_animator.isHuman)
            {
                QuickHumanPoseHandler.GetHumanPose(_animator, ref _pose);
                GetAnimationCurve(CURVE_BODY_POSITION).AddKey(time, _pose.bodyPosition);
                GetAnimationCurve(CURVE_BODY_ROTATION).AddKey(time, _pose.bodyRotation);

                for (int i = 0; i < _pose.muscles.Length; i++)
                {
                    string muscleName = QuickHumanTrait.GetMuscleName(i);
                    GetAnimationCurve(muscleName).AddKey(time, _pose.muscles[i]);
                }
            }

            if (time > _timeLength) _timeLength = time;
        }

        public virtual Vector3 EvaluateTransformPosition(float time)
        {
            return GetAnimationCurve(CURVE_TRANSFORM_POSITION).EvaluateVector3(time);
        }

        public virtual Quaternion EvaluateTransformRotation(float time)
        {
            return GetAnimationCurve(CURVE_TRANSFORM_ROTATION).EvaluateQuaternion(time);
        }

        public virtual void EvaluateHumanPose(float time, ref HumanPose pose)
        {
            if (_animator.isHuman)
            {
                pose.bodyPosition = GetAnimationCurve(CURVE_BODY_POSITION).EvaluateVector3(time);
                pose.bodyRotation = GetAnimationCurve(CURVE_BODY_ROTATION).EvaluateQuaternion(time);

                List<float> muscles = new List<float>();
                for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
                {
                    muscles.Add(_curves[QuickHumanTrait.GetMuscleName(i)].Evaluate(time));
                }

                pose.muscles = muscles.ToArray();
            }
        }

        public virtual float GetTimeLength()
        {
            return _timeLength;
        }

        public virtual AnimationClip ToAnimationClip()
        {
            AnimationClip anim = new AnimationClip();

            QuickAnimationCurve curvePos = GetAnimationCurve(CURVE_BODY_POSITION);
            anim.SetCurve("", typeof(Animator), "RootT.x", curvePos[0]);
            anim.SetCurve("", typeof(Animator), "RootT.y", curvePos[1]);
            anim.SetCurve("", typeof(Animator), "RootT.z", curvePos[2]);

            QuickAnimationCurve curveRot = GetAnimationCurve(CURVE_BODY_ROTATION);
            anim.SetCurve("", typeof(Animator), "RootQ.x", curvePos[0]);
            anim.SetCurve("", typeof(Animator), "RootQ.y", curvePos[1]);
            anim.SetCurve("", typeof(Animator), "RootQ.z", curvePos[2]);
            anim.SetCurve("", typeof(Animator), "RootQ.w", curvePos[3]);

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                anim.SetCurve("", typeof(Animator), muscleName, _curves[muscleName][0]);
            }

            return anim;
        }

        public virtual void SetEvaluateMethod(QuickAnimEvaluateMethod evaluateMethod)
        {
            foreach (var pair in _curves)
            {
                pair.Value.SetEvaluateMethod(evaluateMethod);
            }
        }

        #endregion

    }

}