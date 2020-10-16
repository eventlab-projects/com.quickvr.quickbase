﻿/**
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
	/// An individual key in the VR keyboard.
	/// </summary>
	public class Key : MonoBehaviour {
		public Keyboard keyboard;

		public TextMeshProUGUI label;

		public Vector3 defaultPosition;

		public Vector3 pressedPosition;

		public Vector3 pressDirection = Vector3.down;

		public float pressMagnitude = 0.1f;

		public bool autoInit = false;

		private bool isPressing = false;

		private bool disabled = false;

		private IEnumerator _Press;

		private void Awake () {
			if (autoInit) {
				Init (transform.localPosition);
			}
		}

		/// <summary>
		/// Initialize the key with a default position and pressed position.
		/// </summary>
		/// <param name="defaultPos">Default position.</param>
		public void Init (Vector3 defaultPos) {
			defaultPosition = defaultPos;
			pressedPosition = defaultPos + (Vector3.down * 0.01f);
		}

		private void OnEnable () {
			isPressing = false;
			disabled = false;
			transform.localPosition = defaultPosition;

			OnEnableExtras ();
		}

		/// <summary>
		/// Override this to add custom logic on enable.
		/// </summary>
		protected virtual void OnEnableExtras () {
			// Override me!
		}

		public void OnTriggerEnter (Collider other) {
			if (isPressing || disabled || keyboard.disabled || !keyboard.initialized) {
				return;
			}

			Mallet mallet = other.gameObject.GetComponent<Mallet> ();
			if (mallet != null) {
				if (!mallet.isMovingDownward) {
					return;
				}

				if (mallet.hand == Mallet.MalletHand.Left && keyboard.leftPressing) {
					return;
				} else if (mallet.hand == Mallet.MalletHand.Right && keyboard.rightPressing) {
					return;
				}

				if (_Press != null && _Press.MoveNext ()) {
					StopCoroutine (_Press);
				}
				_Press = Press (other, mallet);
				StartCoroutine (_Press);
			}
		}

		private IEnumerator Press (Collider other, Mallet mallet) {
			isPressing = true;

			if (mallet.hand == Mallet.MalletHand.Left) {
				keyboard.leftPressing = true;
			} else if (mallet.hand == Mallet.MalletHand.Right) {
				keyboard.rightPressing = true;
			}

			mallet.HandleTriggerEnter (this);
			HandleTriggerEnter (other);

			transform.localPosition = pressedPosition;

			yield return new WaitForSeconds (0.125f);

			transform.localPosition = defaultPosition;
			isPressing = false;

			if (mallet.hand == Mallet.MalletHand.Left) {
				keyboard.leftPressing = false;
			} else if (mallet.hand == Mallet.MalletHand.Right) {
				keyboard.rightPressing = false;
			}
		}

		/// <summary>
		/// Override this to handle trigger events. Only fires when
		/// a downward trigger event occurred from the collider
		/// matching keyboard.colliderName.
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