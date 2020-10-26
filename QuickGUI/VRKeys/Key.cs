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
			
		protected bool _isShifted = false;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Start()
        {
			SetShifted(false);
		}

        #endregion

        public virtual void DoAction() 
		{
			
		}

		public virtual void UpdateLayout (Layout translation) 
		{
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