using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
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

            if (_ikTarget.IsValid(stream))
            {
                hStream.SetGoalPosition(ikGoal, _ikTarget.GetPosition(stream));
                hStream.SetGoalWeightPosition(ikGoal, _posWeight.value.GetFloat(stream));

                hStream.SetGoalRotation(ikGoal, _ikTarget.GetRotation(stream));
                hStream.SetGoalWeightRotation(ikGoal, _rotWeight.value.GetFloat(stream));
                
                if (_ikTargetHint.IsValid(stream))
                {
                    hStream.SetHintPosition(ikHint, _ikTargetHint.GetPosition(stream));
                    hStream.SetHintWeightPosition(ikHint, _posWeightHint.value.GetFloat(stream));
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

        [SyncSceneToStream]
        public Transform _ikTargetHips;

        [SyncSceneToStream] public Transform _ikTarget;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeight;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeight;
        [SyncSceneToStream] public Transform _ikTargetHint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightHint;
        [SyncSceneToStream, ReadOnly] public int _avatarIKGoal;

        public bool IsValid()
        {
            return _ikTarget != null;
        }

        public void SetDefaultValues()
        {
            _ikTargetHips = null;

            _ikTarget= null;
            _posWeight = 1.0f;
            _rotWeight = 1.0f;
            _ikTargetHint = null;
            _posWeightHint = 0.0f;
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

    public class QuickIKSolverHumanoid : RigConstraint<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData, QuickIKSolverHumanoidBinder>
    {

    }

}


