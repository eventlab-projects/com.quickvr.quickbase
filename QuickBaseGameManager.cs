using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace QuickVR {

	public class QuickBaseGameManager : MonoBehaviour {

        #region PUBLIC PARAMETERS

        public List<Texture2D> _logos = new List<Texture2D>();
        public float _logoFadeTime = 0.5f;
        public float _logoStayTime = 2.0f;

        public Transform _playerMale = null;
        public Transform _playerFemale = null;

        //public bool _useFootprints = true;
        //public Transform _footprints = null;
        public float _minDistToFootPrints = 0.5f;        

        public AudioClip _headTrackingCalibrationInstructions = null;
        public bool _calibrationAssisted = true;
		
		public float _timeOut = -1;	//Number of seconds to wait until automatic finishing the game
		public QuickStageBase _initialStage = null;

        public string _nextSceneName = "";

        #endregion

        #region PROTECTED PARAMETERS

        protected enum CalibrationStep
        {
            HMDAdjustment,
            ForwardDirection,
        }

        protected Transform _player = null;

        protected DebugManager _debugManager = null;
        protected QuickSceneManager _sceneManager = null;

		protected CameraFade _cameraFade = null;					//A pointer to the object used to control the fade-in and fade-out
		protected bool _running = false;
		protected bool _finishing = false;

		protected PerformanceFPS _fps = null;
		
		protected float _timeRunning = 0.0f;    //The time elapsed since the application entered in Running state. 

        protected QuickInstructionsManager _instructionsManager = null;

        protected QuickUnityVRBase _hTracking;

        protected Matrix4x4 _relativeMatrix = Matrix4x4.identity;

        protected QuickTeleport _teleport = null;
        protected Coroutine _coUpdateTeleport = null;

        #endregion

        #region EVENTS

        public static Action OnCalibrating;
        public static Action OnRunning;
        public static Action OnFinished;
        public static Action OnMovedPlayer;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {            
            OnRunning += StartTeleport;
            QuickTeleport.OnPreTeleport += SaveRelativeMatrix;
            QuickTeleport.OnPostTeleport += SetInitialPositionAndRotation;
        }

        protected virtual void OnDisable()
        {
            OnRunning -= StartTeleport;
            QuickTeleport.OnPostTeleport -= SetInitialPositionAndRotation;
            QuickTeleport.OnPreTeleport -= SaveRelativeMatrix;
        }

        protected virtual void StartTeleport()
        {
            StartCoroutine(CoUpdateTeleport());
        }

        protected virtual void Awake() {
            _calibrationAssisted = !QuickUtils.IsMobileTarget();
            _instructionsManager = QuickSingletonManager.GetInstance<QuickInstructionsManager>();
			_debugManager = QuickSingletonManager.GetInstance<DebugManager>();
            _sceneManager = QuickSingletonManager.GetInstance<QuickSceneManager>();
            _fps = QuickSingletonManager.GetInstance<PerformanceFPS>();
            _cameraFade = QuickSingletonManager.GetInstance<CameraFade>();

            if (!_headTrackingCalibrationInstructions) {
				_headTrackingCalibrationInstructions = Resources.Load<AudioClip>(GetDefaultHMDCalibrationInstructions());
			}

            float tOut = SettingsBase.GetTimeOutMinutes();
            if (tOut >= 0) _timeOut = tOut * 60.0f;

			InitGameConfiguration();
			
            AwakePlayer();
        }

        protected virtual void Start()
        {
            StartPlayer();

            _hTracking = _player ? _player.GetComponent<QuickUnityVRBase>() : null;
            if (_hTracking)
            {
                StartCoroutine(CoUpdate());
            }
            else
            {
                Debug.LogError("NO HEAD TRACKING FOUND!!! APPLICATION IS CLOSED");
                QuickUtils.CloseApplication();
            }
        }

        protected virtual void AwakePlayer()
        {
            if (_playerFemale) _playerFemale.gameObject.SetActive(false);
            if (_playerMale) _playerMale.gameObject.SetActive(false);
            _player = (SettingsBase.GetGender() == SettingsBase.Genders.FEMALE) ? _playerFemale : _playerMale;
            if (!_player)
            {
                QuickHeadTracking hTracking = FindObjectOfType<QuickHeadTracking>();
                if (hTracking) _player = hTracking.transform;
            }
            if (_player) _player.gameObject.SetActive(true);
        }

        protected virtual void StartPlayer()
        {
            if (_player && !_player.GetComponent<QuickHeadTracking>())
            {
                Animator animator = _player.GetComponent<Animator>();
                if (animator && animator.isHuman) _player.gameObject.AddComponent<QuickUnityVR>();
                else _player.gameObject.AddComponent<QuickUnityVRHands>();
            }
        }

        protected virtual string GetDefaultHMDCalibrationInstructions() {
			string path = "HMDCalibrationInstructions/";
            if (SettingsBase.GetLanguage() == SettingsBase.Languages.ENGLISH) path += "en/";
			else path += "es/";
			return path + "instructions";
		}

        protected virtual Texture2D GetHMDCalibrationScreen(CalibrationStep step)
        {
            string path = "HMDCalibrationScreens/";
            path += SettingsBase.GetLanguage() == SettingsBase.Languages.ENGLISH ? "en/" : "es/";
            path += "CalibrationScreen_";
            if (_calibrationAssisted)
            {
                path += "v1";
            }
            else
            {
                if (_hTracking._handTrackingMode == QuickUnityVRBase.HandTrackingMode.Controllers) path += "v2";
                else path += "v3";
            }
            
            path += step == CalibrationStep.HMDAdjustment ? "_00" : "_01";

            return Resources.Load<Texture2D>(path);
        }

		protected virtual void InitGameConfiguration() {

		}

		#endregion

		#region GET AND SET

        public Transform GetPlayer()
        {
            return _player;
        }

        public virtual bool IsRunning()
        {
            return _running;
        }

        public virtual float GetTimeRunning()
        {
            return _timeRunning;
        }

		public virtual void Finish() {
            if (_finishing) return;

			StartCoroutine(CoFinish());
		}

        protected virtual void SaveRelativeMatrix()
        {
            _relativeMatrix = GetPlayer().transform.worldToLocalMatrix;
        }

        public virtual void SetInitialPositionAndRotation()
        {
            Transform target = GetPlayer().transform;

            //Vector3 footPrintsLocalPos = Vector3.zero;
            //Vector3 footPrintsLocalForward = Vector3.forward;
            //if (_footprints != null)
            //{
            //    footPrintsLocalPos = _relativeMatrix.MultiplyPoint(_footprints.position);
            //    footPrintsLocalForward = _relativeMatrix.MultiplyVector(_footprints.forward);
            //}

            _hTracking.SetInitialPosition(target.position);
            _hTracking.SetInitialRotation(target.rotation);

            //if (_footprints != null)
            //{
            //    _footprints.position = target.localToWorldMatrix.MultiplyPoint(footPrintsLocalPos);
            //    _footprints.forward = target.localToWorldMatrix.MultiplyVector(footPrintsLocalForward);
            //}

            _relativeMatrix = Matrix4x4.identity;
        }

        protected bool IsPlayerOnSpot()
        {
            //if (_footprints != null)
            //    return Vector3.Distance(GetPlayer().transform.position, _footprints.position) <= _minDistToFootPrints;
            //else
                return true;
        }

        public virtual IEnumerator WaitParticipantToBeOnSpot()
        {
            // Wait for the participant to be on the right spot (footprints mark) 
            while (!IsPlayerOnSpot())
                yield return null;
                        
            yield return null;
        }

        public virtual void MovePlayerTo(Transform target, bool calibrate = true)
        {
            SaveRelativeMatrix();

            GetPlayer().position = target.position;

            GetPlayer().rotation = target.rotation;
            SetInitialPositionAndRotation();

            if (OnMovedPlayer != null)
                OnMovedPlayer();
        }

        public void EnableTeleport(bool b)
        {
            if (_teleport != null)
            {
                _teleport.enabled = b;

                if (b)
                {
                    if (_coUpdateTeleport == null)
                        _coUpdateTeleport = StartCoroutine(CoUpdateTeleport());
                }
                else if (_coUpdateTeleport != null)
                {
                    StopCoroutine(_coUpdateTeleport);
                    _coUpdateTeleport = null;
                }

                if (!b)
                {
                    _hTracking.SetVRCursorActive(VRCursorType.RIGHT, false);
                    _teleport.SetTrajectoryVisible(false);
                }
            }
        }

        protected virtual bool IsContinueTriggered()
        {
            if (_calibrationAssisted) return Input.GetKeyDown(KeyCode.Return);
            return InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE);
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoUpdate() {
            //Start loading the next scene
            if (_nextSceneName != "") _sceneManager.LoadSceneAsync(_nextSceneName);

            //Adjust the HMD
            yield return StartCoroutine(CoUpdateHMDAdjustment());

            //Show the logos if any
            yield return StartCoroutine(CoShowLogos());

            //Start the calibration process
            if (OnCalibrating != null) OnCalibrating();
            yield return StartCoroutine(CoUpdateStateCalibrating());	//Wait for the VR Devices Calibration
            QuickSingletonManager.GetInstance<QuickVRManager>().Calibrate(true);
            _player.GetComponent<QuickUnityVRBase>().InitVRNodeFootPrints();
            _debugManager.Clear();

            //Start the application
            _cameraFade.FadeIn(5.0f);
            while (_cameraFade.IsFading()) yield return null;
			
			Debug.Log("APPLICATION READY");
			Debug.Log("Time.time = " + Time.time);
			_running = true;
			_timeRunning = 0.0f;

			if (OnRunning != null) OnRunning();
			if (_initialStage) _initialStage.Init();
		}

		protected virtual void LateUpdate() {
			if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_EXIT)) Finish();

			if (_running) {
				_timeRunning += Time.deltaTime;
				if ((_timeOut > 0) && (_timeRunning >= _timeOut)) {
					//The maximum time for the game has expired. 
					Finish();
				}

                //if (_footprints != null)
                //    _footprints.transform.position = new Vector3(_footprints.transform.position.x, GetPlayer().position.y, _footprints.transform.position.z);
            }
		}

        protected virtual IEnumerator CoUpdateHMDAdjustment()
        {
            _cameraFade.SetColor(Color.white);
            _cameraFade.SetTexture(GetHMDCalibrationScreen(CalibrationStep.HMDAdjustment));

            //HMD Adjustment
            _debugManager.Log("Adjusting HMD. Press CONTINUE when ready.");
            while (!IsContinueTriggered()) yield return null;
            
            _cameraFade.SetColor(Color.black);
            _cameraFade.SetTexture(null);
            yield return null;
        }

		protected virtual IEnumerator CoUpdateStateCalibrating() {
            //HMD Forward Direction calibration
            _instructionsManager.Play(_headTrackingCalibrationInstructions);
            _cameraFade.SetColor(Color.white);
            _cameraFade.SetTexture(GetHMDCalibrationScreen(CalibrationStep.ForwardDirection));

            _debugManager.Log("[WAIT] Playing calibration instructions.", Color.red);
            while (_instructionsManager.IsPlaying() && !IsContinueTriggered()) yield return null;

            _debugManager.Log("Wait for the user to look forward. Press RETURN when ready.");
            while (!IsContinueTriggered()) yield return null;

            _instructionsManager.Stop();
            _cameraFade.SetColor(Color.black);
            _cameraFade.SetTexture(null);
            yield return null;
		}

        protected virtual IEnumerator CoPlayInstructions(AudioClip clip, string message = "", Color color = new Color())
        {
            _debugManager.Log(message, color);
            _instructionsManager.Play(clip);
            while (_instructionsManager.IsPlaying() && !InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
            {
                yield return null;
            }
            _instructionsManager.Stop();
        }

        protected virtual IEnumerator CoShowLogos()
        {
            //Show the logos
            Texture calibrationTexture = _cameraFade.GetTexture();
            
            foreach (Texture2D logo in _logos)
            {
                _cameraFade.SetTexture(logo);

                //fadeIN
                _cameraFade.FadeIn(_logoFadeTime);
                while (_cameraFade.IsFading()) yield return null;

                //fadeOUT
                _cameraFade.FadeOut(_logoFadeTime);
                while (_cameraFade.IsFading()) yield return null;
            }

            _cameraFade.SetTexture(calibrationTexture);
        }

		protected virtual IEnumerator CoUpdateTeleport()
        {
            _teleport = GetPlayer().GetComponentInChildren<QuickTeleport>(true);
            if (_teleport != null)
            {
                VRCursorType cType = VRCursorType.RIGHT;
                QuickUnityVRBase hTracking = GetPlayer().GetComponent<QuickUnityVRBase>();
                _teleport.enabled = true;
                QuickUICursor cursor = hTracking.GetVRCursor(cType);

                cursor._RayCastMask &= ~(1 << LayerMask.NameToLayer("PeripheryVision"));

                while (true)
                {
                    yield return null;

                    bool isPointing = hTracking.GetVRHand(QuickVRNode.Type.RightHand).IsPointing();
                    hTracking.SetVRCursorActive(cType, isPointing);
                }
            }
        }

        protected virtual IEnumerator CoFinish()
        {
            if (!_finishing)
            {
                _finishing = true;
                if (OnFinished != null) OnFinished();
                _cameraFade.FadeOut(5.0f);
                while (_cameraFade.IsFading()) yield return null;
                Debug.Log("Elapsed Time = " + _timeRunning.ToString("f3") + " seconds");

                if (_nextSceneName == "") QuickUtils.CloseApplication();
                else _sceneManager.ActivateScene(_nextSceneName);
            }
        }

        #endregion
    }

}