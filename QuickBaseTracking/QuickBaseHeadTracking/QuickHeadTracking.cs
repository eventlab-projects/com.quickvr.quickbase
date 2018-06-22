using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

    [Configurable("QuickHeadTracking")]
	public abstract class QuickHeadTracking : QuickBaseTrackingManager {

		#region PUBLIC PARAMETERS

		public Texture2D _calibrationTexture = null;

		public LayerMask _visibleLayers = -1;	//The layers that will be rendered by the cameras of the head tracking system. 

        public bool _applyHeadRotation = true;
		public bool _applyHeadPosition = true;

        public Camera _pfCamera = null;
		public float _cameraNearPlane = DEFAULT_NEAR_CLIP_PLANE;
		public float _cameraFarPlane = DEFAULT_FAR_CLIP_PLANE;
		
		#endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected Camera _camera = null;

        [SerializeField, HideInInspector]
        protected Transform _cameraControllerRoot = null;

        [SerializeField, HideInInspector]
        protected bool _showCalibrationScreen = true;

		protected Dictionary<VRCursorType, QuickUICursor> _vrCursors = new Dictionary<VRCursorType, QuickUICursor>();

        protected QuickVRHand _vrHandLeft = null;
        protected QuickVRHand _vrHandRight = null;

        #endregion

        #region CONSTANTS

        protected const int DEFAULT_PRIORITY_TRACKING_HEAD = 2000;
		protected const float DEFAULT_NEAR_CLIP_PLANE = 0.035f;
		protected const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

        protected const string CAMERA_CONTROLLER_ROOT_NAME = "__CameraControllerRoot__";
		protected const string CALIBRATION_SCREEN_NAME = "__CalibrationScreen__";

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake() {
            base.Awake();

            if (_animator && !_animator.isHuman) Debug.LogWarning("Animator detected, but Character is not rigged as HUMANOID.");

			CreateCameraControllerRoot();
			CreateCamera();
            CreateVRHands();
            CreateVRCursors();

            if (!_calibrationTexture) _calibrationTexture = LoadDefaultCalibrationTexture();
        }

        protected virtual void CreateCamera()
        {
            _camera = _pfCamera ? Instantiate<Camera>(_pfCamera) : new GameObject().GetOrCreateComponent<Camera>();
            _camera.name = "__Camera__";
            _camera.transform.parent = _cameraControllerRoot;
            _camera.transform.ResetTransformation();
            _camera.tag = "MainCamera";
            _camera.gameObject.GetOrCreateComponent<AudioListener>();
            _camera.gameObject.GetOrCreateComponent<FlareLayer>();
            _camera.gameObject.GetOrCreateComponent<GUILayer>();
            _camera.transform.rotation = transform.rotation;
        }

		protected virtual Texture2D LoadDefaultCalibrationTexture()
        {
            string path = "HMDCalibrationScreens/";
            if (SettingsBase.GetLanguage() == SettingsBase.Languages.ENGLISH) path += "en/";
			else path += "es/";
			return Resources.Load<Texture2D>(path + "CalibrationScreen");
        }

		protected virtual void CreateVRCursors() {
			CreateVRCursor(VRCursorType.HEAD, _camera.transform);
		}

		protected virtual void CreateVRCursor(VRCursorType cType, Transform cTransform) {
			QuickUICursor vrCursor = cTransform.gameObject.AddComponent<QuickUICursor>();
            vrCursor._TriggerVirtualKey = InputManager.DEFAULT_BUTTON_CONTINUE;
            vrCursor._drawRay = (cType == VRCursorType.LEFT || cType == VRCursorType.RIGHT);

    		_vrCursors[cType] = vrCursor;
            SetVRCursorActive(cType, false);
        }

		protected virtual void CreateCameraControllerRoot() {
            //_cameraControllerRoot = new GameObject(CAMERA_CONTROLLER_ROOT_NAME).transform;

            _cameraControllerRoot = transform.CreateChild(CAMERA_CONTROLLER_ROOT_NAME).transform;
        }

        protected virtual void CreateVRHands()
        {
            if (_vrHandLeft._axisAnim == "") _vrHandLeft._axisAnim = "LeftTrigger";
            if (_vrHandRight._axisAnim == "") _vrHandRight._axisAnim = "RightTrigger";
        }

        #endregion

        #region GET AND SET

        protected override int GetDefaultPriority()
        {
            return DEFAULT_PRIORITY_TRACKING_HEAD;
        }

        public virtual QuickUICursor GetVRCursor(VRCursorType cType) {
			if (!_vrCursors.ContainsKey(cType)) return null;

			return _vrCursors[cType];
		}

        public virtual bool IsVRCursorActive(VRCursorType cType)
        {
            QuickUICursor cursor = GetVRCursor(cType);
            return cursor ? cursor.enabled : false;
        }

		public virtual void SetVRCursorActive(VRCursorType cType, bool active) {
			if (!_vrCursors.ContainsKey(cType)) return;

            _vrCursors[cType].enabled = active;
		}

		public virtual Transform GetAvatarHead() {
			return IsHumanoid()? _animator.GetBoneTransform(HumanBodyBones.Head) : transform;
		}

		public virtual Camera GetCamera() {
			return _camera;
		}

		public virtual Transform GetCameraControllerRoot() {
			return _cameraControllerRoot;
		}

        public abstract Vector3 GetEyeCenterPosition();
        
        #endregion

        #region UPDATE

        public override void UpdateTracking() {
            UpdateCameraParameters(_camera);
		}

        protected virtual void UpdateCameraParameters(Camera cam)
        {
            if (!cam) return;

            cam.nearClipPlane = _cameraNearPlane;
            cam.farClipPlane = _cameraFarPlane;
            cam.cullingMask = _visibleLayers.value;
        }

		#endregion

	}

}