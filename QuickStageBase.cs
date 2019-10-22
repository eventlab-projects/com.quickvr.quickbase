﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	public class QuickStageBase : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public float _maxTimeOut = 0.0f;
		public bool _pressKeyToFinish = false;		//The test is finished if RETURN is pressed
		public bool _avoidable = true;				//We can force this test to finish by pressing BACKSPACE

		public string _debugMessage = "";
		public Color _debugMessageColor = Color.white;

        [Space()]
        [Header("Stage Instructions")]
        public List<AudioClip> _instructionsSpanish = new List<AudioClip>();
        public List<AudioClip> _instructionsEnglish = new List<AudioClip>();
        public AudioSource _instructionsAudioSource = null;
        [Range(0.0f, 1.0f)]
        public float _instructionsVolume = 1.0f;
        public float _instructionsTimePause = 0.5f;

        public enum FinishPolicy
        {
            Nothing,
            ExecuteNext,
            GameOver,
        }
        public FinishPolicy _finishPolicy = FinishPolicy.ExecuteNext;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickInstructionsManager _instructionsManager = null;

        protected QuickBaseGameManager _gameManager = null;
		protected DebugManager _debugManager = null;

		protected bool _finished = false;

		protected float _timeStart = 0.0f;	//Indicates when the test started (since the start of the game)

		#endregion

		#region PRIVATE PARAMETERS

		private bool _readyToFinish = false;

        #endregion

        #region EVENTS

        public delegate void OnStageAction(QuickStageBase stageManager);
        public static event OnStageAction OnBeforeInstructions;
        public static event OnStageAction OnAfterInstructions;
        public static event OnStageAction OnFinished;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake() {
            _instructionsManager = QuickSingletonManager.GetInstance<QuickInstructionsManager>();
            _gameManager = QuickSingletonManager.GetInstance<QuickBaseGameManager>();
            _debugManager = QuickSingletonManager.GetInstance<DebugManager>();
        }

		protected virtual void Start()
        {
            enabled = false;
		}

		public virtual void Init() {
            _finished = false;
            _readyToFinish = false;
            gameObject.SetActive(true);
            enabled = true;

            _timeStart = Time.time;
            Debug.Log("===============================");
            _debugManager.Log("RUNNING STAGE: " + GetName());
            if (_debugMessage.Length != 0)
            {
                _debugManager.Log(_debugMessage, _debugMessageColor);
            }

            StartCoroutine(CoUpdate());
		}

        private IEnumerator CoUpdateBase()
        {
            yield return StartCoroutine(CoUpdate());
        }

        protected virtual IEnumerator CoUpdate()
        {
            if (OnBeforeInstructions != null) OnBeforeInstructions(this);

            _instructionsManager.SetAudioSource(_instructionsAudioSource);
            _instructionsManager._timePauseBetweenInstructions = _instructionsTimePause;
            _instructionsManager._volume = _instructionsVolume;
            SettingsBase.Languages lang = SettingsBase.GetLanguage();
            if (lang == SettingsBase.Languages.SPANISH) _instructionsManager.Play(_instructionsSpanish);
            else if (lang == SettingsBase.Languages.ENGLISH) _instructionsManager.Play(_instructionsEnglish);

            while (_instructionsManager.IsPlaying()) yield return null;

            if (OnAfterInstructions != null) OnAfterInstructions(this);

            if (_maxTimeOut > 0) StartCoroutine("CoTimeOut");
        }

		#endregion

		#region GET AND SET

        protected virtual string GetName()
        {
            return GetType().Name + " (" + name + ")";
        }

		protected virtual void SetReadyToFinish(bool isReadyToFinish) {
			_readyToFinish = isReadyToFinish;
			//if (_readyToFinish) _debugManager.Log("Click to continue. ");
		}

		public virtual bool IsFinished() {
			return _finished;
		}

		public virtual void Finish() {
            _instructionsManager.Stop();
            _debugManager.Clear();
            float totalTime = Time.time - _timeStart;
            Debug.Log("STAGE FINISHED: " + GetName());
            Debug.Log("Total Time = " + totalTime);
            Debug.Log("===============================");

            StopAllCoroutines();
            _finished = true;

            if (_finishPolicy == FinishPolicy.ExecuteNext)
            {
                QuickStageBase nextStage = QuickUtils.GetNextSibling<QuickStageBase>(this);
                if (nextStage)
                {
                    //Debug.Log("currentStage = " + name);
                    //Debug.Log("nextStage = " + nextStage.name);
                    nextStage.Init();
                }
            }
            else if (_finishPolicy == FinishPolicy.GameOver)
            {
                _gameManager.Finish();
            }

			enabled = false;
            
            if (OnFinished != null) OnFinished(this);
		}

		#endregion

		#region UPDATE

		protected virtual IEnumerator CoTimeOut() {
			yield return new WaitForSeconds(_maxTimeOut);
			SetReadyToFinish(true);
		}

		protected virtual void Update() {
			if (
				(_avoidable && InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CANCEL)) ||
				(_readyToFinish && (!_pressKeyToFinish || InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)))
				) 
			{
				Finish();
			}
		}

		#endregion
	}

}