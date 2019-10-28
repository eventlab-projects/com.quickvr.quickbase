using UnityEngine;

using UnityEngine.Animations.Rigging;


using System;

namespace QuickVR
{

    public struct QuickIKSolverFinalStepJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

        public UnityEngine.Animations.MuscleHandle _jawMuscle;
        public float _jawMuscleValue;

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

            UnityEngine.Animations.AnimationHumanStream hStream = stream.AsHuman();
            hStream.SetMuscle(_jawMuscle, jobWeight.Get(stream) * _jawMuscleValue);

            hStream.SolveIK();
        }

    }

    [System.Serializable]
    public struct QuickIKSolverFinalStepJobData : IAnimationJobData
    {

        public bool IsValid()
        {
            return true;
        }

        public void SetDefaultValues()
        {

        }

    }

    public class QuickIKSolverFinalStepBinder : AnimationJobBinder<QuickIKSolverFinalStepJob, QuickIKSolverFinalStepJobData>
    {

        public override QuickIKSolverFinalStepJob Create(Animator animator, ref QuickIKSolverFinalStepJobData data, Component component)
        {
            QuickIKSolverFinalStepJob job = new QuickIKSolverFinalStepJob();
            job._jawMuscle = new UnityEngine.Animations.MuscleHandle(HeadDof.JawDownUp);

            HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);
            for (int i = 0; i < pose.muscles.Length; i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                if (muscleName == job._jawMuscle.name)
                {
                    job._jawMuscleValue = pose.muscles[i];
                    break;
                }
            }

            return job;
        }

        public override void Destroy(QuickIKSolverFinalStepJob job)
        {

        }
    }

    public class QuickIKSolverFinalStep : RigConstraint<QuickIKSolverFinalStepJob, QuickIKSolverFinalStepJobData, QuickIKSolverFinalStepBinder>
    {

    }

}