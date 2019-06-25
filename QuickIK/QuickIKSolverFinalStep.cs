using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;

using System;

namespace QuickVR
{

    public struct QuickIKSolverFinalStepJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

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

            stream.AsHuman().SolveIK();
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
            return new QuickIKSolverFinalStepJob();
        }

        public override void Destroy(QuickIKSolverFinalStepJob job)
        {

        }
    }

    public class QuickIKSolverFinalStep : RigConstraint<QuickIKSolverFinalStepJob, QuickIKSolverFinalStepJobData, QuickIKSolverFinalStepBinder>
    {

    }

}