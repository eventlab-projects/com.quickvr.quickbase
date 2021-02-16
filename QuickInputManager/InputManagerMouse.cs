using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    public class InputManagerMouse : BaseInputManager
    {

        #region PUBLIC ATTRIBUTES

        public float _mouseSensitivity = 0.1f;

        #endregion

        #region PROTECTED PARAMETERS

        protected enum AxisCode
        {
            HorizontalDelta,
            VerticalDelta,
        }

        protected enum ButtonCode
        {
            Left,
            Right,
            Middle,
        }

        protected static Dictionary<string, AxisCode> _stringToAxis = new Dictionary<string, AxisCode>();
        protected static Dictionary<string, ButtonCode> _stringToButton = new Dictionary<string, ButtonCode>();

        protected Mouse _mouse
        {
            get
            {
                return Mouse.current;
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            foreach (ButtonCode k in QuickUtils.GetEnumValues<ButtonCode>())
            {
                _stringToButton[k.ToString()] = k;
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
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, AxisCode.HorizontalDelta.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, AxisCode.VerticalDelta.ToString());
            
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCode.Left.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, ButtonCode.Right.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, ButtonCode.Middle.ToString());
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

        protected override float ImpGetAxis(string axis)
        {
            float aValue = 0;

            if (_mouse != null)
            {
                AxisCode aCode = _stringToAxis[axis];
                if (aCode == AxisCode.HorizontalDelta)
                {
                    aValue = _mouse.delta.x.ReadValue() * _mouseSensitivity;
                }
                else if (aCode == AxisCode.VerticalDelta)
                {
                    aValue = _mouse.delta.y.ReadValue() * _mouseSensitivity;
                }
            }

            return aValue;
        }

        protected override bool ImpGetButton(string button)
        {
            bool result = false; 
            if (_mouse != null)
            {
                ButtonCode bCode = _stringToButton[button];
                if (bCode == ButtonCode.Left)
                {
                    result = _mouse.leftButton.isPressed;
                }
                else if (bCode == ButtonCode.Right)
                {
                    result = _mouse.rightButton.isPressed;
                }
                else if (bCode == ButtonCode.Middle)
                {
                    result = _mouse.middleButton.isPressed;
                }
            }
            
            return result;
        }

        #endregion

    }

}
