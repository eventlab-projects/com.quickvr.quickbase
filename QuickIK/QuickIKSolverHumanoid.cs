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

        public ReadWriteTransformHandle _constrained;
        public ReadOnlyTransformHandle _source;

        public ReadOnlyTransformHandle _ikTargetLeftHand;
        public ReadOnlyTransformHandle _ikTargetRightHand;
        public ReadOnlyTransformHandle _ikTargetLeftFoot;
        public ReadOnlyTransformHandle _ikTargetRightFoot;

        public FloatProperty jobWeight { get; set; }

        #endregion

        public void ProcessRootMotion(AnimationStream stream)
        {

        }

        public void ProcessAnimation(AnimationStream stream)
        {
            AnimationHumanStream hStream = stream.AsHuman();

            //1) Update the bodyPosition to match the hipsTarget
            Vector3 offset = _source.GetPosition(stream) - _constrained.GetPosition(stream);
            hStream.bodyPosition += offset;

            //2) Configure the IK Solver
            hStream.SetGoalPosition(AvatarIKGoal.LeftHand, _ikTargetLeftHand.GetPosition(stream));
            hStream.SetGoalWeightPosition(AvatarIKGoal.LeftHand, 1.0f);

            hStream.SetGoalPosition(AvatarIKGoal.RightHand, _ikTargetRightHand.GetPosition(stream));
            hStream.SetGoalWeightPosition(AvatarIKGoal.RightHand, 1.0f);

            hStream.SetGoalPosition(AvatarIKGoal.LeftFoot, _ikTargetLeftFoot.GetPosition(stream));
            hStream.SetGoalWeightPosition(AvatarIKGoal.LeftFoot, 1.0f);

            hStream.SetGoalPosition(AvatarIKGoal.RightFoot, _ikTargetRightFoot.GetPosition(stream));
            hStream.SetGoalWeightPosition(AvatarIKGoal.RightFoot, 1.0f);

            hStream.SolveIK();

            //hStream.ResetToStancePose();
            //Debug.Log(stream.isHumanStream);
        }
    }

    [System.Serializable]
    public struct QuickIKSolverHumanoidJobData : IAnimationJobData
    {

        public Transform _constrainedObject;

        [SyncSceneToStream]
        public Transform _sourceObject;

        [SyncSceneToStream]
        public Transform _ikTargetLeftHand;

        [SyncSceneToStream]
        public Transform _ikTargetRightHand;

        [SyncSceneToStream]
        public Transform _ikTargetLeftFoot;

        [SyncSceneToStream]
        public Transform _ikTargetRightFoot;

        public bool IsValid()
        {
            return _constrainedObject && _sourceObject;
        }

        public void SetDefaultValues()
        {
            _constrainedObject = null;
            _sourceObject = null;

            _ikTargetLeftHand = null;
            _ikTargetRightHand = null;
            _ikTargetLeftFoot = null;
            _ikTargetRightFoot = null;
        }

    }

    public class QuickIKSolverHumanoidBinder : AnimationJobBinder<QuickIKSolverHumanoidJob, QuickIKSolverHumanoidJobData>
    {

        public override QuickIKSolverHumanoidJob Create(Animator animator, ref QuickIKSolverHumanoidJobData data, Component component)
        {
            QuickIKSolverHumanoidJob job = new QuickIKSolverHumanoidJob();
            job._constrained = ReadWriteTransformHandle.Bind(animator, data._constrainedObject);
            job._source = ReadOnlyTransformHandle.Bind(animator, data._sourceObject);

            job._ikTargetLeftHand = ReadOnlyTransformHandle.Bind(animator, data._ikTargetLeftHand);
            job._ikTargetRightHand = ReadOnlyTransformHandle.Bind(animator, data._ikTargetRightHand);
            job._ikTargetLeftFoot = ReadOnlyTransformHandle.Bind(animator, data._ikTargetLeftFoot);
            job._ikTargetRightFoot = ReadOnlyTransformHandle.Bind(animator, data._ikTargetRightFoot);

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


