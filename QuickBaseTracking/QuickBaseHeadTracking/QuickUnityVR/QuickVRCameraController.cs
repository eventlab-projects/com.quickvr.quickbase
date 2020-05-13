using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickVRCameraController : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public LayerMask _visibleLayers = -1;	//The layers that will be rendered by the cameras of the head tracking system. 

        public Camera _pfCamera = null;
        public float _cameraNearPlane = DEFAULT_NEAR_CLIP_PLANE;
        public float _cameraFarPlane = DEFAULT_FAR_CLIP_PLANE;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Camera _camera = null;

        #endregion

        #region CONSTANTS

        protected const float DEFAULT_NEAR_CLIP_PLANE = 0.05f;
        protected const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _camera = _pfCamera ? Instantiate<Camera>(_pfCamera) : new GameObject().GetOrCreateComponent<Camera>();
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

        #endregion

        #region UPDATE

        public virtual void UpdateCameraPosition(Animator animator)
        {
            _camera.nearClipPlane = _cameraNearPlane;
            _camera.farClipPlane = _cameraFarPlane;
            _camera.cullingMask = _visibleLayers.value;

            //Apply the correct rotation to the cameracontrollerroot:
            Vector3 up = animator.transform.up;
            Vector3 rightCam = Vector3.ProjectOnPlane(_camera.transform.right, up).normalized;
            Vector3 r = animator.GetBoneTransform(HumanBodyBones.RightEye).position - animator.GetBoneTransform(HumanBodyBones.LeftEye).position;
            Vector3 rightHead = Vector3.ProjectOnPlane(r, up).normalized;
            float rotOffset = Vector3.SignedAngle(rightCam, rightHead, up);
            transform.Rotate(up, rotOffset, Space.World);

            //This forces the camera to be in the Avatar's eye center. 
            Vector3 offset = animator.GetEyeCenterPosition() - _camera.transform.position;
            transform.position += offset;
        }

        #endregion

    }

}
