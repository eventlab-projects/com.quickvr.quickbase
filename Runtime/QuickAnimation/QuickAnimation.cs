using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace QuickVR
{

    [System.Serializable]
    public class QuickAnimationKeyframe
    {
        public float _time = 0;
        public int[] _curveMask = { 0, 0, 0, 0, 0, 0, 0, 0 };

        public virtual bool IsEmpty()
        {
            foreach (uint i in _curveMask)
            {
                if (i != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void AddCurve(int curveID)
        {
            int maskID = Mathf.FloorToInt(curveID / 32.0f);
            int d = curveID % 32;
            _curveMask[maskID] |= 1 << d;
        }

        public virtual bool HasCurve(int curveID)
        {
            int maskID = Mathf.FloorToInt(curveID / 32.0f);
            int d = curveID % 32;
            return (_curveMask[maskID] & (1 << d)) != 0;
        }

        public override string ToString()
        {
            string result = "";

            for (int i = 0; i < _curveMask.Length; i++)
            {
                result = System.Convert.ToString(_curveMask[i], 2) + " " + result;
            }

            return result;
        }

    }

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

        protected List<QuickAnimationKeyframe> _keyFrames = new List<QuickAnimationKeyframe>();

        protected Dictionary<string, int> _toCurveID = new Dictionary<string, int>();
        protected Dictionary<int, string> _toCurveName = new Dictionary<int, string>();
        protected int _currentCurveID = 0;

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
            InitHelpers();
        }

        protected virtual void InitHelpers()
        {
            AddCurveID(CURVE_TRANSFORM_POSITION);
            AddCurveID(CURVE_TRANSFORM_ROTATION);

            AddCurveID(CURVE_BODY_POSITION);
            AddCurveID(CURVE_BODY_ROTATION);

            AddCurveID(CURVE_LEFT_FOOT_IK_GOAL_POSITION);
            AddCurveID(CURVE_LEFT_FOOT_IK_GOAL_ROTATION);

            AddCurveID(CURVE_RIGHT_FOOT_IK_GOAL_POSITION);
            AddCurveID(CURVE_RIGHT_FOOT_IK_GOAL_ROTATION);

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                AddCurveID(QuickHumanTrait.GetMuscleName(i));
            }
        }

        protected virtual void AddCurveID(string curveName)
        {
            int curveID = _currentCurveID++;
            _toCurveID[curveName] = curveID;
            _toCurveName[curveID] = curveName;
        }

        #endregion

        #region GET AND SET

        public virtual int GetCurveID(string curveName)
        {
            return _toCurveID[curveName];
        }

        public virtual string GetCurveName(int curveID)
        {
            return _toCurveName[curveID];
        }

        public List<QuickAnimationKeyframe> GetKeyframes()
        {
            return _keyFrames;
        }

        public virtual QuickAnimationCurve[] GetAnimationCurves()
        {
            return _curves.Values.ToArray();
        }

        public virtual int GetNumCurves()
        {
            return _curves.Count;
        }

        public virtual QuickAnimationCurve GetAnimationCurve(string curveName)
        {
            if (!_curves.ContainsKey(curveName))
            {
                _curves[curveName] = new QuickAnimationCurve(curveName);
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
            QuickAnimationKeyframe kFrame = new QuickAnimationKeyframe();
            kFrame._time = time;

            AddKey(CURVE_TRANSFORM_POSITION, time, _animator.transform.position, forceAdd, kFrame);
            AddKey(CURVE_TRANSFORM_ROTATION, time, _animator.transform.rotation, forceAdd, kFrame);

            if (_animator.isHuman)
            {
                QuickHumanPoseHandler.GetHumanPose(_animator, ref _pose);
                Vector3 bodyPosition = _pose.bodyPosition;
                Quaternion bodyRotation = _pose.bodyRotation;

                AddKey(CURVE_BODY_POSITION, time, bodyPosition, forceAdd, kFrame);
                AddKey(CURVE_BODY_ROTATION, time, bodyRotation, forceAdd, kFrame);

                Vector3 ikGoalPos;
                Quaternion ikGoalRot;
                _animator.GetIKGoalFromBodyPose(AvatarIKGoal.LeftFoot, bodyPosition, bodyRotation, out ikGoalPos, out ikGoalRot);
                AddKey(CURVE_LEFT_FOOT_IK_GOAL_POSITION, time, ikGoalPos, forceAdd, kFrame);
                AddKey(CURVE_LEFT_FOOT_IK_GOAL_ROTATION, time, ikGoalRot, forceAdd, kFrame);

                _animator.GetIKGoalFromBodyPose(AvatarIKGoal.RightFoot, bodyPosition, bodyRotation, out ikGoalPos, out ikGoalRot);
                AddKey(CURVE_RIGHT_FOOT_IK_GOAL_POSITION, time, ikGoalPos, forceAdd, kFrame);
                AddKey(CURVE_RIGHT_FOOT_IK_GOAL_ROTATION, time, ikGoalRot, forceAdd, kFrame);

                for (int i = 0; i < _pose.muscles.Length; i++)
                {
                    string muscleName = QuickHumanTrait.GetMuscleName(i);
                    AddKey(muscleName, time, _pose.muscles[i], forceAdd, kFrame);
                }
            }

            if (time > _timeLength)
            {
                _timeLength = time;
            }

            if (!kFrame.IsEmpty())
            {
                _keyFrames.Add(kFrame);
            }
        }

        protected virtual void AddKey(string curveName, float time, Vector3 value, bool forceAdd, QuickAnimationKeyframe kFrame)
        {
            if (GetAnimationCurve(curveName).AddKey(time, value, forceAdd))
            {
                kFrame.AddCurve(GetCurveID(curveName));
            }
        }

        protected virtual void AddKey(string curveName, float time, Quaternion value, bool forceAdd, QuickAnimationKeyframe kFrame)
        {
            if (GetAnimationCurve(curveName).AddKey(time, value, forceAdd))
            {
                kFrame.AddCurve(GetCurveID(curveName));
            }
        }

        protected virtual void AddKey(string curveName, float time, float value, bool forceAdd, QuickAnimationKeyframe kFrame)
        {
            if (GetAnimationCurve(curveName).AddKey(time, value, forceAdd))
            {
                kFrame.AddCurve(GetCurveID(curveName));
            }
        }

        //protected virtual void AddKey<T>(string curveName, float time, T value, bool forceAdd, QuickAnimationKeyframe kFrame)
        //{
        //    if (GetAnimationCurve(curveName).AddKey(time, value, forceAdd) != -1)
        //    {
        //        kFrame._curveMask |= 1 << GetCurveID(curveName);
        //    }
        //}

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