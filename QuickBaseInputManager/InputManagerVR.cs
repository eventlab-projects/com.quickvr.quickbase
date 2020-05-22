using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR;
using WebXR;
using UnityEngine.UI;

namespace QuickVR {

	[System.Serializable]
	public class InputManagerVR : BaseInputManager {

        #region PUBLIC PARAMETERS

        public enum AxisCodes
        {
            TriggerIndexLeft = 9,
            TriggerHandLeft = 11,
            PadLeftX = 1,
            PadLeftY = 2,

            TriggerIndexRight = 10,
            TriggerHandRight = 12,
            PadRightX = 4,
            PadRightY = 5,
        }

        public enum ButtonCodes
        {
            PrimaryLeft,
            SecondaryLeft,
            PadPressLeft,
            PadTouchLeft,
            TriggerIndexPressLeft,
            TriggerIndexTouchLeft,
            TriggerHandPressLeft,

            PrimaryRight,
            SecondaryRight,
            PadPressRight,
            PadTouchRight,
            TriggerIndexPressRight,
            TriggerIndexTouchRight,
            TriggerHandPressRight,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

#if UNITY_WEBGL
        protected QuickWebXRHandlersManager _wxrHandlersManager
        {
            get
            {
                return WebXRManager.Instance.transform.GetOrCreateComponent<QuickWebXRHandlersManager>();
            }
        }
#endif

        protected static Dictionary<string, AxisCodes> _toAxis = new Dictionary<string, AxisCodes>();
        protected static Dictionary<string, ButtonCodes> _toButton = new Dictionary<string, ButtonCodes>();

        #endregion

        #region CONSTANTS

        protected const float AXIS_PRESSED_THRESHOLD = 0.6f;

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            foreach (AxisCodes axis in QuickUtils.GetEnumValues<AxisCodes>())
            {
                _toAxis[axis.ToString()] = axis;
            }

            foreach (ButtonCodes button in QuickUtils.GetEnumValues<ButtonCodes>())
            {
                _toButton[button.ToString()] = button;
            }
        }

        #endregion

        #region GET AND SET

#if UNITY_WEBGL
        public virtual QuickWebXRHandlerController GetHandlerController(string name)
        {
            return _wxrHandlersManager.GetHandlerController(name.Contains("Left"));
        }
#endif

        public override string[] GetAxisCodes()
        {
            return GetCodes<AxisCodes>();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<ButtonCodes>();
        }

        public string GetRealCode(string code)
        {
            if (QuickUnityVR._handsSwaped)
            {
                if (code.Contains("Left")) code = code.Replace("Left", "Right");
                else code = code.Replace("Right", "Left");
            }

            return code;
        }

        #endregion

        #region INPUT MANAGEMENT

        protected override float ImpGetAxis(string axis)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return ImpGetAxisWebXR(axis);
#else
            return ImpGetAxisUnityVR(axis);
#endif
        }

        protected override bool ImpGetButton(string button)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return ImpGetButtonWebXR(button);
#else
            return ImpGetButtonUnityVR(button);
#endif
        }

        protected virtual float ImpGetAxisUnityVR(string axis)
        {
            return Input.GetAxis("QuickVR_" + GetRealCode(axis));
        }

        protected virtual float ImpGetAxisWebXR(string axis)
        {
            QuickWebXRHandlerController h = GetHandlerController(axis);
            AxisCodes aCode = _toAxis[axis];
            if (aCode == AxisCodes.TriggerIndexLeft || aCode == AxisCodes.TriggerIndexRight)
            {
                return h.GetAxis(0);
            }
            if (aCode == AxisCodes.TriggerHandLeft || aCode == AxisCodes.TriggerHandRight)
            {
                return h.GetAxis(1);
            }
            if (aCode == AxisCodes.PadLeftY || aCode == AxisCodes.PadRightY)
            {
                return h.GetAxis(2);
            }
            if (aCode == AxisCodes.PadLeftX || aCode == AxisCodes.PadRightX)
            {
                return h.GetAxis(3);
            }

            return 0;
        }

        protected virtual bool ImpGetButtonUnityVR(string button)
        {
            button = GetRealCode(button);

            //Axis based buttons
            if (button == ButtonCodes.TriggerIndexPressLeft.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerIndexLeft.ToString()) > AXIS_PRESSED_THRESHOLD;
            }
            if (button == ButtonCodes.TriggerHandPressLeft.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerHandLeft.ToString()) > AXIS_PRESSED_THRESHOLD;
            }
            if (button == ButtonCodes.TriggerIndexPressRight.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerIndexRight.ToString()) > AXIS_PRESSED_THRESHOLD;
            }
            if (button == ButtonCodes.TriggerHandPressRight.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerHandRight.ToString()) > AXIS_PRESSED_THRESHOLD;
            }

            //Key based buttons
            string kName = "JoystickButton";
            KeyCode k = KeyCode.None;
            if (button.Contains("Primary"))
            {
                k = QuickUtils.ParseEnum<KeyCode>(kName + (button.Contains("Left") ? "2" : "0"));
            }
            else if (button.Contains("Secondary"))
            {
                k = QuickUtils.ParseEnum<KeyCode>(kName + (button.Contains("Left") ? "3" : "1"));
            }
            else if (button.Contains("PadPress"))
            {
                k = QuickUtils.ParseEnum<KeyCode>(kName + (button.Contains("Left") ? "8" : "9"));
            }
            else if (button.Contains("PadTouch"))
            {
                k = QuickUtils.ParseEnum<KeyCode>(kName + (button.Contains("Left") ? "16" : "17"));
            }
            else if (button.Contains("TriggerIndexTouch"))
            {
                k = QuickUtils.ParseEnum<KeyCode>(kName + (button.Contains("Left") ? "14" : "15"));
            }

            return Input.GetKey(k);
        }

        protected virtual bool ImpGetButtonWebXR(string button)
        {
            QuickWebXRHandlerController h = GetHandlerController(button);
            ButtonCodes bCode = _toButton[button];

            if (bCode == ButtonCodes.TriggerIndexPressLeft || bCode == ButtonCodes.TriggerIndexPressRight)
            {
                return h.GetAxis(0) > AXIS_PRESSED_THRESHOLD;
            }
            if (bCode == ButtonCodes.TriggerHandPressLeft || bCode == ButtonCodes.TriggerHandPressRight)
            {
                return h.GetAxis(1) > AXIS_PRESSED_THRESHOLD;
            }
            if (bCode == ButtonCodes.PadPressLeft || bCode == ButtonCodes.PadPressRight)
            {
                return h.GetButton(3);
            }
            if (bCode == ButtonCodes.PrimaryLeft || bCode == ButtonCodes.PrimaryRight)
            {
                return h.GetButton(4);
            }
            if (bCode == ButtonCodes.SecondaryLeft || bCode == ButtonCodes.SecondaryRight)
            {
                return h.GetButton(5);
            }

            return false;
        }

#endregion

    }

}
