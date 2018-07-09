using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	public class QuickStageBase : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public float _maxTimeOut = 0.0f;
		public bool _pressKeyToFinish = false;		//The test is finished if RETURN is pressed
		public bool _avoidable = true;				//We can force this test to finish by pressing BACKSPACE
		public bool _finishGameWhenOver = false;	//When this stage is over, it actually will finish the game

		public bool _sendOnInitEvent = true;
		public bool _sendOnFinishedEvent = true;

		public List<QuickStageBase> _nextStages = new List<QuickStageBase>();	//The tests to execute once this test finishes its execution
		public List<QuickStageBase> _killStages = new List<QuickStageBase>();	//The tests to kill once this test finishes its execution

        public string _debugMessage = "";
		public Color _debugMessageColor = Color.white;

		public delegate void OnInitAction(QuickStageBase stageManager);
		public static event OnInitAction OnInit;

		public delegate void OnFinishedAction(QuickStageBase stageManager);
		public static event OnFinishedAction OnFinished;

		#endregion

		#region PROTECTED PARAMETERS

		protected QuickBaseGameManager _gameManager = null;
		protected DebugManager _debugManager = null;

		protected string _testName = "";

		protected bool _finished = true;

		protected float _timeStart = 0.0f;	//Indicates when the test started (since the start of the game)

		#endregion

		#region PRIVATE PARAMETERS

		private bool _readyToFinish = false;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
			_testName = gameObject.name;
            _gameManager = QuickSingletonManager.GetInstance<QuickBaseGameManager>();
            _debugManager = QuickSingletonManager.GetInstance<DebugManager>();
        }

		protected virtual void Start() {

            enabled = false;
		}

		public virtual void Init() {
			if (_sendOnInitEvent && (OnInit != null)) OnInit(this);

            _finished = false;
			_readyToFinish = false;
			gameObject.SetActive(true);
			enabled = true;

			if (_maxTimeOut > 0) StartCoroutine("CoTimeOut");

			_timeStart = Time.time;

            if (_debugMessage.Length == 0) _debugMessage = name;
			_debugManager.Log(_debugMessage, _debugMessageColor);

			Debug.Log("RUNNING STAGE: " + _testName);
		}

		protected virtual T CreateStageManager<T>(string stageName) where T : QuickStageBase {
			GameObject go = new GameObject(stageName);
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			return go.AddComponent<T>();
		}

		#endregion

		#region GET AND SET

		protected virtual void SetReadyToFinish(bool isReadyToFinish) {
			_readyToFinish = isReadyToFinish;
			//if (_readyToFinish) _debugManager.Log("Click to continue. ");
		}

		public virtual bool IsFinished() {
			return _finished;
		}

		public virtual void Finish() {
			FinishSilently();

			if (_finishGameWhenOver) _gameManager.Finish();
			else {
				//Kill and start the indicated tests
				foreach (QuickStageBase bManager in _killStages) bManager.Finish();
				foreach (QuickStageBase bManager in _nextStages) bManager.Init();
			}
			enabled = false;
			if (_sendOnFinishedEvent && (OnFinished != null)) OnFinished(this);
		}

		/// <summary>
		/// The test is finished silently, i.e., nor the child tests are forced to start nor the kill tests are forced to finish. 
		/// </summary>
		public virtual void FinishSilently() {
			_debugManager.Clear();
			float totalTime = Time.time - _timeStart;
			Debug.Log("===============================");
			Debug.Log("TEST FINISHED: " + _testName);
			Debug.Log("Total Time = " + totalTime);
			Debug.Log("===============================");

			StopAllCoroutines();
			_finished = true;
			//gameObject.SetActive(false);
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