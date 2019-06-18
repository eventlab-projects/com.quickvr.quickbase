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
        public float _posWeight;
        public float _rotWeight;

        public QuickIKTarget(ReadOnlyTransformHandle handle, float pWeight, float rWeight)
        {
            _handle = handle;
            _posWeight = pWeight;
            _rotWeight = rWeight;
        }

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
            hStream.SetGoalWeightPosition(ikGoal, ikTarget._posWeight);

            hStream.SetGoalRotation(ikGoal, ikTarget._handle.GetRotation(stream));
            hStream.SetGoalWeightRotation(ikGoal, ikTarget._rotWeight);
        }
    }

    [System.Serializable]
    public struct QuickIKSolverHumanoidJobData : IAnimationJobData
    {

        [SyncSceneToStream]
        public Transform _ikTargetHips;

        [Header("Left Hand IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetLeftHand;
        [Range(0.0f, 1.0f)] public float _posWeightLeftHand;
        [Range(0.0f, 1.0f)] public float _rotWeightLeftHand;

        [Header("Right Hand IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetRightHand;
        [Range(0.0f, 1.0f)] public float _posWeightRightHand;
        [Range(0.0f, 1.0f)] public float _rotWeightRightHand;

        [Header("Left Foot IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetLeftFoot;
        [Range(0.0f, 1.0f)] public float _posWeightLeftFoot;
        [Range(0.0f, 1.0f)] public float _rotWeightLeftFoot;

        [Header("Right Foot IK Solver")]
        [SyncSceneToStream] public Transform _ikTargetRightFoot;
        [Range(0.0f, 1.0f)] public float _posWeightRightFoot;
        [Range(0.0f, 1.0f)] public float _rotWeightRightFoot;

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

            job._ikTargetLeftHand = new QuickIKTarget(ReadOnlyTransformHandle.Bind(animator, data._ikTargetLeftHand), data._posWeightLeftHand, data._rotWeightLeftHand);
            job._ikTargetRightHand = new QuickIKTarget(ReadOnlyTransformHandle.Bind(animator, data._ikTargetRightHand), data._posWeightRightHand, data._rotWeightRightHand);
            job._ikTargetLeftFoot = new QuickIKTarget(ReadOnlyTransformHandle.Bind(animator, data._ikTargetLeftFoot), data._posWeightLeftFoot, data._rotWeightLeftFoot);
            job._ikTargetRightFoot = new QuickIKTarget(ReadOnlyTransformHandle.Bind(animator, data._ikTargetRightFoot), data._posWeightRightFoot, data._rotWeightRightFoot);

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


