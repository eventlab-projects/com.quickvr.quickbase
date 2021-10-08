using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	public class QuickStageBase : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public float _maxTimeOut = 0;
		public bool _pressKeyToFinish = false;		//The test is finished if RETURN is pressed
		public bool _avoidable = true;				//We can force this test to finish by pressing BACKSPACE

		[Space()]
        [Header("Stage Instructions")]
        public List<AudioClip> _instructionsSpanish = new List<AudioClip>();
        public List<AudioClip> _instructionsEnglish = new List<AudioClip>();
        public AudioSource _instructionsAudioSource = null;
        [Range(0.0f, 1.0f)]
        public float _instructionsVolume = 1.0f;
        public float _instructionsTimePause = 0.5f;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickInstructionsManager _instructionsManager = null;

        protected QuickBaseGameManager _gameManager = null;

		protected float _timeStart = 0.0f;	//Indicates when the test started (since the start of the game)

        protected QuickCoroutineManager _coManager = null;
        protected int _coSet = -1;

        #endregion

        #region PRIVATE ATTRIBUTES

        private static Stack<QuickStageBase> _stackStages = new Stack<QuickStageBase>();

        #endregion

        #region EVENTS

        public delegate void OnStageAction();
        public event OnStageAction OnInit;
        public event OnStageAction OnFinish;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake() {
            _instructionsManager = QuickSingletonManager.GetInstance<QuickInstructionsManager>();
            _gameManager = QuickSingletonManager.GetInstance<QuickBaseGameManager>();
            _coManager = QuickSingletonManager.GetInstance<QuickCoroutineManager>();
        }

		protected virtual void Start()
        {
            enabled = false;
		}

		public virtual void Init()
        {
            PushStage(this);

            enabled = true;

            _timeStart = Time.time;
            QuickVRManager.Log("RUNNING STAGE: " + GetName());

            if (OnInit != null)
            {
                OnInit();
            }

            StartCoroutine(CoUpdateBase());
		}

        #endregion

		#region GET AND SET

        private static void PushStage(QuickStageBase stage)
        {
            _stackStages.Push(stage);
        }

        private static QuickStageBase PopStage()
        {
            return _stackStages.Pop();
        }

        public static QuickStageBase GetTopStage()
        {
            return _stackStages.Count > 0 ? _stackStages.Peek() : null;
        }

        public static void ClearStackStages()
        {
            _stackStages.Clear();
        }

        protected virtual string GetName()
        {
            return GetType().Name + " (" + name + ")";
        }

		//public virtual bool IsFinished() {
		//	return _finished;
		//}

		public virtual void Finish() 
        {
            PopStage();

            _instructionsManager.Stop();
            float totalTime = Time.time - _timeStart;
            QuickVRManager.Log("STAGE FINISHED: " + GetName() + " " + totalTime.ToString("f3"));

            _coManager.StopCoroutineSet(_coSet);
            StopAllCoroutines();

            if (OnFinish != null)
            {
                OnFinish();
            }

            //Look for the nextStage to be executed
            QuickStageBase nextStage = null;
            for (int i = transform.GetSiblingIndex() + 1; !nextStage && i < transform.parent.childCount; i++)
            {
                Transform t = transform.parent.GetChild(i);
                if (t.gameObject.activeInHierarchy)
                {
                    nextStage = transform.parent.GetChild(i).GetComponent<QuickStageBase>();
                }
            }
            if (nextStage)
            {
                nextStage.Init();
            }
            
			enabled = false;
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
            if (lang == SettingsBase.Languages.Spanish)
            {
                _instructionsManager.Play(_instructionsSpanish);
            }
            else if (lang == SettingsBase.Languages.English)
            {
                if (_instructionsEnglish.Count > 0)
                {
                    _instructionsManager.Play(_instructionsEnglish);
                }
            }

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

        public static void PrintStackStages()
        {
            QuickVRManager.Log("======================================");
            foreach (QuickStageBase stage in _stackStages)
            {
                QuickVRManager.Log(stage.name);
            }
            QuickVRManager.Log("======================================");
        }

        #endregion
    }

}