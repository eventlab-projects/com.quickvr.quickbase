using UnityEngine;

using UnityEngine.Animations.Rigging;


using System;

namespace QuickVR
{

    public struct QuickIKSolverHumanoidJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

        public ReadOnlyTransformHandle _ikTarget;
        public FloatProperty _posWeight;
        public FloatProperty _rotWeight;
        public ReadOnlyTransformHandle _ikTargetHint;
        public FloatProperty _posWeightHint;

        public int _boneID;

        public FloatProperty jobWeight { get; set; }

        #endregion

        public void ProcessRootMotion(UnityEngine.Animations.AnimationStream stream)
        {

        }

        public void ProcessAnimation(UnityEngine.Animations.AnimationStream stream)
        {
            if (!stream.isHumanStream)
            {
                Debug.LogError("Character must be Humanoid!!!");
                return;
            }

            SetGoal(stream, QuickIKSolverHumanoid.GetAvatarIKGoal((HumanBodyBones)_boneID));
        }

        private void SetGoal(UnityEngine.Animations.AnimationStream stream, AvatarIKGoal ikGoal)
        {
            UnityEngine.Animations.AnimationHumanStream hStream = stream.AsHuman();
            AvatarIKHint ikHint = QuickIKSolverHumanoid.GetAvatarIKHint(ikGoal);

            float jWeight = jobWeight.Get(stream);

            if (_ikTarget.IsValid(stream))
            {
                hStream.SetGoalPosition(ikGoal, _ikTarget.GetPosition(stream));
                hStream.SetGoalWeightPosition(ikGoal, jWeight * _posWeight.Get(stream));

                hStream.SetGoalRotation(ikGoal, _ikTarget.GetRotation(stream));
                hStream.SetGoalWeightRotation(ikGoal, jWeight * _rotWeight.Get(stream));
                
                if (_ikTargetHint.IsValid(stream))
                {
                    hStream.SetHintPosition(ikHint, _ikTargetHint.GetPosition(stream));
                    hStream.SetHintWeightPosition(ikHint, jWeight * _posWeightHint.Get(stream));
                }
                else hStream.SetHintWeightPosition(ikHint, 0.0f);
            }
            else
            {
                hStream.SetGoalWeightPosition(ikGoal, 0.0f);
                hStream.SetGoalWeightRotation(ikGoal, 0.0f);
                hStream.SetHintWeightPosition(ikHint, 0.0f);
            }
            
        }

    }

    [System.Serializable]
    public struct QuickIKSolverHumanoidJobData : IAnimationJobData
    {
        [SyncSceneToStream] public Transform _ikTarget;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeight;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeight;
        [SyncSceneToStream] public Transform _ikTargetHint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightHint;
        [SyncSceneToStream, ReadOnly] public int _boneID;

        public bool IsValid()
        {
            return true;//_ikTarget != null;
        }

        public void SetDefaultValues()
        {
            _ikTarget= null;
            _posWeight = 1.0f;
            _rotWeight = 1.0f;
            _ikTargetHint = null;
            _posWeightHint = 1.0f;
        }

    }

    public class QuickIKSolverHumanoidBinder : AnimationJobBinder<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData>
    {

        public override QuickIKSolverHumanoidJob Create(Animator animator, ref QuickIKSolverHumanoidJobData data, Component component)
        {
            QuickIKSolverHumanoidJob job = new QuickIKSolverHumanoidJob();
            QuickIKSolverHumanoid ikSolver = (QuickIKSolverHumanoid)component;
            
            job._ikTarget = ReadOnlyTransformHandle.Bind(animator, data._ikTarget);
            job._posWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._posWeight)));
            job._rotWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._rotWeight)));
            job._ikTargetHint = ReadOnlyTransformHandle.Bind(animator, data._ikTargetHint);
            job._posWeightHint = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._posWeightHint)));
            job._boneID = (int)ikSolver._boneID; 

            return job;
        }

        public override void Destroy(QuickIKSolverHumanoidJob job)
        {

        }
    }

    public class QuickIKSolverHumanoid : RigConstraint<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData, QuickIKSolverHumanoidBinder>, IQuickIKSolver
    {

        #region PUBLIC ATTRIBUTES

        //The bone chain hierarchy
        public Transform m_boneUpper = null;
        public Transform m_boneMid = null;
        public Transform m_boneLimb = null;

        public HumanBodyBones _boneID
        {
            get
            {
                return m_boneID;
            }
            set
            {
                m_boneID = value;
            }
        }

        public Transform _boneUpper
        {
            get
            {
                return m_boneUpper;
            }
            set
            {
                m_boneUpper = value;
            }
        }

        public Transform _boneMid
        {
            get
            {
                return m_boneMid;
            }
            set
            {
                m_boneMid = value;
            }
        }

        public Transform _boneLimb
        {
            get
            {
                return m_boneLimb;
            }
            set
            {
                m_boneLimb = value;
            }
        }

        public Transform _targetLimb
        {
            get
            {
                return data._ikTarget;
            }
            set
            {
                data._ikTarget = value;
            }
        }

        public Transform _targetHint
        {
            get
            {
                return data._ikTargetHint;
            }
            set
            {
                data._ikTargetHint = value;
            }
        }

        public float _weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }

        public float _weightIKPos
        {
            get
            {
                return data._posWeight;
            }
            set
            {
                data._posWeight = value;
            }
        }

        public float _weightIKRot
        {
            get
            {
                return data._rotWeight;
            }
            set
            {
                data._rotWeight = value;
            }
        }

        public float _weightIKHint
        {
            get
            {
                return data._posWeightHint;
            }
            set
            {
                data._posWeightHint = value;
            }
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator
        {
            get
            {
                return GetComponentInParent<Animator>();
            }
        }

        protected HumanBodyBones m_boneID = HumanBodyBones.LastBone;

        #endregion

        #region GET AND SET

        public static AvatarIKGoal GetAvatarIKGoal(HumanBodyBones boneID)
        {
            return QuickUtils.ParseEnum<AvatarIKGoal>(boneID.ToString());
        }

        public static AvatarIKHint GetAvatarIKHint(HumanBodyBones boneID)
        {
            return GetAvatarIKHint(GetAvatarIKGoal(boneID));
        }

        public static AvatarIKHint GetAvatarIKHint(AvatarIKGoal ikGoal)
        {
            if (ikGoal == AvatarIKGoal.LeftHand) return AvatarIKHint.LeftElbow;
            if (ikGoal == AvatarIKGoal.RightHand) return AvatarIKHint.RightElbow;
            if (ikGoal == AvatarIKGoal.LeftFoot) return AvatarIKHint.LeftKnee;
            return AvatarIKHint.RightKnee;
        }

        #endregion

    }

}


