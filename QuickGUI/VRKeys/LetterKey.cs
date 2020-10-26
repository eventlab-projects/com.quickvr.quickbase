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

namespace VRKeys {

	/// <summary>
	/// An individual letter key.
	/// </summary>
	public class LetterKey : Key {

		#region PROTECTED ATTRIBUTES

		protected TextMeshProUGUI _shiftedLabel
        {
			get
            {
				return transform.childCount > 1 ? transform.GetChild(1).GetComponent<TextMeshProUGUI>() : null;
			}
        }

		#endregion

		public string character = "";

		public string shiftedChar = "";

		public override void SetShifted(bool shifted)
        {
            base.SetShifted(shifted);

			_label.text = shifted ? shiftedChar : character;
			if (_shiftedLabel)
			{
				_shiftedLabel.text = shifted ? character : shiftedChar;
			}
        }

        public string GetCharacter () 
		{
			return IsShifted()? shiftedChar : character;
		}

        public override void DoAction()
        {
			_keyboard.AddCharacter(GetCharacter());
		}
	}
}