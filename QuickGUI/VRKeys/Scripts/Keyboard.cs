/**
 * Original version from: https://www.campfireunion.com
 */

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace VRKeys 
{

	public class Keyboard : MonoBehaviour 
	{
		[Serializable]
		public class KeyboardUpdateEvent : UnityEvent<string> { }

		[Serializable]
		public class KeyboardSubmitEvent : UnityEvent<string> { }


		#region PUBLIC ATTRIBUTES

		public KeyboardLayout keyboardLayout = KeyboardLayout.Qwerty;

		public TextMeshProUGUI displayText;

		public float _blinkTime = 0.5f;

		#endregion

		#region PROTECTED ATTRIBUTES

		protected string text = "";
		protected ShiftKey _shiftKey = null;
		protected float _timeBlinking = 0;
		protected bool _isInitialized = false;
		protected Key[] _keys = null;
		protected bool shifted = false;
		protected Layout _layout;

		#endregion

		public bool disabled = true;

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

        protected virtual void Start () 
		{
			SetLayout(keyboardLayout);

			displayText.text = text;
			_shiftKey = GetComponentInChildren<ShiftKey>();

			_isInitialized = true;
		}

		protected virtual void SetupKeys()
		{
			// Remove previous keys
			if (_keys != null)
			{
				foreach (Key k in _keys)
				{
					if (!k.IsProtected()) Destroy(k.gameObject);
				}
			}

			CreateRowKeys(_layout.row1Keys, _layout.row1Shift, 1, _layout.row1Offset);  //Numbers row
			CreateRowKeys(_layout.row2Keys, _layout.row2Shift, 2, _layout.row2Offset);  //QWERTY row
			CreateRowKeys(_layout.row3Keys, _layout.row3Shift, 3, _layout.row3Offset);  //ASDF row
			CreateRowKeys(_layout.row4Keys, _layout.row4Shift, 4, _layout.row4Offset); //ZXCV row

			_keys = GetComponentsInChildren<Key>();
		}

		protected virtual void CreateRowKeys(string[] rowKeys, string[] rowKeysShift, int rowNum, float rowOffset)
		{
			for (int i = 0; i < rowKeys.Length; i++)
			{
				LetterKey key = Instantiate<LetterKey>(Resources.Load<LetterKey>("Prefabs/pf_QuickVRKeyboardButton"), transform.GetChild(1));
				key.transform.localPosition = (Vector3.right * ((KEY_WIDTH * i) + rowOffset));
				key.transform.localPosition += (Vector3.down * KEY_HEIGHT * rowNum);

				key.character = rowKeys[i];
				key.shiftedChar = rowKeysShift[i];

				key.name = "Key: " + rowKeys[i];
				key.gameObject.SetActive(true);
			}
		}

		#endregion

		#region GET AND SET

		public virtual bool IsInitialized()
        {
			return _isInitialized;
        }

		public void Enable () {
			disabled = false;

			foreach (Transform t in transform)
            {
				t.gameObject.SetActive(true);
            }
		}

		public void Disable () {
			disabled = true;

			foreach (Transform t in transform)
			{
				t.gameObject.SetActive(false);
			}
		}

		public Key[] GetKeys()
        {
			return _keys;
        }

		public void SetText (string txt) 
		{
			text = txt;
			UpdateDisplayText();
		}

		/// <summary>
		/// Add a character to the input text.
		/// </summary>
		/// <param name="character">Character.</param>
		public void AddCharacter (string character) 
		{
			text += character;
			UpdateDisplayText();
		}

		/// <summary>
		/// Toggle whether the characters are shifted (caps).
		/// </summary>
		public bool ToggleShift () 
		{
			shifted = !shifted;

			foreach (Key key in _keys) {
				key.SetShifted(shifted);
			}

			return shifted;
		}

		/// <summary>
		/// Backspace one character.
		/// </summary>
		public void Backspace() 
		{
			if (text.Length > 0) 
			{
				text = text.Substring (0, text.Length - 1);
			}

			UpdateDisplayText();
		}

		/// <summary>
		/// Submit and close the keyboard.
		/// </summary>
		public void Submit() 
		{
			OnSubmit.Invoke(text);
		}

		/// <summary>
		/// Set the language of the keyboard.
		/// </summary>
		/// <param name="layout">New language.</param>
		public void SetLayout (KeyboardLayout layout) {
			keyboardLayout = layout;
			_layout = LayoutList.GetLayout(keyboardLayout);

			SetupKeys();

			// Update extra keys
			foreach (Key key in _keys)
			{
				key.UpdateLayout(_layout);
			}
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
			displayText.text = text;
			if (_timeBlinking < _blinkTime)
            {
				displayText.text += "|";
			}

			_timeBlinking += Time.deltaTime;
			if (_timeBlinking > _blinkTime * 2)
            {
				_timeBlinking = 0;
            }
        }

		#endregion

	}
}