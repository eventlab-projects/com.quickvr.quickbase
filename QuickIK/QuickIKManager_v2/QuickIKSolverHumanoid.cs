using UnityEngine;

using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;

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

        public IntProperty _avatarIKGoal;

        public FloatProperty jobWeight { get; set; }

        #endregion

        public void ProcessRootMotion(AnimationStream stream)
        {

        }

        public void ProcessAnimation(AnimationStream stream)
        {
            if (!stream.isHumanStream)
            {
                Debug.LogError("Character must be Humanoid!!!");
                return;
            }

            SetGoal(stream, (AvatarIKGoal)_avatarIKGoal.value.GetInt(stream));
        }

        private void SetGoal(AnimationStream stream, AvatarIKGoal ikGoal)
        {
            AnimationHumanStream hStream = stream.AsHuman();
            AvatarIKHint ikHint = GetIKHint(ikGoal);

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

        private AvatarIKHint GetIKHint(AvatarIKGoal ikGoal)
        {
            if (ikGoal == AvatarIKGoal.LeftHand) return AvatarIKHint.LeftElbow;
            if (ikGoal == AvatarIKGoal.RightHand) return AvatarIKHint.RightElbow;
            if (ikGoal == AvatarIKGoal.LeftFoot) return AvatarIKHint.LeftKnee;
            return AvatarIKHint.RightKnee;
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
        [SyncSceneToStream, ReadOnly] public int _avatarIKGoal;

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
            
            job._ikTarget = ReadOnlyTransformHandle.Bind(animator, data._ikTarget);
            job._posWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._posWeight)));
            job._rotWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._rotWeight)));
            job._ikTargetHint = ReadOnlyTransformHandle.Bind(animator, data._ikTargetHint);
            job._posWeightHint = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._posWeightHint)));
            job._avatarIKGoal = IntProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(nameof(data._avatarIKGoal)));

            return job;
        }

        public override void Destroy(QuickIKSolverHumanoidJob job)
        {

        }
    }

    public class QuickIKSolverHumanoid : RigConstraint<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData, QuickIKSolverHumanoidBinder>, IQuickIKSolver
    {

        protected Animator _animator
        {
            get
            {
                return GetComponentInParent<Animator>();
            }
        }

        public Transform _boneUpper
        {
            get
            {
                AvatarIKGoal ikGoal = (AvatarIKGoal)data._avatarIKGoal;
                if (ikGoal == AvatarIKGoal.LeftHand) return _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                if (ikGoal == AvatarIKGoal.RightHand) return _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                if (ikGoal == AvatarIKGoal.LeftFoot) return _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                return _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            }
        }

        public Transform _boneMid
        {
            get
            {
                AvatarIKGoal ikGoal = (AvatarIKGoal)data._avatarIKGoal;
                if (ikGoal == AvatarIKGoal.LeftHand) return _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                if (ikGoal == AvatarIKGoal.RightHand) return _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                if (ikGoal == AvatarIKGoal.LeftFoot) return _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                return _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            }
        }

        public Transform _boneLimb
        {
            get
            {
                AvatarIKGoal ikGoal = (AvatarIKGoal)data._avatarIKGoal;
                if (ikGoal == AvatarIKGoal.LeftHand) return _animator.GetBoneTransform(HumanBodyBones.LeftHand);
                if (ikGoal == AvatarIKGoal.RightHand) return _animator.GetBoneTransform(HumanBodyBones.RightHand);
                if (ikGoal == AvatarIKGoal.LeftFoot) return _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                return _animator.GetBoneTransform(HumanBodyBones.RightFoot);
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

    }

}


