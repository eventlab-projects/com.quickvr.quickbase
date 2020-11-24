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
		public class KeyboardUpdateEvent : UnityEvent<string> { }

		[Serializable]
		public class KeyboardSubmitEvent : UnityEvent<string> { }


		#region PUBLIC ATTRIBUTES

		public float _blinkTime = 0.5f;

		#endregion

		#region PROTECTED ATTRIBUTES

		protected string text = "";
		protected TextMeshProUGUI _textInput = null;
		protected TextMeshProUGUI _textHint = null;

		protected float _timeBlinking = 0;
		protected Key[] _keys = null;
		protected Layout _layout = new Layout();

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

		/// <summary>
		/// Listen for events whenever the text changes.
		/// </summary>
		public KeyboardUpdateEvent OnUpdate = new KeyboardUpdateEvent ();

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
			CreateRowKeys(_layout.row1Keys, 1);
			CreateRowKeys(_layout.row2Keys, 2);
			CreateRowKeys(_layout.row3Keys, 3);
			_keys = GetComponentsInChildren<Key>();

			_textInput = transform.Find("__TextInput__").GetComponentInChildren<TextMeshProUGUI>();
		}

		protected virtual void CreateRowKeys(KeyCode[] rowKeys, int rowNum)
		{
			for (int i = 0; i < rowKeys.Length; i++)
			{
				Key key = Instantiate(Resources.Load<Transform>("Prefabs/pf_QuickVRKeyboardButton"), _rootKeys).GetOrCreateComponent<Key>();
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
				iManager._active = !enabled;
            }

			_isEnabled = enabled;
		}

		public virtual bool IsEnabled()
		{
			return _isEnabled;
		}

		public virtual Key[] GetKeys()
        {
			return _keys;
        }

		public virtual string GetText()
        {
			return text;
        }

		public virtual void SetText(string txt) 
		{
			text = txt;
			UpdateDisplayText();
		}

		public virtual void AddText(string txt) 
		{
			text += txt;
			UpdateDisplayText();
		}

		public virtual void Backspace()
		{
			if (text.Length > 0)
			{
				text = text.Substring(0, text.Length - 1);
			}

			UpdateDisplayText();
		}

		public virtual bool ToggleShift () 
		{
			shifted = !shifted;

			foreach (Key key in _keys) {
				key.SetShifted(shifted);
			}

			return shifted;
		}

		public virtual void Submit() 
		{
			OnSubmit.Invoke(text);
		}

		#endregion

        #region UPDATE

		protected virtual void UpdateDisplayText()
        {
			_timeBlinking = 0;
			OnUpdate.Invoke(text);
		}

		protected virtual void Update()
        {
			if (!QuickVRManager.IsXREnabled())
			{
				UpdateKeyboardMono();
			}

			_textInput.text = text;
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

            foreach (Key k in _keys)
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