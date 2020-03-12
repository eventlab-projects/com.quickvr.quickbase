using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR {

    public class QuickCopyPoseBase : MonoBehaviour {

        #region PUBLIC PARAMETERS

        protected Animator _source = null;
        protected Animator _dest = null;

        #endregion

        #region PROTECTED PARAMETERS

        protected Transform _sourceOrigin = null;
        protected Transform _destOrigin = null;

        protected HumanPoseHandler _handlerSource = null;
        protected HumanPoseHandler _handlerDest = null;
        protected HumanPose _poseSource = new HumanPose();
        protected HumanPose _poseDest = new HumanPose();
        protected HumanPose _initialPoseDest = new HumanPose();

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += CopyPose;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= CopyPose;
        }

        protected virtual void Awake()
        {
            _destOrigin = transform.CreateChild("__DestOrigin__");
            _sourceOrigin = transform.CreateChild("__SourceOrigin__");
        }

        protected virtual HumanPoseHandler CreatePoseHandler(Animator animator, Transform origin)
        {
            HumanPoseHandler poseHandler = null;

            if (animator)
            {
                poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);

                origin.position = animator.transform.position;
                origin.rotation = animator.transform.rotation;
            }

            return poseHandler;
        }

        #endregion

        #region GET AND SET

        public virtual Animator GetAnimatorSource()
        {
            return _source;
        }

        public virtual Animator GetAnimatorDest()
        {
            return _dest;
        }

        public virtual void SetAnimatorSource(Animator animator)
        {
            _source = animator;
            _handlerSource = CreatePoseHandler(_source, _sourceOrigin);
        }

        public virtual void SetAnimatorDest(Animator animator)
        {
            //Restore the initial HumanPose that _dest had at the begining
            if (_dest)
            {
                _handlerDest.SetHumanPose(ref _initialPoseDest);
            }

            _dest = animator;
            _handlerDest = CreatePoseHandler(_dest, _destOrigin);

            //Save the current HumanPose of the new _dest
            if (_dest)
            {
                GetHumanPose(_dest, _handlerDest, ref _initialPoseDest);
            }
        }

        public virtual void GetHumanPose(Animator animator, HumanPoseHandler poseHandler, ref HumanPose result)
        {
            //Save the current transform properties
            Vector3 tmpPos = animator.transform.position;
            Quaternion tmpRot = animator.transform.rotation;

            //Set the transform to the world origin
            animator.transform.SetProperties(Vector3.zero, Quaternion.identity);

            //Copy the pose
            poseHandler.GetHumanPose(ref result);

            //Restore the transform properties
            animator.transform.SetProperties(tmpPos, tmpRot);
        }

        #endregion

        #region UPDATE

        protected virtual void CopyPose()
        {
            if (_source && _dest)
            {
                GetHumanPose(_source, _handlerSource, ref _poseSource);
                GetHumanPose(_dest, _handlerDest, ref _poseDest);
                _handlerDest.SetHumanPose(ref _poseSource);
            }
        }

        #endregion

    }

}
