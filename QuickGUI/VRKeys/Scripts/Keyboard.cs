/**
 * Copyright (c) 2017 The Campfire Union Inc - All Rights Reserved.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for
 * full license information.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace VRKeys 
{

	/// <summary>
	/// Keyboard input system for use with NewtonVR. To use, drop the VRKeys prefab
	/// into your scene and activate as needed. Listen for OnUpdate and OnSubmit events,
	/// and set the text via SetText(string).
	///
	/// Input validation can be done during OnUpdate and OnSubmit events by calling
	/// ShowValidationMessage(msg) and HideValidationMessage(). The keyboard does not
	/// automatically hide OnSubmit, but rather you should call SetActive(false) when
	/// you have finished validating the submitted text.
	/// </summary>
	/// 

	public class Keyboard : MonoBehaviour 
	{

		#region PUBLIC ATTRIBUTES

		public KeyboardLayout keyboardLayout = KeyboardLayout.Qwerty;

		public TextMeshProUGUI displayText;

		public float _blinkTime = 0.5f;
		public string text = "";

		#endregion

		#region PROTECTED ATTRIBUTES

		protected ShiftKey _shiftKey = null;
		protected float _timeBlinking = 0;

        #endregion

        [Space (15)]
		

		protected bool _isInitialized = false;

		public bool disabled = true;

		[Serializable]
		public class KeyboardUpdateEvent : UnityEvent<string> { }

		[Serializable]
		public class KeyboardSubmitEvent : UnityEvent<string> { }

		[Space (15)]

		/// <summary>
		/// Listen for events whenever the text changes.
		/// </summary>
		public KeyboardUpdateEvent OnUpdate = new KeyboardUpdateEvent ();

		/// <summary>
		/// Listen for events when Submit() is called.
		/// </summary>
		public KeyboardSubmitEvent OnSubmit = new KeyboardSubmitEvent ();

		/// <summary>
		/// Listen for events when Cancel() is called.
		/// </summary>
		public UnityEvent OnCancel = new UnityEvent ();

		protected Key[] _keys = null;

		private bool shifted = false;

		private Layout _layout;

        #region CONSTANTS

        protected const float keyWidth = 0.16f;
		protected const float keyHeight = 0.16f;

        #endregion

        /// <summary>
        /// Initialization.
        /// </summary>
        protected virtual void Start () 
		{
			SetLayout(keyboardLayout);

			displayText.text = text;
			_shiftKey = GetComponentInChildren<ShiftKey>();

			_isInitialized = true;
		}

		public virtual bool IsInitialized()
        {
			return _isInitialized;
        }

		/// <summary>
		/// Make sure mallets don't stay attached if VRKeys is disabled without
		/// calling Disable().
		/// </summary>
		private void OnDisable () {
			Disable ();
		}

		/// <summary>
		/// Enable the keyboard.
		/// </summary>
		public void Enable () {
			disabled = false;

			foreach (Transform t in transform)
            {
				t.gameObject.SetActive(true);
            }
		}

		/// <summary>
		/// Disable the keyboard.
		/// </summary>
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

		/// <summary>
		/// Set the text value all at once.
		/// </summary>
		/// <param name="txt">New text value.</param>
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

		/// <summary>
		/// Setup the keys.
		/// </summary>
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

			CreateRowKeys(_layout.row1Keys, _layout.row1Shift, 1, _layout.row1Offset);	//Numbers row
			CreateRowKeys(_layout.row2Keys, _layout.row2Shift, 2, _layout.row2Offset);	//QWERTY row
			CreateRowKeys(_layout.row3Keys, _layout.row3Shift, 3, _layout.row3Offset);	//ASDF row
			CreateRowKeys(_layout.row4Keys, _layout.row4Shift, 4, _layout.row4Offset); //ZXCV row

			_keys = GetComponentsInChildren<Key>();
		}

        protected virtual void CreateRowKeys(string[] rowKeys, string[] rowKeysShift, int rowNum, float rowOffset)
        {
            for (int i = 0; i < rowKeys.Length; i++)
            {
                LetterKey key = Instantiate<LetterKey>(Resources.Load<LetterKey>("Prefabs/pf_QuickVRKeyboardButton"), transform.GetChild(1));
                key.transform.localPosition = (Vector3.right * ((keyWidth * i) + rowOffset));
				key.transform.localPosition += (Vector3.down * keyHeight * rowNum);

                key.character = rowKeys[i];
                key.shiftedChar = rowKeysShift[i];

                key.name = "Key: " + rowKeys[i];
                key.gameObject.SetActive(true);
			}
        }

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