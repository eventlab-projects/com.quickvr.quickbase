using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerUnity : BaseInputManager
    {

        #region PUBLIC ATTRIBUTES

        public float _mouseSensitivity = 0.1f;

        #endregion

        #region PROTECTED PARAMETERS

        protected enum AxisCode
        {
            Horizontal,
            Vertical,
        };

        protected static Dictionary<string, Key> _stringToKey = new Dictionary<string, Key>();
        protected static Dictionary<Key, string> _keyToString = new Dictionary<Key, string>();
        protected static Dictionary<AxisCode, string> _axisCodeToString = new Dictionary<AxisCode, string>();

        protected Mouse _mouse
        {
            get
            {
                return Mouse.current;
            }
        }

        protected Keyboard _keyboard
        {
            get
            {
                return Keyboard.current;
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            foreach (Key k in QuickUtils.GetEnumValues<Key>())
            {
                _stringToKey[k.ToString()] = k;
                _keyToString[k] = k.ToString();
            }

            foreach (AxisCode c in QuickUtils.GetEnumValues<AxisCode>())
            {
                _axisCodeToString[c] = c.ToString();
            }
        }

        public override void Reset()
        {
            base.Reset();

            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, Key.D.ToString(), Key.A.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, Key.W.ToString(), Key.S.ToString());
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, Key.Enter.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, Key.Backspace.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, Key.Escape.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, Key.R.ToString());

            //Configure the default buttons
            //ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, Key.Enter.ToString(), KeyCode.JoystickButton0.ToString());
            //ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, Key.Backspace.ToString(), KeyCode.JoystickButton1.ToString());
            //ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, Key.Escape.ToString());
            //ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, Key.R.ToString(), KeyCode.JoystickButton3.ToString());
        }

        #endregion

        #region GET AND SET

        public static Key ToKeyCode(string keyName)
        {
            return _stringToKey[keyName];
        }

        public static string ToString(Key k)
        {
            return _keyToString[k];
        }

        protected static string ToString(AxisCode c)
        {
            return _axisCodeToString[c];
        }

        public override string[] GetAxisCodes()
        {
            return GetCodes<AxisCode>();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<Key>();
        }

        protected override float ImpGetAxis(string axis)
        {
            float aValue = 0;

            if (axis == ToString(AxisCode.Horizontal))
            {
                if (_mouse != null)
                {
                    aValue = _mouse.delta.x.ReadValue() * _mouseSensitivity;
                }
            }
            else if (axis == ToString(AxisCode.Vertical))
            {
                if (_mouse != null)
                {
                    aValue = _mouse.delta.y.ReadValue() * _mouseSensitivity;
                }
            }

            //float aValue = Input.GetAxis(axis);
            return aValue;
        }

        protected override bool ImpGetButton(string button)
        {
            bool result = false; 
            if (_keyboard != null)
            {
                Key key = ToKeyCode(button);
                result = _keyboard[key].isPressed;
            }
            
            return result;
        }

        #endregion

    }

}
