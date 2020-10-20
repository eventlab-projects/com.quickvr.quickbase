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

#if UNITY_2020_1_OR_NEWER
                camera.transform.rotation = QuickSingletonManager.GetInstance<QuickVRPlayArea>().GetVRNode(HumanBodyBones.Head).transform.rotation;
#endif

                //Apply the correct rotation to the cameracontrollerroot:
                QuickIKManager ikManager = animator.GetComponent<QuickIKManager>();
                Vector3 up = animator.transform.up;

                Vector3 forwardCam = Vector3.ProjectOnPlane(camera.transform.forward, up).normalized;
                Vector3 forwardHead = Vector3.ProjectOnPlane(ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb.forward, up);

                float rotOffset = Vector3.SignedAngle(forwardCam, forwardHead, up);
                transform.Rotate(up, rotOffset, Space.World);

                //This forces the camera to be in the Avatar's eye center. 
                Vector3 offset = animator.GetEyeCenterPosition() - camera.transform.position;
                transform.position += offset;
            }
        }

        #endregion

    }

}
