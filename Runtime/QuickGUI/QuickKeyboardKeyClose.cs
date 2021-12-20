using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

namespace QuickVR 
{

	public class QuickKeyboardKeyClose : QuickKeyboardKey
	{

		#region GET AND SET

		public override void DoAction()
		{
			_keyboard.Enable(false);
		}

		#endregion

    }
		
}