using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerKeyboard : InputManagerGeneric<Keyboard, BaseInputManager.DefaultCode, Key>
    {

        #region PRIVATE ATTRIBUTES

        private static Keyboard _keyboard = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, Key.D.ToString(), Key.A.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, Key.W.ToString(), Key.S.ToString());
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, Key.Enter.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, Key.Backspace.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, Key.Escape.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, Key.R.ToString());
        }

        #endregion

        #region GET AND SET

        protected override Keyboard GetInputDevice()
        {
            return Keyboard.current;
        }

        protected override float ImpGetAxis(DefaultCode axis)
        {
            return 0;
        }

        protected override bool ImpGetButton(Key button)
        {
            return _inputDevice[button].isPressed;
        }

        protected static void CheckKeyboard()
        {
            if (_keyboard == null)
            {
                _keyboard = Keyboard.current;
            }
}

        public static bool GetKeyDown(Key key)
        {
            CheckKeyboard();

            return _keyboard != null && _keyboard[key].wasPressedThisFrame;
        }

        public static bool GetKey(Key key)
        {
            CheckKeyboard();

            return _keyboard != null && _keyboard[key].isPressed;
        }

        public static bool GetKeyUp(Key key)
        {
            CheckKeyboard();

            return _keyboard != null && _keyboard[key].wasReleasedThisFrame;
        }

        #endregion

    }

}
