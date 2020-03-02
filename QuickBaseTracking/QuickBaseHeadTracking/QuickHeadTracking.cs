using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

	public abstract class QuickHeadTracking : QuickBaseTrackingManager {

		#region PUBLIC PARAMETERS

		public Texture2D _calibrationTexture = null;

		public LayerMask _visibleLayers = -1;	//The layers that will be rendered by the cameras of the head tracking system. 

        public Camera _pfCamera = null;
		public float _cameraNearPlane = DEFAULT_NEAR_CLIP_PLANE;
		public float _cameraFarPlane = DEFAULT_FAR_CLIP_PLANE;
		
		#endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected bool _showCalibrationScreen = true;

		protected Dictionary<VRCursorType, QuickUICursor> _vrCursors = new Dictionary<VRCursorType, QuickUICursor>();

        protected QuickVRHand _vrHandLeft = null;
        protected QuickVRHand _vrHandRight = null;

        #endregion

        #region CONSTANTS

		protected const float DEFAULT_NEAR_CLIP_PLANE = 0.05f;
		protected const float DEFAULT_FAR_CLIP_PLANE = 500.0f;

		protected const string CALIBRATION_SCREEN_NAME = "__CalibrationScreen__";

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake() {
            base.Awake();

            if (_animator && !_animator.isHuman) Debug.LogWarning("Animator detected, but Character is not rigged as HUMANOID.");

			InitCameraController();
            CreateVRHands();
            CreateVRCursors();

            if (!_calibrationTexture) _calibrationTexture = LoadDefaultCalibrationTexture();
        }

        protected override void Start()
        {
            base.Start();

            _vrManager.AddHeadTrackingSystem(this);
        }

        protected virtual Texture2D LoadDefaultCalibrationTexture()
        {
            string path = "HMDCalibrationScreens/";
            if (SettingsBase.GetLanguage() == SettingsBase.Languages.ENGLISH) path += "en/";
			else path += "es/";
			return Resources.Load<Texture2D>(path + "CalibrationScreen");
        }

		protected virtual void CreateVRCursors() {
			CreateVRCursor(VRCursorType.HEAD, Camera.main.transform);
		}

		protected virtual void CreateVRCursor(VRCursorType cType, Transform cTransform) {
			QuickUICursor vrCursor = cTransform.gameObject.AddComponent<QuickUICursor>();
            vrCursor._TriggerVirtualKey = InputManager.DEFAULT_BUTTON_CONTINUE;
            vrCursor._drawRay = (cType == VRCursorType.LEFT || cType == VRCursorType.RIGHT);

    		_vrCursors[cType] = vrCursor;
            SetVRCursorActive(cType, false);
        }

		protected virtual void InitCameraController() {
            QuickVRCameraController cameraController = _vrManager.GetCameraController();
            cameraController.CreateCamera(_pfCamera);
            cameraController.SetAnimator(_animator);
        }

        protected virtual void CreateVRHands()
        {
            if (_vrHandLeft._axisAnim == "") _vrHandLeft._axisAnim = "LeftTrigger";
            if (_vrHandRight._axisAnim == "") _vrHandRight._axisAnim = "RightTrigger";
        }

        #endregion

        #region GET AND SET

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

		public abstract Vector3 GetEyeCenterPosition();
        
        #endregion

        #region UPDATE

        public override void UpdateTracking()
        {
            Camera cam = _vrManager.GetCameraController().GetCamera();

            cam.nearClipPlane = _cameraNearPlane;
            cam.farClipPlane = _cameraFarPlane;
            cam.cullingMask = _visibleLayers.value;
		}

		#endregion

	}

}