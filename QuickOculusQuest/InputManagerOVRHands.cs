using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public class InputManagerOVRHands : BaseInputManager
    {

        #region PUBLIC ATTRIBUTES

        public enum ButtonCodes
        {
            IndexPinch,
            MiddlePinch,
            RingPinch,
            LittlePinch,
            ThumbUp,
            ThumbDown,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickOVRHandsInitializer _ovrHands
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickOVRHandsInitializer>();
            }
        }

        #endregion

        #region GET AND SET

        public override string[] GetButtonCodes()
        {
            string[] sufix = { "Left", "Right" };
            List<string> codes = new List<string>();
            foreach (string s in sufix)
            {
                foreach (ButtonCodes b in QuickUtils.GetEnumValues<ButtonCodes>())
                {
                    codes.Add(b.ToString() + s);
                }
            }

            return GetCodes(codes);
        }

        protected virtual QuickOVRHand GetOVRhand(string button)
        {
            return _ovrHands? _ovrHands.GetOVRHand(button.Contains("Left")) : null;
        }

        protected override float ImpGetAxis(string axis)
        {
            return 0.0f;
        }

        protected override bool ImpGetButton(string button)
        {
            if (QuickUtils.IsHandTrackingSupported() && _ovrHands)
            {
                QuickOVRHand h = GetOVRhand(button);
                //Pinching gestures
                if (button.Contains("Pinch"))
                {
                    if (button.Contains("Thumb")) return h.GetFingerIsPinching(OVRHand.HandFinger.Thumb);
                    if (button.Contains("Index")) return h.GetFingerIsPinching(OVRHand.HandFinger.Index);
                    if (button.Contains("Middle")) return h.GetFingerIsPinching(OVRHand.HandFinger.Middle);
                    if (button.Contains("Ring")) return h.GetFingerIsPinching(OVRHand.HandFinger.Ring);
                    if (button.Contains("Little")) return h.GetFingerIsPinching(OVRHand.HandFinger.Pinky);
                }
                else if (button.Contains("ThumbUp")) return h.IsThumbUp();
                else if (button.Contains("ThumbDown")) return h.IsThumbDown();
            }
            
            return false;
        }

        #endregion


    }

}
