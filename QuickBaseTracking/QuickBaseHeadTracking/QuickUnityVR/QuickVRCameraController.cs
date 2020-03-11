using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickVRCameraController : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected Camera _camera = null;
        protected Animator _animator = null;

        #endregion

        #region CREATION AND DESTRUCTION

        public virtual void CreateCamera(Camera pfCamera)
        {
            _camera = pfCamera ? Instantiate<Camera>(pfCamera) : new GameObject().GetOrCreateComponent<Camera>();
            _camera.name = "__Camera__";
            _camera.transform.parent = transform;
            _camera.transform.ResetTransformation();
            _camera.tag = "MainCamera";
            _camera.gameObject.GetOrCreateComponent<AudioListener>();
            _camera.gameObject.GetOrCreateComponent<FlareLayer>();
        }

        #endregion

        #region GET AND SET

        public virtual Camera GetCamera()
        {
            return _camera;
        }

        public virtual void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public virtual Animator GetAnimator()
        {
            return _animator;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateCameraPosition()
        {
            //Apply the correct rotation to the cameracontrollerroot:
            Vector3 up = _animator.transform.up;
            Vector3 rightCam = Vector3.ProjectOnPlane(_camera.transform.right, up).normalized;
            Vector3 r = _animator.GetBoneTransform(HumanBodyBones.RightEye).position - _animator.GetBoneTransform(HumanBodyBones.LeftEye).position;
            Vector3 rightHead = Vector3.ProjectOnPlane(r, up).normalized;
            float rotOffset = Vector3.SignedAngle(rightCam, rightHead, up);
            transform.Rotate(up, rotOffset, Space.World);

            //This forces the camera to be in the Avatar's eye center. 
            Vector3 offset = _animator.GetEyeCenterPosition() - _camera.transform.position;
            transform.position += offset;
        }

        #endregion

    }

}
