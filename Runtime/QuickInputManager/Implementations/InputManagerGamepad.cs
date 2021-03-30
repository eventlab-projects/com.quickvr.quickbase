using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerGamepad : InputManagerGeneric<Gamepad, InputManagerGamepad.AxisCode, InputManagerGamepad.ButtonCode>
    {

        #region PUBLIC ATTRIBUTES

        public enum AxisCode
        {
            LeftStick_Horizontal,
            LeftStick_Vertical,

            RightStick_Horizontal,
            RightStick_Vertical,

            LeftTrigger,
            RightTrigger,
        };

        public enum ButtonCode
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

        public float _deadZone = 0.2f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, AxisCode.RightStick_Horizontal.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, AxisCode.LeftStick_Vertical.ToString());

            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCode.South.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, ButtonCode.East.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_EXIT, ButtonCode.Select.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, ButtonCode.North.ToString());
        }

        #endregion

        #region GET AND SET

        protected override Gamepad GetInputDevice()
        {
            return Gamepad.current;
        }

        protected override float ImpGetAxis(AxisCode axis)
        {
            float result = 0;

            //Left Stick
            if (axis == AxisCode.LeftStick_Horizontal)
            {
                float f = _inputDevice.leftStick.right.ReadValue();
                result = f > 0 ? f : -1 * _inputDevice.leftStick.left.ReadValue();
            }
            else if (axis == AxisCode.LeftStick_Vertical)
            {
                float f = _inputDevice.leftStick.up.ReadValue();
                result = f > 0 ? f : -1 * _inputDevice.leftStick.down.ReadValue();
            }

            //Right Stick
            else if (axis == AxisCode.RightStick_Horizontal)
            {
                float f = _inputDevice.rightStick.right.ReadValue();
                result = f > 0 ? f : -1 * _inputDevice.rightStick.left.ReadValue();
            }
            else if (axis == AxisCode.RightStick_Vertical)
            {
                float f = _inputDevice.rightStick.up.ReadValue();
                result = f > 0 ? f : -1 * _inputDevice.rightStick.down.ReadValue();
            }

            //Left Trigger
            else if (axis == AxisCode.LeftTrigger)
            {
                result = _inputDevice.leftTrigger.ReadValue();
            }

            //Right Trigger
            else if (axis == AxisCode.RightTrigger)
            {
                result = _inputDevice.rightTrigger.ReadValue();
            }

            return Mathf.Abs(result) > _deadZone ? result : 0;
        }

        protected override bool ImpGetButton(ButtonCode button)
        {
            return _inputDevice[(GamepadButton)button].isPressed;
        }

        #endregion

    }

}
