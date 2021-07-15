using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

namespace QuickVR 
{

	public class QuickKeyboardKey : MonoBehaviour 
	{

        #region PUBLIC ATTRIBUTES

		public Key _keyCode = Key.None;
		public bool _hasShiftedValue = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickKeyboard _keyboard
        {
			get
            {
				return GetComponentInParent<QuickKeyboard>();
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
			if (_keyCode == Key.Enter)
            {
				_keyboard.Submit();
            }
			else if (_keyCode == Key.LeftShift)
            {
				_keyboard.ToggleShift();
            }
			else if (_keyCode == Key.Space)
            {
				_keyboard.AddText(" ");
            }
			else if (_keyCode == Key.Backspace)
            {
				_keyboard.Backspace();
            }
			else if (_keyCode == Key.Delete)
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