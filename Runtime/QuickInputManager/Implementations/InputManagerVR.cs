using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;


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

        protected static List<ButtonCodes> _buttons = new List<ButtonCodes>();
        protected static Dictionary<ButtonCodes, InputManager.VirtualButtonState> _buttonState = new Dictionary<ButtonCodes, InputManager.VirtualButtonState>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            _buttons = QuickUtils.GetEnumValues<ButtonCodes>();
            foreach (ButtonCodes bCode in _buttons)
            {
                _buttonState[bCode] = InputManager.VirtualButtonState.Idle;
            }

            base.Awake();
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

        protected override float ImpGetAxis(AxisCode axis)
        {
            return GetAxis(axis);
        }

        protected override bool ImpGetButton(ButtonCodes button)
        {
            return GetKey(button);
        }

        private static InputDevice GetXRController(AxisCode axis)
        {
            return InputDevices.GetDeviceAtXRNode(axis <= AxisCode.LeftGrip ? XRNode.LeftHand : XRNode.RightHand);
        }

        private static InputDevice GetXRController(ButtonCodes key)
        {
            return InputDevices.GetDeviceAtXRNode(key <= ButtonCodes.LeftGripTouch ? XRNode.LeftHand : XRNode.RightHand);
        }

        public static float GetAxis(AxisCode axis)
        {
            InputDevice controller = GetXRController(axis);

            float result = 0;

            if (controller != null)
            {
                if (axis == AxisCode.LeftStick_Horizontal || axis == AxisCode.RightStick_Horizontal)
                {
                    if (controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 tmp))
                    {
                        result = tmp.x;
                    }
                }
                else if (axis == AxisCode.LeftStick_Vertical || axis == AxisCode.RightStick_Vertical)
                {
                    if (controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 tmp))
                    {
                        result = tmp.y;
                    }
                }
                else if (axis == AxisCode.LeftTrigger || axis == AxisCode.RightTrigger)
                {
                    controller.TryGetFeatureValue(CommonUsages.trigger, out result);
                }
                else if (axis == AxisCode.LeftGrip || axis == AxisCode.RightGrip)
                {
                    controller.TryGetFeatureValue(CommonUsages.grip, out result);
                }
            }

            return result;
        }

        public static bool GetKeyDown(ButtonCodes key)
        {
            return _buttonState[key] == InputManager.VirtualButtonState.Triggered;
        }

        public static bool GetKey(ButtonCodes button)
        {
            bool result = false;
            InputDevice controller = GetXRController(button);

            if (button == ButtonCodes.LeftPrimaryPress || button == ButtonCodes.RightPrimaryPress)
            {
                controller.TryGetFeatureValue(CommonUsages.primaryButton, out result);
            }
            else if (button == ButtonCodes.LeftPrimaryTouch || button == ButtonCodes.RightPrimaryTouch)
            {
                controller.TryGetFeatureValue(CommonUsages.primaryTouch, out result);
            }
            else if (button == ButtonCodes.LeftSecondaryPress || button == ButtonCodes.RightSecondaryPress)
            {
                controller.TryGetFeatureValue(CommonUsages.secondaryButton, out result);
            }
            else if (button == ButtonCodes.LeftSecondaryTouch || button == ButtonCodes.RightSecondaryTouch)
            {
                controller.TryGetFeatureValue(CommonUsages.secondaryTouch, out result);
            }
            else if (button == ButtonCodes.LeftStickPress || button == ButtonCodes.RightStickPress)
            {
                controller.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out result);
            }
            else if (button == ButtonCodes.LeftStickTouch || button == ButtonCodes.RightStickTouch)
            {
                controller.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out result);
            }
            else if (button == ButtonCodes.LeftTriggerPress || button == ButtonCodes.RightTriggerPress)
            {
                controller.TryGetFeatureValue(CommonUsages.triggerButton, out result);
            }
            else if (button == ButtonCodes.LeftTriggerTouch || button == ButtonCodes.RightTriggerTouch)
            {
                controller.TryGetFeatureValue(QuickVRUsages.triggerTouch, out result);
            }
            else if (button == ButtonCodes.LeftGripPress || button == ButtonCodes.RightGripPress)
            {
                controller.TryGetFeatureValue(CommonUsages.gripButton, out result);
            }
            else if (button == ButtonCodes.LeftGripTouch || button == ButtonCodes.RightGripTouch)
            {

            }

            return result;
        }

        public static bool GetKeyUp(ButtonCodes key)
        {
            return _buttonState[key] == InputManager.VirtualButtonState.Released;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            foreach (ButtonCodes bCode in _buttons)
            {
                _buttonState[bCode] = InputManager.GetNextState(_buttonState[bCode], GetKey(bCode));
            }
        }

        #endregion

    }

}

