using UnityEngine;
using TMPro;

using VRKeys;

namespace QuickVR 
{

	public class QuickKeyboardKey : MonoBehaviour 
	{

        #region PUBLIC ATTRIBUTES

		public KeyCode _keyCode = KeyCode.None;
		public bool _hasShiftedValue = false;

        #endregion

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
			if (_keyCode == KeyCode.Return)
            {
				_keyboard.Submit();
            }
			else if (_keyCode == KeyCode.LeftShift)
            {
				_keyboard.ToggleShift();
            }
			else if (_keyCode == KeyCode.Space)
            {
				_keyboard.AddText(" ");
            }
			else if (_keyCode == KeyCode.Backspace)
            {
				_keyboard.Backspace();
            }
			else if (_keyCode == KeyCode.Delete)
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