using UnityEngine;

using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;

using System;

namespace QuickVR
{

    public struct QuickIKSolverHeadJob : IWeightedAnimationJob
    {

        #region PUBLIC ATTRIBUTES

        public ReadWriteTransformHandle _animatorTransform;

        public ReadWriteTransformHandle _head;
        public ReadWriteTransformHandle _hips;
        public ReadOnlyTransformHandle _ikTargetHead;

        public Quaternion _initialHeadLocalRotation;

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
            Vector3 u = _head.GetPosition(stream) - _hips.GetPosition(stream);
            Vector3 v = _ikTargetHead.GetPosition(stream) - _hips.GetPosition(stream);
            _hips.SetRotation(stream, Quaternion.FromToRotation(u, v) * _hips.GetRotation(stream));

            Quaternion tmp = _animatorTransform.GetRotation(stream);
            _animatorTransform.SetRotation(stream, Quaternion.identity);
            _head.SetLocalRotation(stream, _initialHeadLocalRotation);
            _head.SetRotation(stream, _ikTargetHead.GetRotation(stream) * _head.GetRotation(stream));
            _animatorTransform.SetRotation(stream, tmp);
        }
    }

    [System.Serializable]
    public struct QuickIKSolverHeadJobData : IAnimationJobData
    {

        [SyncSceneToStream] public Transform _ikTargetHead;

        public bool IsValid()
        {
            return _ikTargetHead != null;
        }

        public void SetDefaultValues()
        {
            _ikTargetHead = null;
        }

    }

    public class QuickIKSolverHeadBinder : AnimationJobBinder<QuickIKSolverHeadJob, QuickIKSolverHeadJobData>
    {

        public override QuickIKSolverHeadJob Create(Animator animator, ref QuickIKSolverHeadJobData data, Component component)
        {
            QuickIKSolverHeadJob job = new QuickIKSolverHeadJob();
            job._animatorTransform = ReadWriteTransformHandle.Bind(animator, animator.transform);
            job._head = ReadWriteTransformHandle.Bind(animator, animator.GetBoneTransform(HumanBodyBones.Head));
            job._hips = ReadWriteTransformHandle.Bind(animator, animator.GetBoneTransform(HumanBodyBones.Hips));
            job._ikTargetHead = ReadOnlyTransformHandle.Bind(animator, data._ikTargetHead);

            job._initialHeadLocalRotation = animator.GetBoneTransform(HumanBodyBones.Head).localRotation;


            return job;
        }

        public override void Destroy(QuickIKSolverHeadJob job)
        {

        }
    }

    public class QuickIKSolverHead : RigConstraint<QuickIKSolverHeadJob, QuickIKSolverHeadJobData, QuickIKSolverHeadBinder>, IQuickIKSolver
    {

        protected Animator _animator
        {
            get
            {
                return GetComponentInParent<Animator>();
            }
        }

        public HumanBodyBones _boneID
        {
            get
            {
                return HumanBodyBones.Head;
            }
            set
            {

            }
        }

        public Transform _boneUpper
        {
            get
            {
                return _animator.GetBoneTransform(HumanBodyBones.Head);
            }
            set
            {

            }
        }

        public Transform _boneMid
        {
            get
            {
                return _animator.GetBoneTransform(HumanBodyBones.Hips);
            }
            set
            {

            }
        }

        public Transform _boneLimb
        {
            get
            {
                return _animator.GetBoneTransform(HumanBodyBones.Head);
            }
            set
            {

            }
        }

        public Transform _targetLimb
        {
            get
            {
                return data._ikTargetHead;
            }
            set
            {
                data._ikTargetHead = value;
            }
        }

        public Transform _targetHint
        {
            get
            {
                return null;
            }
            set
            {

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
                return 1.0f;
            }
            set
            {
                
            }
        }

        public float _weightIKRot
        {
            get
            {
                return 1.0f;
            }
            set
            {

            }
        }

        public float _weightIKHint
        {
            get
            {
                return 0.0f;
            }
            set
            {

            }
        }

    }

}


