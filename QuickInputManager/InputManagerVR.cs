using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace QuickVR 
{

	[System.Serializable]
	public class InputManagerVR : BaseInputManager 
    {

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

        protected static Dictionary<string, AxisCodes> _toAxis = new Dictionary<string, AxisCodes>();
        protected static Dictionary<string, ButtonCodes> _toButton = new Dictionary<string, ButtonCodes>();

        #endregion

        #region CONSTANTS

        public const float AXIS_PRESSED_THRESHOLD = 0.6f;

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

        public override void Reset()
        {
            base.Reset();

            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCodes.TriggerIndexPressRight.ToString());
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, ButtonCodes.TriggerIndexPressLeft.ToString());
        }

        #endregion

        #region GET AND SET

        public static AxisCodes ToAxis(string aName)
        {
            return _toAxis[aName];
        }

        public static ButtonCodes ToButton(string bName)
        {
            return _toButton[bName];
        }

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
            return Input.GetAxis("QuickVR_" + GetRealCode(axis));
        }

        protected override bool ImpGetButton(string button)
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
                k = InputManagerUnity.ToKeyCode(kName + (button.Contains("Left") ? "2" : "0"));
            }
            else if (button.Contains("Secondary"))
            {
                k = InputManagerUnity.ToKeyCode(kName + (button.Contains("Left") ? "3" : "1"));
            }
            else if (button.Contains("PadPress"))
            {
                k = InputManagerUnity.ToKeyCode(kName + (button.Contains("Left") ? "8" : "9"));
            }
            else if (button.Contains("PadTouch"))
            {
                k = InputManagerUnity.ToKeyCode(kName + (button.Contains("Left") ? "16" : "17"));
            }
            else if (button.Contains("TriggerIndexTouch"))
            {
                k = InputManagerUnity.ToKeyCode(kName + (button.Contains("Left") ? "14" : "15"));
            }

            return Input.GetKey(k);
        }

        #endregion

    }

}
