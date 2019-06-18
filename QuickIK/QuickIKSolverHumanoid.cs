using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;

using System;

namespace QuickVR
{

    public struct QuickIKTarget
    {
        public ReadOnlyTransformHandle _handle;
        public FloatProperty _posWeight;
        public FloatProperty _rotWeight;
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

            hStream.SetGoalPosition(ikGoal, ikTarget._handle.GetPosition(stream));
            hStream.SetGoalWeightPosition(ikGoal, ikTarget._posWeight.value.GetFloat(stream));

            hStream.SetGoalRotation(ikGoal, ikTarget._handle.GetRotation(stream));
            hStream.SetGoalWeightRotation(ikGoal, ikTarget._rotWeight.value.GetFloat(stream));
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

        [Header("Right Hand IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetRightHand;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightRightHand;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightRightHand;

        [Header("Left Foot IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetLeftFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightLeftFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightLeftFoot;

        [Header("Right Foot IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetRightFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _posWeightRightFoot;
        [SyncSceneToStream, Range(0.0f, 1.0f)] public float _rotWeightRightFoot;

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

            _ikTargetRightHand = null;
            _posWeightRightHand = 1.0f;
            _rotWeightRightHand = 1.0f;

            _ikTargetLeftFoot = null;
            _posWeightLeftFoot = 1.0f;
            _rotWeightLeftFoot = 1.0f;

            _ikTargetRightFoot = null;
            _posWeightRightFoot = 1.0f;
            _rotWeightRightFoot = 1.0f;
        }

    }

    public class QuickIKSolverHumanoidBinder : AnimationJobBinder<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData>
    {

        public override QuickIKSolverHumanoidJob Create(Animator animator, ref QuickIKSolverHumanoidJobData data, Component component)
        {
            QuickIKSolverHumanoidJob job = new QuickIKSolverHumanoidJob();
            job._hips = ReadWriteTransformHandle.Bind(animator, animator.GetBoneTransform(HumanBodyBones.Hips));
            job._ikTargetHips = ReadOnlyTransformHandle.Bind(animator, data._ikTargetHips);

            job._ikTargetLeftHand = CreateQuickIKTarget(animator, component, data._ikTargetLeftHand, data._posWeightLeftHand, nameof(data._posWeightLeftHand), data._rotWeightLeftHand, nameof(data._rotWeightLeftHand));
            job._ikTargetRightHand = CreateQuickIKTarget(animator, component, data._ikTargetRightHand, data._posWeightRightHand, nameof(data._posWeightRightHand), data._rotWeightRightHand, nameof(data._rotWeightRightHand));
            job._ikTargetLeftFoot = CreateQuickIKTarget(animator, component, data._ikTargetLeftFoot, data._posWeightLeftFoot, nameof(data._posWeightLeftFoot), data._rotWeightLeftFoot, nameof(data._rotWeightLeftFoot));
            job._ikTargetRightFoot = CreateQuickIKTarget(animator, component, data._ikTargetRightFoot, data._posWeightRightFoot, nameof(data._posWeightRightFoot), data._rotWeightRightFoot, nameof(data._rotWeightRightFoot));

            return job;
        }

        protected virtual QuickIKTarget CreateQuickIKTarget(Animator animator, Component component, Transform target, float posWeight, string pwName, float rotWeight, string rwName)
        {
            QuickIKTarget ikTargetData = new QuickIKTarget();
            ikTargetData._handle = ReadOnlyTransformHandle.Bind(animator, target);
            ikTargetData._posWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(pwName));
            ikTargetData._rotWeight = FloatProperty.Bind(animator, component, PropertyUtils.ConstructConstraintDataPropertyName(rwName));

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


