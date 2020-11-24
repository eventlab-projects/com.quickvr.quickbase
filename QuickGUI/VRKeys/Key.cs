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
using System.Collections;
using TMPro;

namespace VRKeys 
{

	public class Key : MonoBehaviour 
	{

        #region PUBLIC ATTRIBUTES

		public KeyCode _keyCode = KeyCode.None;
		public bool _hasShiftedValue = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Keyboard _keyboard
        {
			get
            {
				return GetComponentInParent<Keyboard>();
            }
        }

		protected TextMeshProUGUI _label
		{
			get
            {
				return transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			}
		}
			
		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Start()
        {
			SetShifted(false);
		}

        #endregion

        #region GET AND SET

		public virtual void SetShifted(bool shifted)
		{
			if (_hasShiftedValue)
            {
				string label = GetLabel();
				SetLabel(shifted ? label.ToUpper() : label.ToLower());
			}
		}

		public string GetLabel()
		{
			return _label.text;
		}

		public virtual void SetLabel(string text)
		{
			_label.text = text;
		}

		public virtual void DoAction()
		{
			if (_keyCode == KeyCode.Return)
            {
				_keyboard.Submit();
            }
			else if (_keyCode == KeyCode.LeftShift)
            {
				_keyboard.ToggleShift();
            }
			else if (_keyCode == KeyCode.Space)
            {
				_keyboard.AddText(" ");
            }
			else if (_keyCode == KeyCode.Backspace)
            {
				_keyboard.Backspace();
            }
			else if (_keyCode == KeyCode.Delete)
            {
				_keyboard.SetText("");
            }
			else
            {
				//A general key
				_keyboard.AddText(GetLabel());
			}
		}

		#endregion

    }
		
}