using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.Controls;

namespace QuickVR
{
    public class InputManagerVR : InputManagerGeneric<XRController, InputManagerVR.AxisCode, InputManagerVR.ButtonCodes>
    {

        #region PUBLIC ATTRIBUTES

        public enum AxisCode
        {
            //Left Controller
            LeftStick_Horizontal,
            LeftStick_Vertical,

            LeftTrigger,

            LeftGrip,

            //Right Controller
            RightStick_Horizontal,
            RightStick_Vertical,

            RightTrigger,

            RightGrip,
        }

        public enum ButtonCodes
        {
            //Left Controller
            LeftPrimaryPress,
            LeftPrimaryTouch,

            LeftSecondaryPress,
            LeftSecondaryTouch,

            LeftStickPress,
            LeftStickTouch,

            LeftTriggerPress,
            LeftTriggerTouch,

            LeftGripPress,
            LeftGripTouch,

            //Right Controller
            RightPrimaryPress,
            RightPrimaryTouch,

            RightSecondaryPress,
            RightSecondaryTouch,

            RightStickPress,
            RightStickTouch,

            RightTriggerPress,
            RightTriggerTouch,

            RightGripPress,
            RightGripTouch,
        }

        public float _deadZone = 0.2f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Dictionary<AxisCode, string> _toAxisControl = new Dictionary<AxisCode, string>();
        protected Dictionary<ButtonCodes, string> _toButtonControl = new Dictionary<ButtonCodes, string>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            //Axis controls
            _toAxisControl[AxisCode.LeftStick_Horizontal] = _toAxisControl[AxisCode.RightStick_Horizontal] = "thumbstick";
            _toAxisControl[AxisCode.LeftStick_Vertical] = _toAxisControl[AxisCode.RightStick_Vertical] = "thumbstick";

            _toAxisControl[AxisCode.LeftTrigger] = _toAxisControl[AxisCode.RightTrigger] = "trigger";

            _toAxisControl[AxisCode.LeftGrip] = _toAxisControl[AxisCode.RightGrip] = "grip";

            //Buton controls
            _toButtonControl[ButtonCodes.LeftPrimaryPress] = _toButtonControl[ButtonCodes.RightPrimaryPress] = "primarybutton";
            _toButtonControl[ButtonCodes.LeftPrimaryTouch] = _toButtonControl[ButtonCodes.RightPrimaryTouch] = "primarytouched";

            _toButtonControl[ButtonCodes.LeftSecondaryPress] = _toButtonControl[ButtonCodes.RightSecondaryPress] = "secondarybutton";
            _toButtonControl[ButtonCodes.LeftSecondaryTouch] = _toButtonControl[ButtonCodes.RightSecondaryTouch] = "secondarytouched";

            _toButtonControl[ButtonCodes.LeftStickPress] = _toButtonControl[ButtonCodes.RightStickPress] = "thumbstickclicked";
            _toButtonControl[ButtonCodes.LeftStickTouch] = _toButtonControl[ButtonCodes.RightStickTouch] = "thumbsticktouched";

            _toButtonControl[ButtonCodes.LeftTriggerPress] = _toButtonControl[ButtonCodes.RightTriggerPress] = "triggerpressed";
            _toButtonControl[ButtonCodes.LeftTriggerTouch] = _toButtonControl[ButtonCodes.RightTriggerTouch] = "triggertouched";

            _toButtonControl[ButtonCodes.LeftGripPress] = _toButtonControl[ButtonCodes.RightGripPress] = "grippressed";
            _toButtonControl[ButtonCodes.LeftGripTouch] = _toButtonControl[ButtonCodes.RightGripTouch] = "grippressed";

            base.Awake();
        }

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCodes.RightTriggerPress.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CALIBRATE, ButtonCodes.LeftSecondaryPress.ToString());
        }

        #endregion

        #region GET AND SET

        protected override XRController GetInputDevice()
        {
            return null;
        }

        protected override void SetInputDevice(AxisCode axis)
        {
            _inputDevice = (axis <= AxisCode.LeftGrip) ? XRController.leftHand : XRController.rightHand;
        }

        protected override void SetInputDevice(ButtonCodes button)
        {
            _inputDevice = (button <= ButtonCodes.LeftGripTouch) ? XRController.leftHand : XRController.rightHand;
        }

        protected override float ImpGetAxis(AxisCode axis)
        {
            AxisControl aControl = null;

            if (axis == AxisCode.LeftStick_Horizontal || axis == AxisCode.RightStick_Horizontal)
            {
                aControl = _inputDevice.GetChildControl<Vector2Control>(_toAxisControl[axis]).x;
            }
            else if (axis == AxisCode.LeftStick_Vertical || axis == AxisCode.RightStick_Vertical)
            {
                aControl = _inputDevice.GetChildControl<Vector2Control>(_toAxisControl[axis]).y;
            }
            else
            {
                aControl = _inputDevice.GetChildControl<AxisControl>(_toAxisControl[axis]);
            }

            float aValue = aControl.ReadValue();
            return Mathf.Abs(aValue) > _deadZone ? aValue : 0;
        }

        protected override bool ImpGetButton(ButtonCodes button)
        {
            return _inputDevice.GetChildControl<ButtonControl>(_toButtonControl[button]).isPressed;
        }

        #endregion

    }

}

