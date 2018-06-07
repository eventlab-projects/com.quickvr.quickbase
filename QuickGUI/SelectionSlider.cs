using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace QuickVR {
    
	// This class works similarly to the SelectionRadial class except
    // it has a physical manifestation in the scene.  This can be
    // either a UI slider or a mesh with the SlidingUV shader.  The
    // functions as a bar that fills up whilst the user looks at it
    // and holds down the Fire1 button.
	[RequireComponent(typeof(AudioSource))]

	public class SelectionSlider : QuickUIInteractiveItem {

		#region PUBLIC PARAMETERS

		public float _Duration = 2f;             
		public AudioClip _OnOverClip;            
		public AudioClip _OnFilledClip;          
		public Slider _Slider;                     
		public Color _SliderTint = new Color(1,1,1,0);
		public bool _DisableOnBarFill;             

		#endregion
        
		#region PROTECTED PARAMETERS

		protected Collider _Collider;
		protected AudioSource _Audio;                       			

		protected Image _ImageToTint;                        
		protected Color _originalColor;
        
		protected enum State {
			IDLE,
			BAR_FILLING,
			BAR_COMPLETING_FILL,
			BAR_FILLED,
		};
		protected State _state = State.IDLE;

		#endregion

		#region EVENTS

		public event Action OnBarFilled;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
			_Audio = GetComponent<AudioSource>();
			_Collider = GetComponent<Collider>();
			_ImageToTint = GetComponentInChildren<Image>();
			if (_ImageToTint) _originalColor = _ImageToTint.color;

			SetState(State.IDLE);
		}

        #endregion

		#region GET AND SET

		protected virtual void SetState(State newState) {
			if (newState == State.IDLE) {
				StopCoroutine("FillBar");
				SetSliderValue(0f);
			}
			else if (newState == State.BAR_COMPLETING_FILL) {
				_Audio.clip = _OnFilledClip;
				_Audio.Play();
			}
			else if (newState == State.BAR_FILLED) {
				if (_DisableOnBarFill) {
					enabled = false;
					if (_Collider) _Collider.enabled = false;
				}
				if (OnBarFilled != null) OnBarFilled ();
			}

			_state = newState;
		}

		protected virtual void SetSliderValue(float sliderValue) {
			// If there is a slider component set it's value to the given slider value.
			if (_Slider) _Slider.value = sliderValue;

			// If there is a renderer set the shader's property to the given slider value.
			Shader.SetGlobalFloat("_QuickVRGUISliderValue", sliderValue);
		}

		public virtual bool IsBarFilled() {
			return _state == State.BAR_FILLED;
		}

		#endregion

		#region UPDATE

        protected IEnumerator FillBar() {
            // When the bar starts to fill, reset the timer.
			SetState(State.BAR_FILLING);
			float elapsedTime = 0.0f;

            // Until the timer is greater than the fill time...
			while (elapsedTime < _Duration) {
                // ... add to the timer the difference between frames.
				elapsedTime += Time.deltaTime;

                // Set the value of the slider or the UV based on the normalised time.
				SetSliderValue(elapsedTime / _Duration);
                
                // Wait until next frame.
                yield return null;
            }

			SetState(State.BAR_COMPLETING_FILL);
			while (_Audio.isPlaying) yield return null;

			SetState(State.BAR_FILLED);
        }

		public override void Over() {
			// The user is now looking at the bar.
			if (_state == State.IDLE) {
				if (_ImageToTint) _ImageToTint.color = _originalColor * (1.0f - _SliderTint.a) + new Color(_SliderTint.r, _SliderTint.g, _SliderTint.b) * _SliderTint.a;

				_Audio.clip = _OnOverClip;
				_Audio.Play();

				base.Over();
			}
		}

		public override void Out() {
			// The user is no longer looking at the bar.
			if ((_state == State.IDLE) || (_state == State.BAR_FILLING)) {
				if (_ImageToTint) _ImageToTint.color = _originalColor;

				SetState(State.IDLE);

				base.Out();
			}
		}

		public override void Up() {
			if (_state == State.BAR_FILLING) SetState(State.IDLE);

			base.Up();
		}

		public override void Down() {
            // If the user is looking at the bar start the FillBar coroutine and store a reference to it.
			if (IsOver() && (_state == State.IDLE)) StartCoroutine("FillBar");

			base.Down();
        }

	}

	#endregion

}