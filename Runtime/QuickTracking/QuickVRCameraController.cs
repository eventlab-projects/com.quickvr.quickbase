using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

namespace QuickVR
{

    public class QuickVRCameraController : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public LayerMask _visibleLayers = -1;	//The layers that will be rendered by the cameras of the head tracking system. 

        public Camera _pfCamera = null;
        public float _cameraNearPlane = DEFAULT_NEAR_CLIP_PLANE;
        public float _cameraFarPlane = DEFAULT_FAR_CLIP_PLANE;

        [Header("Camera Mono Options:")]
        public bool _rotateCamera = true;
        public float _speedH = 2.0f;
        public float _speedV = 2.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animatorSource = null;
        
        protected Transform _head
        {
            get
            {
                return _animatorSource.GetBoneTransform(HumanBodyBones.Head);
            }
        }

        protected static Camera _camera = null;
        
        protected float _offsetH = 0;
        protected float _offsetV = 0;
        protected Quaternion _initialLocalRotationHead = Quaternion.identity;

        #endregion

        #region CONSTANTS

        public const float DEFAULT_NEAR_CLIP_PLANE = 0.05f;
        public const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

        //Rotation limits for CameraMono
        const float MAX_HORIZONTAL_ANGLE = 80;
        const float MAX_VERTICAL_ANGLE = 45;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            if (!Camera.main)
            {
                Camera camera = _pfCamera ? Instantiate<Camera>(_pfCamera) : new GameObject().GetOrCreateComponent<Camera>();
                camera.name = "__Camera__";
                camera.tag = "MainCamera";
                camera.gameObject.GetOrCreateComponent<FlareLayer>();
            }

            _camera = Camera.main;
            _camera.transform.parent = transform;
            _camera.transform.ResetTransformation();
            _camera.GetOrCreateComponent<AudioListener>();

            if (!QuickVRManager.IsXREnabled())
            {
                _camera.fieldOfView = 70.0f;//90.0f;
            }

            QuickUICursor.CreateVRCursor(QuickUICursor.Role.Head, _camera.transform);
        }

        #endregion

        #region GET AND SET

        public static Camera GetCamera()
        {
            return _camera;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateCameraPosition(Animator animator)
        {
            foreach (Camera cam in Camera.allCameras)
            {
                cam.nearClipPlane = _cameraNearPlane;
                cam.farClipPlane = _cameraFarPlane;
                cam.cullingMask = _visibleLayers.value;
            }

            UpdateCameraRotation();
            
            if (animator)
            {
                Transform tEyeCenter = animator.GetEyeCenter();
                if (transform.parent != animator.transform)
                {
                    transform.parent = animator.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }

                //Apply the correct rotation to the cameracontrollerroot:
                Vector3 up = animator.transform.up;
                Vector3 rightCam = Vector3.ProjectOnPlane(_camera.transform.right, up);
                Vector3 rightHead = Vector3.ProjectOnPlane(tEyeCenter.right, up);

                float rotOffset = Vector3.SignedAngle(rightCam, rightHead, up);
                transform.Rotate(up, rotOffset, Space.World);

                //This forces the camera to be in the Avatar's eye center. 
                Vector3 offset = tEyeCenter.position - _camera.transform.position;
                transform.position += offset;
            }
        }

        protected virtual void UpdateCameraRotation()
        {
            if (QuickVRManager.IsXREnabled())
            {
                UpdateCameraRotationXR();
            }
            else
            {
                //UpdateCameraRotationMono();
            }
        }

        protected virtual void UpdateCameraRotationXR()
        {
            //On the legacy XRMode, the camera is automatically rotated with the movement of the HMD. In The XRPlugin mode, we
            //have to manually apply the rotation of the HMD to the camera. 
            if (QuickSingletonManager.GetInstance<QuickVRManager>()._XRMode == QuickVRManager.XRMode.XRPlugin)
            {
                QuickVRNode vrNodeHead = QuickSingletonManager.GetInstance<QuickVRPlayArea>().GetVRNode(HumanBodyBones.Head);
                vrNodeHead.UpdateState();
                _camera.transform.localRotation = vrNodeHead.transform.localRotation;
            }
        }

        protected virtual void UpdateCameraRotationMono()
        {
            if (_rotateCamera)
            {
                float x = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
                float y = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
                _offsetH += _speedH * x;
                _offsetV -= _speedV * y;

                _offsetH = Mathf.Clamp(_offsetH, -MAX_HORIZONTAL_ANGLE, MAX_HORIZONTAL_ANGLE);
                _offsetV = Mathf.Clamp(_offsetV, -MAX_VERTICAL_ANGLE, MAX_VERTICAL_ANGLE);
            }

            //Reset the rotation of the head
            _head.localRotation = _initialLocalRotationHead;

            //Align the Camera with the avatar
            _camera.transform.rotation = _animatorSource.transform.rotation;

            //Temporaly make the Camera to be child of the head
            Transform tmpParent = _camera.transform.parent;
            _camera.transform.parent = _head;

            //Rotate the head of the avatar
            _head.Rotate(_camera.transform.up, _offsetH, Space.World);
            _head.Rotate(_camera.transform.right, _offsetV, Space.World);

            //Restore Camera's parent
            _camera.transform.parent = tmpParent;
        }

        #endregion

    }

}
