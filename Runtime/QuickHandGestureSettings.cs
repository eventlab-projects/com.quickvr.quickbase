using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickHandGestureKey
    {
        public InputManagerVR.ButtonCodes _buttonCode;
        public bool _invert = false;

        public bool IsPressed()
        {
            bool pressed = InputManagerVR.GetKey(_buttonCode);
            return _invert ? !pressed : pressed;
        }
    }

    [CreateAssetMenu(fileName = "Data", menuName = "QuickVR/QuickHandGestureSettings", order = 1)]
    public class QuickHandGestureSettings : ScriptableObject
    {
        public List<QuickHandGestureKey> _pointingKeys = new List<QuickHandGestureKey>();
        public GestureMode _pointingGestureMode = GestureMode.AllKeys;

        public List<QuickHandGestureKey> _thumbUpKeys = new List<QuickHandGestureKey>();
        public GestureMode _thumbUpGestureMode = GestureMode.AllKeys;

        public enum GestureMode
        {
            AllKeys,    //All keys need to be pressed to produce the gesture
            AnyKey      //Pressing any key produces the gesture
        }

        public bool IsPointing()
        {
            return IsGesture(_pointingKeys, _pointingGestureMode);
        }

        public bool IsThumbUp()
        {
            return IsGesture(_thumbUpKeys, _thumbUpGestureMode);
        }

        protected bool IsGesture(List<QuickHandGestureKey> gKeys, GestureMode gMode) 
        {
            if (gKeys.Count == 0) return false;

            bool isPressed = gKeys[0].IsPressed();
            for (int i = 1; i < gKeys.Count; i++)
            {
                if (gMode == GestureMode.AllKeys)
                {
                    isPressed &= gKeys[i].IsPressed();
                }
                else
                {
                    isPressed |= gKeys[i].IsPressed();
                }
            }

            return isPressed;
        }

    }

}


