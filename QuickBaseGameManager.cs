using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace QuickVR {

	public class QuickBaseGameManager : MonoBehaviour {

        #region PUBLIC PARAMETERS

        public Transform _playerMale = null;
        public Transform _playerFemale = null;

        //public bool _useFootprints = true;
        //public Transform _footprints = null;
        public float _minDistToFootPrints = 0.5f;        

        protected QuickStageBase _initialStagePre = null;
        protected QuickStageBase _finalStagePre = null;

        protected QuickStageBase _initialStageMain = null;
        protected QuickStageBase _finalStageMain = null;

        protected QuickStageBase _initialStagePost = null;
        protected QuickStageBase _finalStagePost = null;

        [HideInInspector] 
        public bool _useExpirationDate = false;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickUserGUICalibration _guiCalibration = null;

        protected QuickVRManager _vrManager = null;

        protected Transform _player = null;

        protected DebugManager _debugManager = null;
        protected QuickSceneManager _sceneManager = null;

		protected bool _running = false;
		protected bool _finishing = false;

		protected float _timeRunning = 0.0f;    //The time elapsed since the application entered in Running state. 

        protected QuickInstructionsManager _instructionsManager = null;

        protected QuickUnityVR _hTracking;

        protected QuickTeleport _teleport = null;
        protected Coroutine _coUpdateTeleport = null;

        [SerializeField, HideInInspector]
        protected int _expirationDay = 0;

        [SerializeField, HideInInspector]
        protected int _expirationMonth = 0;

        [SerializeField, HideInInspector]
        protected int _expirationYear = 0;

        [SerializeField, HideInInspector]
        protected Transform _rootStagesPre = null;

        [SerializeField, HideInInspector]
        protected Transform _rootStagesMain = null;

        [SerializeField, HideInInspector]
        protected Transform _rootStagesPost = null;

        #endregion

        #region CONSTANTS 

        protected const string ROOT_STAGES_PRE_NAME = "__StagesPre__";
        protected const string ROOT_STAGES_MAIN_NAME = "__StagesMain__";
        protected const string ROOT_STAGES_POST_NAME = "__StagesPost__";

        #endregion

        #region EVENTS

        public static Action OnRunning;
        public static Action OnMovedPlayer;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Reset()
        {
            _rootStagesPre = transform.CreateChild(ROOT_STAGES_PRE_NAME);
            _rootStagesMain = transform.CreateChild(ROOT_STAGES_MAIN_NAME);
            _rootStagesPost = transform.CreateChild(ROOT_STAGES_POST_NAME);
        }

        protected virtual void OnEnable()
        {            
            OnRunning += StartTeleport;
            QuickTeleport.OnPostTeleport += SetInitialPositionAndRotation;
        }

        protected virtual void OnDisable()
        {
            OnRunning -= StartTeleport;
            QuickTeleport.OnPostTeleport -= SetInitialPositionAndRotation;
        }

        protected virtual void StartTeleport()
        {
            StartCoroutine(CoUpdateTeleport());
        }

        protected virtual void Awake() {
            _guiCalibration = QuickSingletonManager.GetInstance<QuickUserGUICalibration>();
            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
            //_calibrationAssisted = !QuickUtils.IsMobileTarget();
            _instructionsManager = QuickSingletonManager.GetInstance<QuickInstructionsManager>();
			_debugManager = QuickSingletonManager.GetInstance<DebugManager>();
            _sceneManager = QuickSingletonManager.GetInstance<QuickSceneManager>();
            
            InitGameConfiguration();
			
            AwakePlayer();
        }

        protected virtual void Start()
        {
            StartPlayer();

            if (_hTracking)
            {
                QuickSingletonManager.GetInstance<CameraFade>().SetColor(Color.black);
                _hTracking.CheckHandtrackingMode();
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
                QuickUnityVR hTracking = FindObjectOfType<QuickUnityVR>();
                if (hTracking) _player = hTracking.transform;
            }
            if (_player) _player.gameObject.SetActive(true);
        }

        protected virtual void StartPlayer()
        {
            if (_player)
            {
                _hTracking = _player.GetOrCreateComponent<QuickUnityVR>();
            }
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

		public virtual void Finish() 
        {
            if (_finishing) return;

			StartCoroutine(CoFinish());
		}

        public virtual void SetInitialPositionAndRotation()
        {
            Transform target = GetPlayer().transform;

            _hTracking.SetInitialPosition(target.position);
            _hTracking.SetInitialRotation(target.rotation);
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
                    QuickUICursor.GetVRCursor(QuickUICursor.Role.RightHand).enabled = false;
                    _teleport.SetTrajectoryVisible(false);
                }
            }
        }

        public virtual void GetExpirationDate(out int day, out int month, out int year)
        {
            day = _expirationDay;
            month = _expirationMonth;
            year = _expirationYear;
        }

        public virtual void SetExpirationDate(int day, int month, int year)
        {
            _expirationDay = day;
            _expirationMonth = month;
            _expirationYear = year;
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoUpdate() {
            //Check if the game has expired
            bool gameExpired = false;
            if (_useExpirationDate)
            {
                if (!QuickUtils.IsInternetConnection())
                {
                    _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.InternetConnectionRequired, _hTracking._handTrackingMode);
                    gameExpired = true;
                }
                else
                {
                    int day, month, year;
                    QuickUtils.GetDateOnline(out day, out month, out year);
                    DateTime timeNow = new DateTime(year, month, day);
                    DateTime timeExp = new DateTime(_expirationYear, _expirationMonth, _expirationDay);
                    if (timeNow >= timeExp)
                    {
                        _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.TimeExpired, _hTracking._handTrackingMode);
                        gameExpired = true;
                        Debug.Log("GAME DATE EXPIRED!!!");
                    }
                }
            }

            if (gameExpired)
            {
                while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
                {
                    yield return null;
                }

                QuickUtils.CloseApplication();
            }
            else
            {
                //yield return StartCoroutine(CoUpdateStateCalibration());

                GetInitialAndFinalStages(_rootStagesPre, out _initialStagePre, out _finalStagePre);
                //Debug.Log("INITIAL STAGE PRE = " + _initialStagePre);
                //Debug.Log("FINAL STAGE PRE = " + _finalStagePre);

                if (_initialStagePre)
                {
                    _initialStagePre.Init();
                }
                while (_finalStagePre && !_finalStagePre.IsFinished())
                {
                    yield return null;
                }

                Debug.Log("APPLICATION READY");
                Debug.Log("Time.time = " + Time.time);
                _running = true;
                _timeRunning = 0.0f;

                if (OnRunning != null) OnRunning();

                GetInitialAndFinalStages(_rootStagesMain, out _initialStageMain, out _finalStageMain);
                if (_initialStageMain)
                {
                    _initialStageMain.Init();
                }

                //Debug.Log("INITIAL STAGE = " + _initialStageMain);
                //Debug.Log("FINAL STAGE = " + _finalStageMain);
            }
		}

        protected virtual void GetInitialAndFinalStages(Transform rootStages, out QuickStageBase initialStage, out QuickStageBase finalStage)
        {
            initialStage = null;
            finalStage = null;

            for (int i = 0; i < rootStages.childCount; i++)
            {
                Transform tChild = rootStages.GetChild(i);
                if (tChild.gameObject.activeInHierarchy)
                {
                    QuickStageBase s = tChild.GetComponent<QuickStageBase>();
                    if (s)
                    {
                        if (!initialStage)
                        {
                            initialStage = s;
                        }
                        finalStage = s;
                    }
                }
            }
        }

		protected virtual void LateUpdate() {
            if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_EXIT))
            {
                Finish();
            }

			if (_running) 
            {
				_timeRunning += Time.deltaTime;
				
                //if (_footprints != null)
                //    _footprints.transform.position = new Vector3(_footprints.transform.position.x, GetPlayer().position.y, _footprints.transform.position.z);
            }
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

        protected virtual IEnumerator CoUpdateTeleport()
        {
            _teleport = GetPlayer().GetComponentInChildren<QuickTeleport>(true);
            if (_teleport != null)
            {
                QuickUICursor.Role cType = QuickUICursor.Role.RightHand;
                _teleport.enabled = true;
                QuickUICursor cursor = QuickUICursor.GetVRCursor(cType);

                cursor._RayCastMask &= ~(1 << LayerMask.NameToLayer("PeripheryVision"));

                while (true)
                {
                    yield return null;

                    bool isPointing = GetPlayer().GetComponent<QuickUnityVR>().GetVRHand(QuickVRNode.Type.RightHand).IsPointing();
                    QuickUICursor.GetVRCursor(cType).enabled = isPointing;
                }
            }
        }

        protected virtual IEnumerator CoFinish()
        {
            if (!_finishing)
            {
                _finishing = true;

                _rootStagesMain.gameObject.SetActive(false);    //Kill all the Main stages

                GetInitialAndFinalStages(_rootStagesPost, out _initialStagePost, out _finalStagePost);
                if (_initialStagePost)
                {
                    _initialStagePost.Init();
                }
                while (!_finalStagePost.IsFinished())
                {
                    yield return null;
                }

                Debug.Log("Elapsed Time = " + _timeRunning.ToString("f3") + " seconds");

                QuickUtils.CloseApplication();
            }
        }

        #endregion
    }

}