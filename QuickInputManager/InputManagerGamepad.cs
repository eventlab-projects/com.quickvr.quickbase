using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerGamepad : BaseInputManager
    {

        #region PUBLIC ATTRIBUTES

        public float _deadZone = 0.2f;

        #endregion

        #region PROTECTED PARAMETERS

        public enum AxisCode
        {
            LeftStick_Horizontal,
            LeftStick_Vertical,

            RightStick_Horizontal, 
            RightStick_Vertical,

            LeftTrigger, 
            RightTrigger, 
        };

        protected enum ButtonCode
        {
            North = GamepadButton.North,
            East = GamepadButton.East,
            South = GamepadButton.South,
            West = GamepadButton.West,

            Start = GamepadButton.Start,
            Select = GamepadButton.Select,

            LeftStick = GamepadButton.LeftStick,
            RightStick = GamepadButton.RightStick,

            DpadUp = GamepadButton.DpadUp,
            DpadDown = GamepadButton.DpadDown,
            DpadLeft = GamepadButton.DpadLeft,
            DpadRight = GamepadButton.DpadRight,
            
            LeftTrigger = GamepadButton.LeftTrigger,
            RightTrigger = GamepadButton.RightTrigger,
            LeftButton = GamepadButton.LeftShoulder,
            RightButton = GamepadButton.RightShoulder,
        }

        protected static Dictionary<string, AxisCode> _stringToAxis = new Dictionary<string, AxisCode>();
        protected static Dictionary<string, ButtonCode> _stringToButton = new Dictionary<string, ButtonCode>();

        Gamepad _gamepad
        {
            get
            {
                return Gamepad.current;
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            foreach (ButtonCode b in QuickUtils.GetEnumValues<ButtonCode>())
            {
                _stringToButton[b.ToString()] = b;
            }

            foreach (AxisCode c in QuickUtils.GetEnumValues<AxisCode>())
            {
                _stringToAxis[c.ToString()] = c;
            }
        }

        public override void Reset()
        {
            base.Reset();

            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, AxisCode.RightStick_Horizontal.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, AxisCode.RightStick_Vertical.ToString());

            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCode.South.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, ButtonCode.East.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, ButtonCode.Select.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, ButtonCode.North.ToString());
        }

        #endregion

        #region GET AND SET

        public override string[] GetAxisCodes()
        {
            return GetCodes<AxisCode>();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<ButtonCode>();
        }

        protected override float ImpGetAxis(string axisName)
        {
            float aValue = 0;
            if (_gamepad != null)
            {
                aValue = GetAxisValue(_stringToAxis[axisName]);
            }

            return aValue;
        }

        protected override bool ImpGetButton(string buttonName)
        {
            bool result = false; 
            if (_gamepad != null)
            {
                GamepadButton b = (GamepadButton)_stringToButton[buttonName];
                result = _gamepad[b].isPressed;
            }

            return result;
        }

        protected float GetAxisValue(AxisCode aCode)
        {
            float result = 0;

            //Left Stick
            if (aCode == AxisCode.LeftStick_Horizontal)
            {
                float f = _gamepad.leftStick.right.ReadValue();
                result = f > 0 ? f : -1 * _gamepad.leftStick.left.ReadValue();
            }
            else if (aCode == AxisCode.LeftStick_Vertical)
            {
                float f = _gamepad.leftStick.up.ReadValue();
                result = f > 0 ? f : -1 * _gamepad.leftStick.down.ReadValue();
            }

            //Right Stick
            else if (aCode == AxisCode.RightStick_Horizontal)
            {
                float f = _gamepad.rightStick.right.ReadValue();
                result = f > 0 ? f : -1 * _gamepad.rightStick.left.ReadValue();
            }
            else if (aCode == AxisCode.RightStick_Vertical)
            {
                float f = _gamepad.rightStick.up.ReadValue();
                result = f > 0 ? f : -1 * _gamepad.rightStick.down.ReadValue();
            }

            //Left Trigger
            else if (aCode == AxisCode.LeftTrigger)
            {
                result = _gamepad.leftTrigger.ReadValue();
            }

            //Right Trigger
            else if (aCode == AxisCode.RightTrigger)
            {
                result = _gamepad.rightTrigger.ReadValue();
            }

            return Mathf.Abs(result) > _deadZone? result : 0;
        }

        #endregion

    }

}
