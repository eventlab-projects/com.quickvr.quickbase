/**
 * Copyright (c) 2017 The Campfire Union Inc - All Rights Reserved.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for
 * full license information.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

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
		public string spaceButtonLabel = "Space";
		public string enterButtonLabel = "OK";
		public string cancelButtonLabel = "Cancel";
		public string shiftButtonLabel = "Shift";
		public string backspaceButtonLabel = "<<";
		public string clearButtonLabel = "Clear";

		public string[] row1Keys = { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" };
		public string[] row1Shift = { "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+" };

		public string[] row2Keys = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\" };
		public string[] row2Shift = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}", "|" };

		public string[] row3Keys = { "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'" };
		public string[] row3Shift = { "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\"" };

		public string[] row4Keys = { "z", "x", "c", "v", "b", "n", "m", ",", ".", "?" };
		public string[] row4Shift = { "Z", "X", "C", "V", "B", "N", "M", "<", ">", "/" };

		public float row1Offset = 0f;
		public float row2Offset = 0.24f;
		public float row3Offset = 0.32f;
		public float row4Offset = 0.40f;

	}
}