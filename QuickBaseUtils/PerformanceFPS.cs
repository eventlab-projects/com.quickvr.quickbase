using UnityEngine;
using System.Collections;

using UnityEngine.UI;

namespace QuickVR {

    public class PerformanceFPS : MonoBehaviour
    {
		
		// Attach this to a GUIText to make a _frames/second indicator.
		//
		// It calculates _frames/second over each _updateInterval,
		// so the display does not keep changing wildly.
		//
		// It is also fairly accurate at very low FPS counts (<10).
		// We do this not by simply counting _frames per interval, but
		// by _accumulating FPS for each frame. This way we end up with
		// correct overall FPS even if the interval renders something like
		// 5.5 _frames.
		
		#region PUBLIC PARAMETERS

		public float _updateInterval = 0.5f;
		public KeyCode _showHideKey = KeyCode.F12;
		public bool _showFPS = false;

		#endregion

		#region PROTECTED PARAMETERS
		
		protected float _accum = 0; 		//FPS _accumulated over the interval
		protected int _frames = 0; 			//frames drawn over the interval
		protected float _timeLeft = 0.0f; 	//Left time for current interval

        protected Canvas _canvas = null;
        protected Text _text = null;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Start() {
			_timeLeft = _updateInterval;

            _canvas = transform.CreateChild("__Canvas__").gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _text = _canvas.transform.CreateChild("__Text__").gameObject.AddComponent<Text>();
            _text.fontSize = 20;
            _text.font = Resources.Load<Font>("Fonts/arial");
        }

		#endregion

		#region UPDATE

		protected virtual void Update() {
			if (Input.GetKeyDown(_showHideKey)) _showFPS = !_showFPS;

            _canvas.gameObject.SetActive(_showFPS);

			_timeLeft -= Time.deltaTime;
			_accum += Time.timeScale * Time.deltaTime;
			++_frames;

			// Interval ended - update GUI text and start new interval
			if (_timeLeft <= 0.0) {
				// display two fractional digits (f2 format)
				float fps = (float)_frames / _accum;
                _text.text = fps.ToString("f2") + " FPS"; 

				if(fps < 30) _text.color = Color.yellow;
				else if (fps < 10) _text.color = Color.red;
				else _text.color = Color.green;

				_timeLeft = _updateInterval;
				_accum = 0.0F;
				_frames = 0;
			}
		}

		#endregion
	}

}