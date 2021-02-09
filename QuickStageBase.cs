using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	public class QuickStageBase : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public float _maxTimeOut = 0;
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
        }
        public FinishPolicy _finishPolicy = FinishPolicy.ExecuteNext;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickInstructionsManager _instructionsManager = null;

        protected QuickBaseGameManager _gameManager = null;
		protected DebugManager _debugManager = null;

		protected bool _finished = false;

		protected float _timeStart = 0.0f;	//Indicates when the test started (since the start of the game)

        protected QuickCoroutineManager _coManager = null;
        protected int _coSet = -1;

		#endregion

		#region EVENTS

        public delegate void OnStageAction(QuickStageBase stageManager);
        public static event OnStageAction OnInit;
        public static event OnStageAction OnFinished;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake() {
            _instructionsManager = QuickSingletonManager.GetInstance<QuickInstructionsManager>();
            _gameManager = QuickSingletonManager.GetInstance<QuickBaseGameManager>();
            _debugManager = QuickSingletonManager.GetInstance<DebugManager>();
            _coManager = QuickSingletonManager.GetInstance<QuickCoroutineManager>();
        }

		protected virtual void Start()
        {
            enabled = false;
		}

		public virtual void Init()
        {
            _finished = false;
            enabled = true;

            _timeStart = Time.time;
            _debugManager.Log("RUNNING STAGE: " + GetName());
            if (_debugMessage.Length != 0)
            {
                _debugManager.Log(_debugMessage, _debugMessageColor);
            }

            if (OnInit != null) OnInit(this);

            StartCoroutine(CoUpdateBase());
		}

        #endregion

		#region GET AND SET

        protected virtual string GetName()
        {
            return GetType().Name + " (" + name + ")";
        }

		public virtual bool IsFinished() {
			return _finished;
		}

		public virtual void Finish() {
            _finished = true;
            _instructionsManager.Stop();
            _debugManager.Clear();
            float totalTime = Time.time - _timeStart;
            Debug.Log("STAGE FINISHED: " + GetName() + " " + totalTime.ToString("f3"));

            _coManager.StopCoroutineSet(_coSet);
            StopAllCoroutines();

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
            
			enabled = false;
            
            if (OnFinished != null) OnFinished(this);
		}

		#endregion

		#region UPDATE

		protected virtual void Update()
        {
			if (_avoidable && InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CANCEL)) 
			{
				Finish();
			}
		}

        private IEnumerator CoUpdateBase()
        {
            _instructionsManager.SetAudioSource(_instructionsAudioSource);
            _instructionsManager._timePauseBetweenInstructions = _instructionsTimePause;
            _instructionsManager._volume = _instructionsVolume;
            SettingsBase.Languages lang = SettingsBase.GetLanguage();
            if (lang == SettingsBase.Languages.SPANISH) _instructionsManager.Play(_instructionsSpanish);
            else if (lang == SettingsBase.Languages.ENGLISH) _instructionsManager.Play(_instructionsEnglish);

            while (_instructionsManager.IsPlaying()) yield return null;

            _coSet = _coManager.BeginCoroutineSet();
            _coManager.StartCoroutine(CoUpdate(), _coSet);
            _coManager.StartCoroutine(CoWaitForUserInput(), _coSet);
            _coManager.StartCoroutine(CoTimeOut(), _coSet);

            yield return _coManager.WaitForCoroutineSet(_coSet);

            Finish();
        }

        protected virtual IEnumerator CoWaitForUserInput()
        {
            if (_pressKeyToFinish)
            {
                while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
                {
                    yield return null;
                }
                Finish();
            }
        }

        protected virtual IEnumerator CoTimeOut()
        {
            if (_maxTimeOut > 0)
            {
                yield return new WaitForSeconds(_maxTimeOut);
                Finish();
            }
            else if (_maxTimeOut < 0)
            {
                while (true)
                {
                    yield return null;
                }
            }
        }

        protected virtual IEnumerator CoUpdate()
        {
            yield break;
        }

        #endregion
    }

}