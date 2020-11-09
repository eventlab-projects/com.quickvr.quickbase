using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
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

        protected Animator _animatorSource 
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource();
            }
        }

        #endregion

        #region CONSTANTS

        protected const float DEFAULT_NEAR_CLIP_PLANE = 0.05f;
        protected const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            if (!Camera.main)
            {
                Camera camera = _pfCamera ? Instantiate<Camera>(_pfCamera) : new GameObject().GetOrCreateComponent<Camera>();
                camera.name = "__Camera__";
                camera.transform.parent = transform;
                camera.transform.ResetTransformation();
                camera.tag = "MainCamera";
                camera.gameObject.GetOrCreateComponent<FlareLayer>();
            }
            
            Camera.main.GetOrCreateComponent<AudioListener>();
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
            
            if (animator)
            {
                Camera camera = Camera.main;

                if (QuickSingletonManager.GetInstance<QuickVRManager>()._XRMode == QuickVRManager.XRMode.XRPlugin)
                {
                    camera.transform.rotation = QuickSingletonManager.GetInstance<QuickVRPlayArea>().GetVRNode(HumanBodyBones.Head).transform.rotation;
                }
                
                //Apply the correct rotation to the cameracontrollerroot:
                Vector3 up = animator.transform.up;
                Vector3 rightCam = Vector3.ProjectOnPlane(camera.transform.right, up).normalized;
                Vector3 r = animator.GetEye(false).position - animator.GetEye(true).position;
                Vector3 rightHead = Vector3.ProjectOnPlane(r, up).normalized;
                float rotOffset = Vector3.SignedAngle(rightCam, rightHead, up);
                transform.Rotate(up, rotOffset, Space.World);

                //This forces the camera to be in the Avatar's eye center. 
                Vector3 offset = animator.GetEyeCenterPosition() - camera.transform.position;
                transform.position += offset;
            }
        }

        #endregion

    }

}
