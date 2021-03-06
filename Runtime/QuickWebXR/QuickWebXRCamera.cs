using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;

using WebXR;
using UnityEditor;

namespace QuickVR
{

    //Simplified version of WebXRCamera
    public class QuickWebXRCamera : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected Camera _cameraLeft = null;
        protected Camera _cameraRight = null;

        protected WebXRManager _wxrManager = null;
        protected Coroutine _coPostRender = null;

        #endregion

        #region CONSTANTS

        protected Rect RECT_WEBXR_ENABLED = new Rect(0, 0, 0.5f, 1);
        protected Rect RECT_WEBXR_DISABLED = new Rect(0, 0, 1, 1);

        #endregion

        #region DLL IMPORTS

        [DllImport("__Internal")]
        protected static extern void XRPostRender();

        #endregion

#region CREATION AND DESTRUCTION

#if UNITY_WEBGL
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        protected static void Init()
        {
            Camera tmpCamera = Camera.main;
            
            QuickVRCameraController cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();
            QuickWebXRCamera wxrCameras = cameraController.transform.CreateChild("WebXRCameras").GetOrCreateComponent<QuickWebXRCamera>();

            if (tmpCamera) 
            {
                wxrCameras.transform.position = tmpCamera.transform.position;
                wxrCameras.transform.forward = tmpCamera.transform.forward;

                tmpCamera.gameObject.SetActive(false);
            }
        }
#endif

        protected virtual void Awake()
        {
            _wxrManager = WebXRManager.Instance;

            _cameraLeft = transform.CreateChild("__CameraLeft__").GetOrCreateComponent<Camera>();
            _cameraLeft.tag = "MainCamera";

            _cameraRight = transform.CreateChild("__CameraRight__").GetOrCreateComponent<Camera>();
            _cameraRight.rect = new Rect(0.5f, 0, 0.5f, 1);
            _cameraRight.stereoTargetEye = StereoTargetEyeMask.Right;
            _cameraRight.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            _wxrManager.OnHeadsetUpdate += onHeadsetUpdate;
            _wxrManager.OnXRChange += onVRChange;

#if UNITY_EDITOR
            // No editor specific funtionality
#elif UNITY_WEBGL
			_coPostRender = StartCoroutine(endOfFrame());
#endif
            Camera.onPreRender += UpdateStereoTargetEye;
        }

        protected virtual void OnDisable()
        {
            if (_coPostRender != null)
            {
                StopCoroutine(_coPostRender);
            }
        }

#endregion

#region UPDATE

        protected IEnumerator endOfFrame()
        {
            // Wait until end of frame to report back to WebXR browser to submit frame.
            while (enabled)
            {
                yield return new WaitForEndOfFrame();
                XRPostRender();
            }
        }

        protected virtual void onVRChange(WebXRState state)
        {
            if (state == WebXRState.ENABLED)
            {
                _cameraLeft.rect = RECT_WEBXR_ENABLED;
                _cameraLeft.stereoTargetEye = StereoTargetEyeMask.Left;
                _cameraRight.gameObject.SetActive(true);
            }
            else
            {
                _cameraLeft.rect = RECT_WEBXR_DISABLED;
                _cameraLeft.stereoTargetEye = StereoTargetEyeMask.Both;
                _cameraRight.gameObject.SetActive(false);
            }
        }

        protected void onHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix)
        {
            if (_wxrManager.xrState == WebXRState.ENABLED)
            {
                WebXRMatrixUtil.SetTransformFromViewMatrix(_cameraLeft.transform, leftViewMatrix * sitStandMatrix.inverse);
                _cameraLeft.projectionMatrix = leftProjectionMatrix;
                WebXRMatrixUtil.SetTransformFromViewMatrix(_cameraRight.transform, rightViewMatrix * sitStandMatrix.inverse);
                _cameraRight.projectionMatrix = rightProjectionMatrix;
            }
        }

        protected virtual void UpdateStereoTargetEye(Camera cam)
        {
            Shader.SetGlobalInt("STEREO_TARGET_EYE", (int)cam.stereoTargetEye);
        }

        #endregion

    }

}
