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

		public TextMeshProUGUI label;

		private bool isPressing = false;

		private bool disabled = false;

		private IEnumerator _Press;

		#region PROTECTED ATTRIBUTES

		protected Keyboard _keyboard = null;

        #endregion

        #region CREATION AND DESTRUCTION

		protected virtual void Awake()
        {
			_keyboard = GetComponentInParent<Keyboard>();
        }

        #endregion

        private void OnEnable () {
			isPressing = false;
			disabled = false;

			OnEnableExtras ();
		}

		/// <summary>
		/// Override this to add custom logic on enable.
		/// </summary>
		protected virtual void OnEnableExtras () {
			// Override me!
		}

		/// <summary>
		/// Override this to handle trigger events. Only fires when
		/// a downward trigger event occurred from the collider
		/// matching _keyboard.colliderName.
		/// </summary>
		/// <param name="other">Collider.</param>
		public virtual void HandleTriggerEnter (Collider other) {
			// Override me!
		}

		/// <summary>
		/// Disable the key.
		/// </summary>
		public virtual void Disable () {
			disabled = true;
		}

		/// <summary>
		/// Re-enable a disabled key.
		/// </summary>
		public virtual void Enable () {
			disabled = false;
		}

		/// <summary>
		/// Update the key's label from a new language.
		/// </summary>
		/// <param name="translation">Translation object.</param>
		public virtual void UpdateLayout (Layout translation) {
			// Override me!
		}
	}
}