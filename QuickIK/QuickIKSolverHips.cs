using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;

using System;

namespace QuickVR
{

    public struct QuickIKSolverHipsJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

        public ReadWriteTransformHandle _hips;
        public ReadOnlyTransformHandle _ikTargetHips;

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

            //Update the bodyPosition to match the hipsTarget
            Vector3 offset = _ikTargetHips.GetPosition(stream) - _hips.GetPosition(stream);
            hStream.bodyPosition += offset;
        }

    }

    [System.Serializable]
    public struct QuickIKSolverHipsJobData : IAnimationJobData
    {

        [SyncSceneToStream] public Transform _ikTargetHips;

        public bool IsValid()
        {
            return _ikTargetHips != null;
        }

        public void SetDefaultValues()
        {
            _ikTargetHips = null;
        }

    }

    public class QuickIKSolverHipsBinder : AnimationJobBinder<QuickIKSolverHipsJob, QuickIKSolverHipsJobData>
    {

        public override QuickIKSolverHipsJob Create(Animator animator, ref QuickIKSolverHipsJobData data, Component component)
        {
            QuickIKSolverHipsJob job = new QuickIKSolverHipsJob();
            job._hips = ReadWriteTransformHandle.Bind(animator, animator.GetBoneTransform(HumanBodyBones.Hips));
            job._ikTargetHips = ReadOnlyTransformHandle.Bind(animator, data._ikTargetHips);

            return job;
        }

        public override void Destroy(QuickIKSolverHipsJob job)
        {

        }
    }

    public class QuickIKSolverHips : RigConstraint<QuickIKSolverHipsJob, QuickIKSolverHipsJobData, QuickIKSolverHipsBinder>
    {

    }

}


