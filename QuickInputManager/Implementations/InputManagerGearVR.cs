using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class InputManagerGearVR : BaseInputManager
    {

        public enum ButtonCodes
        {
            BUTTON_PAD = 0,
            BUTTON_BACK = 1,
        }

        #region GET AND SET

        public override string[] GetAxisCodes()
        {
            List<string> aCodes = new List<string>();
            aCodes.Add(BaseInputManager.NULL_MAPPING);

            return aCodes.ToArray();
        }

        public override string[] GetButtonCodes()
        {
            List<string> bCodes = new List<string>();
            bCodes.Add(BaseInputManager.NULL_MAPPING);
            bCodes.Add(InputManagerGearVR.ButtonCodes.BUTTON_PAD.ToString());
            bCodes.Add(InputManagerGearVR.ButtonCodes.BUTTON_BACK.ToString());

            return bCodes.ToArray();
        }

        #endregion

        #region INPUT MANAGEMENT

        protected override float ImpGetAxis(string axis)
        {
            float aValue = 0.0f;

            return aValue;
        }

        protected override bool ImpGetButton(string button)
        {
            if (button == ButtonCodes.BUTTON_PAD.ToString()) return Input.GetMouseButton((int)ButtonCodes.BUTTON_PAD);
            if (button == ButtonCodes.BUTTON_BACK.ToString()) return Input.GetMouseButton((int)ButtonCodes.BUTTON_BACK);
            //if (_trackedControllerLeft)
            //{
            //    if (button == "LEFT_TRIGGER") return _trackedControllerLeft.triggerPressed;
            //    if (button == "LEFT_STEAM") return _trackedControllerLeft.steamPressed;
            //    if (button == "LEFT_MENU") return _trackedControllerLeft.menuPressed;
            //    if (button == "LEFT_PAD") return _trackedControllerLeft.padPressed;
            //    if (button == "LEFT_GRIP") return _trackedControllerLeft.gripped;
            //}

            //if (_trackedControllerRight)
            //{
            //    if (button == "RIGHT_TRIGGER") return _trackedControllerRight.triggerPressed;
            //    if (button == "RIGHT_STEAM") return _trackedControllerRight.steamPressed;
            //    if (button == "RIGHT_MENU") return _trackedControllerRight.menuPressed;
            //    if (button == "RIGHT_PAD") return _trackedControllerRight.padPressed;
            //    if (button == "RIGHT_GRIP") return _trackedControllerRight.gripped;
            //}

            return false;
        }

        #endregion
    }

}
