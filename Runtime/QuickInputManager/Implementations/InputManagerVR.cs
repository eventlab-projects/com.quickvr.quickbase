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

        public float _deadZoneThumbStick = 0.2f;
        public float _deadZoneIndexTrigger = 0.1f;

        #endregion

        #region PROTECTED ATTRIBUTES

        private static Dictionary<AxisCode, string[]> _toAxisControl = new Dictionary<AxisCode, string[]>();
        private static Dictionary<ButtonCodes, string[]> _toButtonControl = new Dictionary<ButtonCodes, string[]>();

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            //Axis controls
            _toAxisControl[AxisCode.LeftStick_Horizontal] = _toAxisControl[AxisCode.RightStick_Horizontal] = new string[]{ "thumbstick", "trackpad"};
            _toAxisControl[AxisCode.LeftStick_Vertical] = _toAxisControl[AxisCode.RightStick_Vertical] = new string[] { "thumbstick", "trackpad" };

            _toAxisControl[AxisCode.LeftTrigger] = _toAxisControl[AxisCode.RightTrigger] = new string[] { "trigger" };

            _toAxisControl[AxisCode.LeftGrip] = _toAxisControl[AxisCode.RightGrip] = new string[] { "grip" };

            //Buton controls
            _toButtonControl[ButtonCodes.LeftPrimaryPress] = _toButtonControl[ButtonCodes.RightPrimaryPress] = new string[] { "primarybutton" };
            _toButtonControl[ButtonCodes.LeftPrimaryTouch] = _toButtonControl[ButtonCodes.RightPrimaryTouch] = new string[] { "primarytouched" };

            _toButtonControl[ButtonCodes.LeftSecondaryPress] = _toButtonControl[ButtonCodes.RightSecondaryPress] = new string[] { "secondarybutton" };
            _toButtonControl[ButtonCodes.LeftSecondaryTouch] = _toButtonControl[ButtonCodes.RightSecondaryTouch] = new string[] { "secondarytouched" };

            _toButtonControl[ButtonCodes.LeftStickPress] = _toButtonControl[ButtonCodes.RightStickPress] = new string[] { "thumbstickclicked", "trackpadclicked" };
            _toButtonControl[ButtonCodes.LeftStickTouch] = _toButtonControl[ButtonCodes.RightStickTouch] = new string[] { "thumbsticktouched", "trackpadtouched" };

            _toButtonControl[ButtonCodes.LeftTriggerPress] = _toButtonControl[ButtonCodes.RightTriggerPress] = new string[] { "triggerpressed" };
            _toButtonControl[ButtonCodes.LeftTriggerTouch] = _toButtonControl[ButtonCodes.RightTriggerTouch] = new string[] { "triggertouched" };

            _toButtonControl[ButtonCodes.LeftGripPress] = _toButtonControl[ButtonCodes.RightGripPress] = new string[] { "grippressed", "gripbutton" };
            _toButtonControl[ButtonCodes.LeftGripTouch] = _toButtonControl[ButtonCodes.RightGripTouch] = new string[] { "grippressed" };
        }

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default axes
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_HORIZONTAL, AxisCode.RightStick_Horizontal.ToString());
            ConfigureDefaultAxis(InputManager.DEFAULT_AXIS_VERTICAL, AxisCode.LeftStick_Vertical.ToString());

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

        private static InputControl GetInputControlAxis(XRController controller, AxisCode axis)
        {
            InputControl result = null;
            string[] values = _toAxisControl[axis];
            for (int i = 0; result == null && i < values.Length; i++)
            {
                result = controller.TryGetChildControl(values[i]);
            }

            return result;
        }

        private static ButtonControl GetInputControlButton(XRController controller, ButtonCodes button)
        {
            InputControl result = null;
            string[] values = _toButtonControl[button];
            for (int i = 0; result == null && i < values.Length; i++)
            {
                result = controller.TryGetChildControl(values[i]);
            }

            return (ButtonControl)result;
        }

        protected override float ImpGetAxis(AxisCode axis)
        {
            InputControl tmp = GetInputControlAxis(_inputDevice, axis);
            float aValue = 0;
            float dZone = 0;

            if (tmp != null)
            {
                AxisControl aControl = null;
                
                if (axis == AxisCode.LeftStick_Horizontal || axis == AxisCode.RightStick_Horizontal)
                {
                    aControl = ((Vector2Control)tmp).x;
                    dZone = _deadZoneThumbStick;
                }
                else if (axis == AxisCode.LeftStick_Vertical || axis == AxisCode.RightStick_Vertical)
                {
                    aControl = ((Vector2Control)tmp).y;
                    dZone = _deadZoneThumbStick;
                }
                else
                {
                    aControl = (AxisControl)tmp;
                    dZone = _deadZoneIndexTrigger;
                }

                aValue = aControl.ReadValue();
            }

            return Mathf.Abs(aValue) > dZone ? aValue : 0;
        }

        protected override bool ImpGetButton(ButtonCodes button)
        {
            InputControl tmp = GetInputControlButton(_inputDevice, button);

            return tmp != null ? ((ButtonControl)tmp).isPressed : false;
        }

        private static XRController GetXRController(AxisCode axis)
        {
            return axis <= AxisCode.LeftGrip ? XRController.leftHand : XRController.rightHand;
        }

        private static XRController GetXRController(ButtonCodes key)
        {
            return key <= ButtonCodes.LeftGripTouch ? XRController.leftHand : XRController.rightHand;
        }

        public static float GetAxis(AxisCode axis)
        {
            XRController controller = GetXRController(axis);

            if (controller != null)
            {
                InputControl tmp = GetInputControlAxis(controller, axis);
                AxisControl aControl = null;

                if (axis == AxisCode.LeftStick_Horizontal || axis == AxisCode.RightStick_Horizontal)
                {
                    aControl = ((Vector2Control)tmp).x;
                }
                else if (axis == AxisCode.LeftStick_Vertical || axis == AxisCode.RightStick_Vertical)
                {
                    aControl = ((Vector2Control)tmp).y;
                }
                else
                {
                    aControl = (AxisControl)tmp;
                }

                return aControl.ReadValue();
            }

            return 0;
        }

        public static bool GetKeyDown(ButtonCodes key)
        {
            XRController controller = GetXRController(key);

            if (controller != null)
            {
                ButtonControl bControl = GetInputControlButton(controller, key);
                return bControl != null && bControl.wasPressedThisFrame;
            }

            return false;
        }

        public static bool GetKey(ButtonCodes key)
        {
            XRController controller = GetXRController(key);

            if (controller != null)
            {
                ButtonControl bControl = GetInputControlButton(controller, key);
                return bControl != null && bControl.isPressed;
            }

            return false;
        }

        public static bool GetKeyUp(ButtonCodes key)
        {
            XRController controller = GetXRController(key);

            if (controller != null)
            {
                ButtonControl bControl = GetInputControlButton(controller, key);
                return bControl != null && bControl.wasReleasedThisFrame;
            }

            return false;
        }

        #endregion

    }

}

