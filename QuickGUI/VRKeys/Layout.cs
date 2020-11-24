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

namespace VRKeys {

	/// <summary>
	/// Base class for keyboard layouts to inherit from in order to support
	/// additional languages.
	///
	/// See the VRKeys/Scripts/Layouts folder for example layouts.
	/// To add a translation, you will need to register it in the Layouts.cs
	/// class too.
	/// </summary>
	public class Layout 
	{
		public KeyCode[] row1Keys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P};
		public KeyCode[] row2Keys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Colon};
		public KeyCode[] row3Keys = { KeyCode.LeftShift, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Slash, KeyCode.Period};
	}
}