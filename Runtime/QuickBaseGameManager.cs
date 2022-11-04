using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace QuickVR {

	public class QuickBaseGameManager : MonoBehaviour 
    {

        #region PUBLIC PARAMETERS

        public static QuickBaseGameManager _instance
        {
            get
            {
                return GetTopGameManager();
            }
        }

        //public bool _useFootprints = true;
        //public Transform _footprints = null;
        public float _minDistToFootPrints = 0.5f;        

        [HideInInspector] 
        public bool _useExpirationDate = false;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickUserGUICalibration _guiCalibration = null;

        protected QuickVRManager _vrManager = null;

        protected Transform _player = null;

        protected enum State
        {
            Idle,
            StagesPre, 
            StagesMain,
            StagesPost,
        }
        protected State _state = State.Idle;

		protected float _timeRunning = 0.0f;    //The time elapsed since the application entered in Running state. 

        protected QuickInstructionsManager _instructionsManager = null;

        protected QuickUnityVR _hTracking;

        [SerializeField, HideInInspector]
        protected int _expirationDay = 0;

        [SerializeField, HideInInspector]
        protected int _expirationMonth = 0;

        [SerializeField, HideInInspector]
        protected int _expirationYear = 0;

        protected QuickStageGroup _stagesPre = null;    //Stages executed BEFORE the main logic
        protected QuickStageGroup _stagesMain = null;   //Main logic of the application
        protected QuickStageGroup _stagesPost = null;   //Stages executed AFTER the main logic and BEFORE closing the application

        #endregion

        #region PRIVATE ATTRIBUTES

        private static Stack<QuickBaseGameManager> _stackGameManagers = new Stack<QuickBaseGameManager>();

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
            CreateStagesPre();
            CreateStagesMain();
            CreateStagesPost();

            //_stagesPre, _stagesMain and _stagesPost cannot be inactive. So check if they are marked as 
            //inactive and it that case, reactivate them and deactivate the children. 
            CheckStagesGroup(_stagesPre);
            CheckStagesGroup(_stagesMain);
            CheckStagesGroup(_stagesPost);
        }

        protected virtual void CheckStagesGroup(QuickStageGroup sGroup)
        {
            if (!sGroup.gameObject.activeSelf)
            {
                foreach (Transform t in sGroup.transform)
                {
                    t.gameObject.SetActive(false);
                }

                sGroup.gameObject.SetActive(true);
            }
        }

        protected virtual void CreateStagesPre()
        {
            Transform tStages = transform.CreateChild(ROOT_STAGES_PRE_NAME);
            _stagesPre = tStages.GetComponent<QuickStageGroup>();
            
            if (!_stagesPre)
            {
                //Init the default Stages Pre
                _stagesPre = tStages.gameObject.AddComponent<QuickStageGroup>();
                _stagesPre.transform.CreateChild("HMDAdjustment").GetOrCreateComponent<QuickStageHMDAdjustment>();
                _stagesPre.transform.CreateChild("Calibration").GetOrCreateComponent<QuickStageCalibration>();
                _stagesPre.transform.CreateChild("FadeIn").GetOrCreateComponent<QuickStageFade>();
            }

            _stagesPre.OnFinish += OnFinishStagesPre;
        }

        protected virtual void CreateStagesMain()
        {
            Transform tStages = transform.CreateChild(ROOT_STAGES_MAIN_NAME);
            _stagesMain = tStages.GetComponent<QuickStageGroup>();

            if (!_stagesMain)
            {
                //Init the default Stages Main
                _stagesMain = tStages.gameObject.AddComponent<QuickStageGroup>();
                QuickStageBase dummy = _stagesMain.transform.CreateChild("DeleteMe").GetOrCreateComponent<QuickStageBase>();
                dummy._maxTimeOut = -1;
            }
        }

        protected virtual void CreateStagesPost()
        {
            Transform tStages = transform.CreateChild(ROOT_STAGES_POST_NAME);
            _stagesPost = tStages.GetComponent<QuickStageGroup>();

            if (!_stagesPost)
            {
                //Init the default Stages Post
                _stagesPost = tStages.gameObject.AddComponent<QuickStageGroup>();
                QuickStageFade fade = _stagesPost.transform.CreateChild("FadeOut").GetOrCreateComponent<QuickStageFade>();
                fade._fadeType = QuickStageFade.FadeType.FadeOut;
            }

            _stagesPost.OnFinish += OnFinishStagesPost;
        }


        protected virtual void Awake() 
        {
            PushGameManager(this);
            
            Reset();

            _guiCalibration = QuickSingletonManager.GetInstance<QuickUserGUICalibration>();
            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
            //_calibrationAssisted = !QuickUtils.IsMobileTarget();
            _instructionsManager = QuickSingletonManager.GetInstance<QuickInstructionsManager>();
            
            InitGameConfiguration();
			
            AwakePlayer();
        }

        protected virtual void Start()
        {
            QuickSingletonManager.GetInstance<QuickSceneManager>().SetLogicScene(gameObject.scene);
            StartPlayer();

            if (_hTracking)
            {
                QuickSingletonManager.GetInstance<CameraFade>().SetColor(Color.black);
                StartCoroutine(CoUpdate());
            }
            else
            {
                QuickVRManager.LogError("NO HEAD TRACKING FOUND!!! APPLICATION IS CLOSED");
                QuickUtils.CloseApplication();
            }
        }

        protected virtual void AwakePlayer()
        {
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

        private static void PushGameManager(QuickBaseGameManager gameManager)
        {
            _stackGameManagers.Push(gameManager);
        }

        private static QuickBaseGameManager PopGameManager()
        {
            return _stackGameManagers.Pop();
        }

        private static QuickBaseGameManager GetTopGameManager()
        {
            return _stackGameManagers.Count > 0 ? _stackGameManagers.Peek() : null;
        }

        public virtual bool IsRunning()
        {
            return _state == State.StagesMain;
        }

        public virtual float GetTimeRunning()
        {
            return _timeRunning;
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

        public virtual void Finish()
        {
            if (_state != State.StagesPost)
            {
                _state = State.StagesPost;

                //Kill the pre and main stages
                _stagesPre.gameObject.SetActive(false);
                _stagesMain.gameObject.SetActive(false);
                QuickSingletonManager.GetInstance<QuickInstructionsManager>().Stop();

                QuickStageBase.ClearStackStages();

                _stagesPost.Init();
            }
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
                    _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.InternetConnectionRequired);
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
                        _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.TimeExpired);
                        gameExpired = true;
                        QuickVRManager.Log("GAME DATE EXPIRED!!!");
                    }
                }
            }

            //Skip a frame. This is important in order to guarantee that the Start function of every stage (specially the one
            //of __StagesPre__) has been executed prior to initializing the first stage. 
            yield return null;

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
                //Execute the stagesPre
                _state = State.StagesPre;
                _stagesPre.Init();
            }
		}

        protected virtual void OnFinishStagesPre()
        {
            _state = State.StagesMain;

            QuickVRManager.Log("APPLICATION READY");
            QuickVRManager.Log("Time.time = " + Time.time);

            if (OnRunning != null) OnRunning();

            //Execute the stagesMain
            _timeRunning = 0.0f;
        }

        protected virtual void OnFinishStagesPost()
        {
            PopGameManager();

            if (GetTopGameManager() == null)
            {
                QuickVRManager.Log("Elapsed Time = " + _timeRunning.ToString("f3") + " seconds");

                QuickUtils.CloseApplication();
            }
        }

        protected virtual void LateUpdate() 
        {
            if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_EXIT))
            {
                Finish();
            }

			if (_state == State.StagesMain) 
            {
				_timeRunning += Time.deltaTime;
            }
		}

        #endregion

    }

}