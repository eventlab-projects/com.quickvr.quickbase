using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    
    public class InputManagerWebXR : BaseInputManager
    {

        #region PROTECTED ATTRIBUTES

        protected QuickWebXRHandlersManager _wxrHandlersManager
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickWebXRHandlersManager>();
            }
        }

        #endregion

#region CREATION AND DESTRUCTION

#if UNITY_WEBGL

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        protected static void Init()
        {
            InputManagerVR iManagerVR = _inputManager.GetComponentInChildren<InputManagerVR>();
            if (iManagerVR)
            {
                InputManagerWebXR iManagerWebXR = _inputManager.transform.CreateChild("").GetOrCreateComponent<InputManagerWebXR>();
                for (int i = 0; i < iManagerVR.GetNumAxesMapped(); i++)
                {
                    AxisMapping tmp = iManagerVR.GetAxisMapping(i);
                    AxisMapping aMapping = iManagerWebXR.GetAxisMapping(i);
                    aMapping._axisCode = tmp._axisCode;
                }

                for (int i = 0; i < iManagerVR.GetNumButtonsMapped(); i++)
                {
                    ButtonMapping tmp = iManagerVR.GetButtonMapping(i);
                    ButtonMapping bMapping = iManagerWebXR.GetButtonMapping(i);
                    bMapping._keyCode = tmp._keyCode;
                    bMapping._altKeyCode = tmp._altKeyCode;
                }
            }
        }

#endif

#endregion

#region GET AND SET

        public override string[] GetAxisCodes()
        {
            return GetCodes<InputManagerVR.AxisCodes>();
        }

        public override string[] GetButtonCodes()
        {
            return GetCodes<InputManagerVR.ButtonCodes>();
        }

        public virtual QuickWebXRHandlerController GetHandlerController(string name)
        {
            return _wxrHandlersManager.GetHandlerController(name.Contains("Left"));
        }

        protected override float ImpGetAxis(string axis)
        {
            QuickWebXRHandlerController h = GetHandlerController(axis);
            InputManagerVR.AxisCodes aCode = InputManagerVR.ToAxis(axis);
            if (aCode == InputManagerVR.AxisCodes.TriggerIndexLeft || aCode == InputManagerVR.AxisCodes.TriggerIndexRight)
            {
                return h.GetAxis(0);
            }
            if (aCode == InputManagerVR.AxisCodes.TriggerHandLeft || aCode == InputManagerVR.AxisCodes.TriggerHandRight)
            {
                return h.GetAxis(1);
            }
            if (aCode == InputManagerVR.AxisCodes.PadLeftY || aCode == InputManagerVR.AxisCodes.PadRightY)
            {
                return h.GetAxis(2);
            }
            if (aCode == InputManagerVR.AxisCodes.PadLeftX || aCode == InputManagerVR.AxisCodes.PadRightX)
            {
                return h.GetAxis(3);
            }

            return 0;
        }

        protected override bool ImpGetButton(string button)
        {
            QuickWebXRHandlerController h = GetHandlerController(button);
            InputManagerVR.ButtonCodes bCode = InputManagerVR.ToButton(button);

            if (bCode == InputManagerVR.ButtonCodes.TriggerIndexPressLeft || bCode == InputManagerVR.ButtonCodes.TriggerIndexPressRight)
            {
                return h.GetAxis(0) > InputManagerVR.AXIS_PRESSED_THRESHOLD;
            }
            if (bCode == InputManagerVR.ButtonCodes.TriggerHandPressLeft || bCode == InputManagerVR.ButtonCodes.TriggerHandPressRight)
            {
                return h.GetAxis(1) > InputManagerVR.AXIS_PRESSED_THRESHOLD;
            }
            if (bCode == InputManagerVR.ButtonCodes.PadPressLeft || bCode == InputManagerVR.ButtonCodes.PadPressRight)
            {
                return h.GetButton(3);
            }
            if (bCode == InputManagerVR.ButtonCodes.PrimaryLeft || bCode == InputManagerVR.ButtonCodes.PrimaryRight)
            {
                return h.GetButton(4);
            }
            if (bCode == InputManagerVR.ButtonCodes.SecondaryLeft || bCode == InputManagerVR.ButtonCodes.SecondaryRight)
            {
                return h.GetButton(5);
            }

            return false;
        }

#endregion

    }

}


