using UnityEngine;

using UnityEngine.Animations.Rigging;


using System;

namespace QuickVR
{

    public struct QuickCopyTransformConstraintJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

        public ReadWriteTransformHandle _dstTransform;
        public ReadOnlyTransformHandle _srcTransform;

        public FloatProperty jobWeight { get; set; }

        #endregion

        public void ProcessRootMotion(UnityEngine.Animations.AnimationStream stream)
        {

        }

        public void ProcessAnimation(UnityEngine.Animations.AnimationStream stream)
        {
            _dstTransform.SetPosition(stream, _srcTransform.GetPosition(stream));
            _dstTransform.SetRotation(stream, _srcTransform.GetRotation(stream));
        }

    }

    [System.Serializable]
    public struct QuickCopyTransformConstraintJobData : IAnimationJobData
    {

        [SyncSceneToStream] public Transform _dstTransform;
        [SyncSceneToStream] public Transform _srcTransform;

        public bool IsValid()
        {
            return _dstTransform != null && _srcTransform != null;
        }

        public void SetDefaultValues()
        {
            _dstTransform = null;
            _srcTransform = null;
        }

    }

    public class QuickCopyTransformConstraintBinder : AnimationJobBinder<QuickCopyTransformConstraintJob, QuickCopyTransformConstraintJobData>
    {

        public override QuickCopyTransformConstraintJob Create(Animator animator, ref QuickCopyTransformConstraintJobData data, Component component)
        {
            QuickCopyTransformConstraintJob job = new QuickCopyTransformConstraintJob();

            job._dstTransform = ReadWriteTransformHandle.Bind(animator, data._dstTransform);
            job._srcTransform = ReadOnlyTransformHandle.Bind(animator, data._srcTransform);

            return job;
        }

        public override void Destroy(QuickCopyTransformConstraintJob job)
        {

        }
    }

    public class QuickCopyTransformConstraint : RigConstraint<QuickCopyTransformConstraintJob, QuickCopyTransformConstraintJobData, QuickCopyTransformConstraintBinder>
    {

    }

}


