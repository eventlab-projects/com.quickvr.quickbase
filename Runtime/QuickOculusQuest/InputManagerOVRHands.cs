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

        protected QuickUnityVR _unityVR = null;
        protected QuickOVRHandsInitializer _ovrHands
        {
            get
            {
                if (!m_OVRHands && _unityVR)
                {
                    m_OVRHands = _unityVR.GetComponent<QuickOVRHandsInitializer>();
                }

                return m_OVRHands;
            }
        }
        protected QuickOVRHandsInitializer m_OVRHands = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected override void ResetDefaultConfiguration()
        {
            //Configure the default buttons
            ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CONTINUE, ButtonCodes.ThumbUp + "Right", ButtonCodes.ThumbUp + "Left");
            //ConfigureDefaultButton(InputManager.DEFAULT_BUTTON_CANCEL, ButtonCodes.ThumbDown + "Right", ButtonCodes.ThumbDown + "Left");
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnSourceAnimatorSet += ActionSourceAnimatorSet;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnSourceAnimatorSet += ActionSourceAnimatorSet;
        }

#endregion

        #region GET AND SET

        protected virtual void ActionSourceAnimatorSet(Animator animator)
        {
            _unityVR = animator.GetComponent<QuickUnityVR>();
        }

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
            if (QuickVRManager.IsHandTrackingSupported() && _ovrHands)
            {
                QuickOVRHand h = GetOVRhand(button);
                if (h.IsInitialized())
                {
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
            }
            
            return false;
        }

        #endregion

//#region UPDATE

//        protected virtual void Update()
//        {
//            if (_unityVR)
//            {
//                _active = _unityVR._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands;
//            }
//        }

//#endregion

    }

}
