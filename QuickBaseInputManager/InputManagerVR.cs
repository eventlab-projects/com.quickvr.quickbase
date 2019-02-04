using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR;

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

        #region GET AND SET

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
            if (QuickUnityVRBase._handsSwaped)
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
            float axisPressedThreshold = 0.6f;
            if (button == ButtonCodes.TriggerIndexPressLeft.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerIndexLeft.ToString()) > axisPressedThreshold;
            }
            if (button == ButtonCodes.TriggerHandPressLeft.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerHandLeft.ToString()) > axisPressedThreshold;
            }
            if (button == ButtonCodes.TriggerIndexPressRight.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerIndexRight.ToString()) > axisPressedThreshold;
            }
            if (button == ButtonCodes.TriggerHandPressRight.ToString())
            {
                return ImpGetAxis(AxisCodes.TriggerHandRight.ToString()) > axisPressedThreshold;
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

        #endregion

    }

}
