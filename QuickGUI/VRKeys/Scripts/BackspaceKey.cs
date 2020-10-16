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

namespace VRKeys 
{

	/// <summary>
	/// Backspace key.
	/// </summary>
	public class BackspaceKey : Key 
	{

		public override void DoAction() 
		{
			_keyboard.Backspace();
		}

		public override void UpdateLayout (Layout translation) 
		{
			_label.text = translation.backspaceButtonLabel;
		}

	}
}