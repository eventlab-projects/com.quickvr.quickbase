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
	/// An individual key in the VR _keyboard.
	/// </summary>
	public class Key : MonoBehaviour 
	{

		private bool isPressing = false;

		private IEnumerator _Press;

		#region PROTECTED ATTRIBUTES

		protected Keyboard _keyboard = null;
		protected TextMeshProUGUI _label = null;
		protected bool _isShifted = false;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake()
        {
			_keyboard = GetComponentInParent<Keyboard>();
			_label = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }

		protected virtual void Start()
        {
			SetShifted(false);
		}

        #endregion

        private void OnEnable () {
			isPressing = false;

			OnEnableExtras ();
		}

		/// <summary>
		/// Override this to add custom logic on enable.
		/// </summary>
		protected virtual void OnEnableExtras () {
			// Override me!
		}

		public virtual void DoAction() 
		{
			
		}

		/// <summary>
		/// Update the key's label from a new language.
		/// </summary>
		/// <param name="translation">Translation object.</param>
		public virtual void UpdateLayout (Layout translation) {
			// Override me!
		}

        #region GET AND SET

		public virtual bool IsProtected()
        {
			return name[0] == '$';
        }

		public virtual void SetShifted(bool shifted)
        {
			_isShifted = shifted;
        }

		public virtual bool IsShifted()
        {
			return _isShifted;
        }

        #endregion

    }
}