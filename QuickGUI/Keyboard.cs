/**
 * Original version from: https://www.campfireunion.com
 */

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using QuickVR;

namespace VRKeys 
{

	public class Keyboard : MonoBehaviour 
	{
		[Serializable]
		public class KeyboardSubmitEvent : UnityEvent<string> { }


		#region PUBLIC ATTRIBUTES

		public float _blinkTime = 0.5f;

		#endregion

		#region PROTECTED ATTRIBUTES

		protected string _text = "";
		protected TextMeshProUGUI _textInput = null;
		protected TextMeshProUGUI _textHint = null;

		protected float _timeBlinking = 0;
		protected QuickKeyboardKey[] _keys = null;

		protected bool _isEnabled = true;
		protected bool shifted = false;

		protected Transform _rootKeys
        {
			get
            {
				return transform.Find("__Keyboard__");
            }
        }

		#endregion

		#region CONSTATS

		protected KeyCode[] _keysRow1 = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P };
		protected KeyCode[] _keysRow2 = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Colon };
		protected KeyCode[] _keysRow3 = { KeyCode.LeftShift, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Slash, KeyCode.Period };

		#endregion

		/// <summary>
		/// Listen for events when Submit() is called.
		/// </summary>
		public KeyboardSubmitEvent OnSubmit = new KeyboardSubmitEvent ();

		#region CONSTANTS

        protected const float KEY_WIDTH = 0.16f;
		protected const float KEY_HEIGHT = 0.16f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake() 
		{
			CreateRowKeys(_keysRow1, 1);
			CreateRowKeys(_keysRow2, 2);
			CreateRowKeys(_keysRow3, 3);
			_keys = GetComponentsInChildren<QuickKeyboardKey>();

			_textInput = transform.Find("__TextInput__").GetComponentInChildren<TextMeshProUGUI>();
		}

		protected virtual void CreateRowKeys(KeyCode[] rowKeys, int rowNum)
		{
			for (int i = 0; i < rowKeys.Length; i++)
			{
				QuickKeyboardKey key = Instantiate(Resources.Load<Transform>("Prefabs/pf_QuickVRKeyboardButton"), _rootKeys).GetOrCreateComponent<QuickKeyboardKey>();
				key.transform.localPosition = Vector3.right * (KEY_WIDTH * 0.5f + KEY_WIDTH * i);
				key.transform.localPosition += Vector3.down * ((KEY_HEIGHT * 0.5f) + (KEY_HEIGHT * rowNum));

				KeyCode c = rowKeys[i];
				key._keyCode = c;
				key._hasShiftedValue = ((int)c >= (int)KeyCode.A) && ((int)c <= (int)KeyCode.Z); 

				if (c == KeyCode.LeftShift)
                {
					key.SetLabel('\u25B2'.ToString());
				}
				else if (c == KeyCode.Colon)
                {
					key.SetLabel(":");
				}
				else if (c == KeyCode.Period)
                {
					key.SetLabel(".");
				}
				else if (c == KeyCode.Slash)
                {
					key.SetLabel("/");
				}
				else
                {
					//Letter key
					key.SetLabel(c.ToString().ToLower());
				}
				
				key.name = "Key: " + c.ToString();
			}
		}

		#endregion

		#region GET AND SET

		public virtual void Enable(bool enabled, bool clearTextOnEnable = true) 
		{
			foreach (Transform t in transform)
            {
				t.gameObject.SetActive(enabled);
            }
			
			if (enabled && clearTextOnEnable)
            {
				SetText("");
            }

			if (!QuickVRManager.IsXREnabled())
            {
				InputManagerUnity iManager = QuickSingletonManager.GetInstance<InputManager>().GetComponentInChildren<InputManagerUnity>();
                //iManager._active = !enabled;
                //_rootKeys.gameObject.SetActive(false);
            }

			_isEnabled = enabled;
		}

		public virtual bool IsEnabled()
		{
			return _isEnabled;
		}

		public virtual QuickKeyboardKey[] GetKeys()
        {
			return _keys;
        }

		public virtual string GetText()
        {
			return _text;
        }

		public virtual void SetText(string txt) 
		{
			_text = txt;
			_timeBlinking = 0;
		}

		public virtual void AddText(string txt) 
		{
			SetText(_text + txt);
		}

		public virtual void Backspace()
		{
			string text = _text;
			if (text.Length > 0)
			{
				text = text.Substring(0, text.Length - 1);
			}

			SetText(text);
		}

		public virtual bool ToggleShift () 
		{
			shifted = !shifted;

			foreach (QuickKeyboardKey key in _keys) {
				key.SetShifted(shifted);
			}

			return shifted;
		}

		public virtual void Submit() 
		{
			OnSubmit.Invoke(_text);
		}

		#endregion

        #region UPDATE

		protected virtual void Update()
        {
			if (!QuickVRManager.IsXREnabled())
			{
				UpdateKeyboardMono();
			}

			_textInput.text = _text;
			if (_timeBlinking < _blinkTime)
            {
				_textInput.text += "|";
			}

			_timeBlinking += Time.deltaTime;
			if (_timeBlinking > _blinkTime * 2)
            {
				_timeBlinking = 0;
            }
        }

		protected virtual void UpdateKeyboardMono()
        {
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                ToggleShift();
            }

            foreach (QuickKeyboardKey k in _keys)
            {
				if (k._keyCode != KeyCode.None && Input.GetKeyDown(k._keyCode))
                {
					k.DoAction();
                }
            }
        }

		#endregion

	}
}