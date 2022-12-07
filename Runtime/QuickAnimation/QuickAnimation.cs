using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickAnimation
    {

        [System.Serializable]
        public struct CompressionParameters
        {
            [Header("Compression Error Root")]
            [Range(0.0f, 1.0f)]
            public float _epsilonBodyPosition;
            [Range(0.0f, 1.0f)]
            public float _epsilonBodyRotation;

            [Header("Compression Error Body")]
            [Range(0.0f, 1.0f)]
            public float _epsilonSpine;
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftArm;
            [Range(0.0f, 1.0f)]
            public float _epsilonRightArm;
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftLeg;
            [Range(0.0f, 1.0f)]
            public float _epsilonRightLeg;

            [Header("Compression Error Left Hand")]
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftThumb;
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftIndex;
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftMiddle;
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftRing;
            [Range(0.0f, 1.0f)]
            public float _epsilonLeftLittle;

            [Header("Compression Error Right Hand")]
            [Range(0.0f, 1.0f)]
            public float _epsilonRightThumb;
            [Range(0.0f, 1.0f)]
            public float _epsilonRightIndex;
            [Range(0.0f, 1.0f)]
            public float _epsilonRightMiddle;
            [Range(0.0f, 1.0f)]
            public float _epsilonRightRing;
            [Range(0.0f, 1.0f)]
            public float _epsilonRightLittle;

            public float GetEpsilonFromBone(HumanBodyBones boneID)
            {
                switch (boneID)
                {
                    //Case Spine
                    case HumanBodyBones.Hips:
                    case HumanBodyBones.Spine:
                    case HumanBodyBones.Chest:
                    case HumanBodyBones.UpperChest:
                    case HumanBodyBones.Neck:
                    case HumanBodyBones.Head:
                        return _epsilonSpine;

                    //Case Left Arm
                    case HumanBodyBones.LeftShoulder:
                    case HumanBodyBones.LeftUpperArm:
                    case HumanBodyBones.LeftLowerArm:
                    case HumanBodyBones.LeftHand:
                        return _epsilonLeftArm;

                    //Case Right Arm
                    case HumanBodyBones.RightShoulder:
                    case HumanBodyBones.RightUpperArm:
                    case HumanBodyBones.RightLowerArm:
                    case HumanBodyBones.RightHand:
                        return _epsilonRightArm;

                    //Case Left Leg
                    case HumanBodyBones.LeftUpperLeg:
                    case HumanBodyBones.LeftLowerLeg:
                    case HumanBodyBones.LeftFoot:
                        return _epsilonLeftLeg;

                    //Case Right Leg
                    case HumanBodyBones.RightUpperLeg:
                    case HumanBodyBones.RightLowerLeg:
                    case HumanBodyBones.RightFoot:
                        return _epsilonRightLeg;

                    //Case Left Thumb
                    case HumanBodyBones.LeftThumbProximal:
                    case HumanBodyBones.LeftThumbIntermediate:
                    case HumanBodyBones.LeftThumbDistal:
                        return _epsilonLeftThumb;

                    //Case Left Index
                    case HumanBodyBones.LeftIndexProximal:
                    case HumanBodyBones.LeftIndexIntermediate:
                    case HumanBodyBones.LeftIndexDistal:
                        return _epsilonLeftIndex;

                    //Case Left Middle
                    case HumanBodyBones.LeftMiddleProximal:
                    case HumanBodyBones.LeftMiddleIntermediate:
                    case HumanBodyBones.LeftMiddleDistal:
                        return _epsilonLeftMiddle;

                    //Case Left Ring
                    case HumanBodyBones.LeftRingProximal:
                    case HumanBodyBones.LeftRingIntermediate:
                    case HumanBodyBones.LeftRingDistal:
                        return _epsilonLeftRing;

                    //Case Left Little
                    case HumanBodyBones.LeftLittleProximal:
                    case HumanBodyBones.LeftLittleIntermediate:
                    case HumanBodyBones.LeftLittleDistal:
                        return _epsilonLeftLittle;

                    //Case Right Thumb
                    case HumanBodyBones.RightThumbProximal:
                    case HumanBodyBones.RightThumbIntermediate:
                    case HumanBodyBones.RightThumbDistal:
                        return _epsilonRightThumb;

                    //Case Right Index
                    case HumanBodyBones.RightIndexProximal:
                    case HumanBodyBones.RightIndexIntermediate:
                    case HumanBodyBones.RightIndexDistal:
                        return _epsilonRightIndex;

                    //Case Right Middle
                    case HumanBodyBones.RightMiddleProximal:
                    case HumanBodyBones.RightMiddleIntermediate:
                    case HumanBodyBones.RightMiddleDistal:
                        return _epsilonRightMiddle;

                    //Case Right Ring
                    case HumanBodyBones.RightRingProximal:
                    case HumanBodyBones.RightRingIntermediate:
                    case HumanBodyBones.RightRingDistal:
                        return _epsilonRightRing;

                    //Case Right Little
                    case HumanBodyBones.RightLittleProximal:
                    case HumanBodyBones.RightLittleIntermediate:
                    case HumanBodyBones.RightLittleDistal:
                        return _epsilonRightLittle;
                }

                return -1;
            }
                
        }

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

        public const string CURVE_LEFT_FOOT_IK_GOAL_POSITION = "LeftFootIKGoalPos";
        public const string CURVE_LEFT_FOOT_IK_GOAL_ROTATION = "LeftFootIKGoalRot";
        public const string CURVE_RIGHT_FOOT_IK_GOAL_POSITION = "RightFootIKGoalPos";
        public const string CURVE_RIGHT_FOOT_IK_GOAL_ROTATION = "RightFootIKGoalRot";

        public const float DEFAULT_COMPRESSION_ERROR = 0.001f;

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

        public virtual void SetAnimationCurve(string curveName, QuickAnimationCurve curve)
        {
            _curves[curveName] = curve;

            //Update _timeLength if necessary
            foreach (AnimationCurve c in curve.GetAnimationCurves())
            {
                for (int i = 0; i < c.length; i++)
                {
                    float t = c.keys[i].time;
                    if (t > _timeLength)
                    {
                        _timeLength = t;
                    }
                }
            }
        }

        public virtual void AddKey(float time, bool forceAdd = false)
        {
            GetAnimationCurve(CURVE_TRANSFORM_POSITION).AddKey(time, _animator.transform.position, forceAdd);
            GetAnimationCurve(CURVE_TRANSFORM_ROTATION).AddKey(time, _animator.transform.rotation, forceAdd);

            if (_animator.isHuman)
            {
                QuickHumanPoseHandler.GetHumanPose(_animator, ref _pose);
                Vector3 bodyPosition = _pose.bodyPosition;
                Quaternion bodyRotation = _pose.bodyRotation;

                GetAnimationCurve(CURVE_BODY_POSITION).AddKey(time, bodyPosition, forceAdd);
                GetAnimationCurve(CURVE_BODY_ROTATION).AddKey(time, bodyRotation, forceAdd);

                Vector3 ikGoalPos;
                Quaternion ikGoalRot;
                _animator.GetIKGoalFromBodyPose(AvatarIKGoal.LeftFoot, bodyPosition, bodyRotation, out ikGoalPos, out ikGoalRot);
                GetAnimationCurve(CURVE_LEFT_FOOT_IK_GOAL_POSITION).AddKey(time, ikGoalPos, forceAdd);
                GetAnimationCurve(CURVE_LEFT_FOOT_IK_GOAL_ROTATION).AddKey(time, ikGoalRot, forceAdd);

                _animator.GetIKGoalFromBodyPose(AvatarIKGoal.RightFoot, bodyPosition, bodyRotation, out ikGoalPos, out ikGoalRot);
                GetAnimationCurve(CURVE_RIGHT_FOOT_IK_GOAL_POSITION).AddKey(time, ikGoalPos, forceAdd);
                GetAnimationCurve(CURVE_RIGHT_FOOT_IK_GOAL_ROTATION).AddKey(time, ikGoalRot, forceAdd);

                for (int i = 0; i < _pose.muscles.Length; i++)
                {
                    string muscleName = QuickHumanTrait.GetMuscleName(i);
                    GetAnimationCurve(muscleName).AddKey(time, _pose.muscles[i], forceAdd);
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