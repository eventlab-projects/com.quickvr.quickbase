using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerMouse : InputManagerGeneric<Mouse, InputManagerMouse.AxisCode, InputManagerMouse.ButtonCode>
    {

        #region PUBLIC ATTRIBUTES

        public enum AxisCode
        {
            HorizontalDelta,
            VerticalDelta,
        }

        public enum ButtonCode
        {
            Left,
            Right,
            Middle,
        }

        public float _mouseSensitivity = 0.1f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, AxisCode.HorizontalDelta.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, AxisCode.VerticalDelta.ToString());
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCode.Left.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, ButtonCode.Right.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, ButtonCode.Middle.ToString());
        }

        #endregion

        #region GET AND SET

        protected override Mouse GetInputDevice()
        {
            return Mouse.current;
        }

        protected override float ImpGetAxis(AxisCode axis)
        {
            float result = 0;

            if (axis == AxisCode.HorizontalDelta)
            {
                result = _inputDevice.delta.x.ReadValue() * _mouseSensitivity;
            }
            else if (axis == AxisCode.VerticalDelta)
            {
                result = _inputDevice.delta.y.ReadValue() * _mouseSensitivity;
            }

            return result;
        }

        protected override bool ImpGetButton(ButtonCode button)
        {
            bool result = false;

            if (button == ButtonCode.Left)
            {
                result = _inputDevice.leftButton.isPressed;
            }
            else if (button == ButtonCode.Right)
            {
                result = _inputDevice.rightButton.isPressed;
            }
            else if (button == ButtonCode.Middle)
            {
                result = _inputDevice.middleButton.isPressed;
            }

            return result;
        }

        #endregion

    }

}
