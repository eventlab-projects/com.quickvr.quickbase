using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;

using System;

namespace QuickVR
{

    [System.Serializable]
    public struct QuickIKSolverData
    {
        public Transform _ikTarget;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeight;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeight;
        [SyncSceneToStream] public Transform _hint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightHint;
    }

    public struct QuickIKTarget
    {
        public ReadOnlyTransformHandle _handle;
        public FloatProperty _posWeight;
        public FloatProperty _rotWeight;
        public ReadOnlyTransformHandle _handleHint;
        public FloatProperty _posWeightHint;
    }

    public struct QuickIKSolverHumanoidJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

        public ReadWriteTransformHandle _hips;
        public ReadOnlyTransformHandle _ikTargetHips;

        public QuickIKTarget _ikTargetLeftHand;
        public QuickIKTarget _ikTargetRightHand;
        public QuickIKTarget _ikTargetLeftFoot;
        public QuickIKTarget _ikTargetRightFoot;
        
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

            AnimationHumanStream hStream = stream.AsHuman();

            //1) Update the bodyPosition to match the hipsTarget
            Vector3 offset = _ikTargetHips.GetPosition(stream) - _hips.GetPosition(stream);
            hStream.bodyPosition += offset;

            //2) Configure the IK Solver
            SetGoal(stream, AvatarIKGoal.LeftHand, _ikTargetLeftHand);
            SetGoal(stream, AvatarIKGoal.RightHand, _ikTargetRightHand);
            SetGoal(stream, AvatarIKGoal.LeftFoot, _ikTargetLeftFoot);
            SetGoal(stream, AvatarIKGoal.RightFoot, _ikTargetRightFoot);

            hStream.SolveIK();
        }

        private void SetGoal(AnimationStream stream, AvatarIKGoal ikGoal, QuickIKTarget ikTarget)
        {
            AnimationHumanStream hStream = stream.AsHuman();
            AvatarIKHint ikHint = GetIKHint(ikGoal);

            if (ikTarget._handle.IsValid(stream))
            {
                hStream.SetGoalPosition(ikGoal, ikTarget._handle.GetPosition(stream));
                hStream.SetGoalWeightPosition(ikGoal, ikTarget._posWeight.value.GetFloat(stream));

                hStream.SetGoalRotation(ikGoal, ikTarget._handle.GetRotation(stream));
                hStream.SetGoalWeightRotation(ikGoal, ikTarget._rotWeight.value.GetFloat(stream));
                
                if (ikTarget._handleHint.IsValid(stream))
                {
                    hStream.SetHintPosition(ikHint, ikTarget._handleHint.GetPosition(stream));
                    hStream.SetHintWeightPosition(ikHint, ikTarget._posWeightHint.value.GetFloat(stream));
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

        [Header("Left Hand IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetLeftHand;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightLeftHand;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightLeftHand;
        [SyncSceneToStream] public Transform _ikTargetLeftHandHint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightLeftHandHint;

        [Header("Right Hand IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetRightHand;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightRightHand;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightRightHand;
        [SyncSceneToStream] public Transform _ikTargetRightHandHint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightRightHandHint;

        [Header("Left Foot IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetLeftFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightLeftFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightLeftFoot;
        [SyncSceneToStream] public Transform _ikTargetLeftFootHint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightLeftFootHint;

        [Header("Right Foot IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetRightFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightRightFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightRightFoot;
        [SyncSceneToStream] public Transform _ikTargetRightFootHint;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightRightFootHint;

        [Header("Test")]
        public QuickIKSolverData _test;

        public bool IsValid()
        {
            return true;
        }

        public void SetDefaultValues()
        {
            _ikTargetHips = null;

            _ikTargetLeftHand = null;
            _posWeightLeftHand = 1.0f;
            _rotWeightLeftHand = 1.0f;
            _ikTargetLeftHandHint = null;
            _posWeightLeftHandHint = 0.0f;

            _ikTargetRightHand = null;
            _posWeightRightHand = 1.0f;
            _rotWeightRightHand = 1.0f;
            _ikTargetRightHandHint = null;
            _posWeightRightHandHint = 0.0f;

            _ikTargetLeftFoot = null;
            _posWeightLeftFoot = 1.0f;
            _rotWeightLeftFoot = 1.0f;
            _ikTargetLeftFootHint = null;
            _posWeightLeftFootHint = 0.0f;

            _ikTargetRightFoot = null;
            _posWeightRightFoot = 1.0f;
            _rotWeightRightFoot = 1.0f;
            _ikTargetRightFootHint = null;
            _posWeightRightFootHint = 0.0f;
        }

    }

    public class QuickIKSolverHumanoidBinder : AnimationJobBinder<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData>
    {

        public override QuickIKSolverHumanoidJob Create(Animator animator, ref QuickIKSolverHumanoidJobData data, Component component)
        {
            QuickIKSolverHumanoidJob job = new QuickIKSolverHumanoidJob();
            job._hips = ReadWriteTransformHandle.Bind(animator, animator.GetBoneTransform(HumanBodyBones.Hips));
            job._ikTargetHips = ReadOnlyTransformHandle.Bind(animator, data._ikTargetHips);

            job._ikTargetLeftHand = CreateQuickIKTarget(animator, component, data._ikTargetLeftHand, data._ikTargetLeftHandHint, nameof(data._posWeightLeftHand), nameof(data._rotWeightLeftHand), nameof(data._posWeightLeftHandHint));
            job._ikTargetRightHand = CreateQuickIKTarget(animator, component, data._ikTargetRightHand, data._ikTargetRightHandHint, nameof(data._posWeightRightHand), nameof(data._rotWeightRightHand), nameof(data._posWeightRightHandHint));
            job._ikTargetLeftFoot = CreateQuickIKTarget(animator, component, data._ikTargetLeftFoot, data._ikTargetLeftFootHint, nameof(data._posWeightLeftFoot), nameof(data._rotWeightLeftFoot), nameof(data._posWeightLeftFootHint));
            job._ikTargetRightFoot = CreateQuickIKTarget(animator, component, data._ikTargetRightFoot, data._ikTargetRightFootHint, nameof(data._posWeightRightFoot), nameof(data._rotWeightRightFoot), nameof(data._posWeightRightFootHint));

            return job;
        }

        protected virtual QuickIKTarget CreateQuickIKTarget(Animator animator, Component component, Transform target, Transform hint, string pwName, string rwName, string pwHintName)
        {
            QuickIKTarget ikTargetData = new QuickIKTarget();
            ikTargetData._handle = ReadOnlyTransformHandle.Bind(animator, target);
            ikTargetData._posWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(pwName));
            ikTargetData._rotWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(rwName));
            ikTargetData._handleHint = ReadOnlyTransformHandle.Bind(animator, hint);
            ikTargetData._posWeightHint = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(pwHintName));

            return ikTargetData;
        }

        public override void Destroy(QuickIKSolverHumanoidJob job)
        {

        }
    }

    public class QuickIKSolverHumanoid : RigConstraint<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData, QuickIKSolverHumanoidBinder>
    {

    }

}


