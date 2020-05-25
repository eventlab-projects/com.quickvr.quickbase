using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using WebXR;

namespace QuickVR
{
    
    [RequireComponent(typeof(WebXRController))]
    public class QuickWebXRHandlerController : QuickWebXRHandlerBase
    {

        #region PROTECTED ATTRIBUTES

        protected WebXRController _controller = null;
        protected WebXRControllerButton[] _buttonValues = null;
        protected float[] _axesValues = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            _controller = GetComponent<WebXRController>();
            _role = _controller.hand == WebXRControllerHand.LEFT ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            WebXRManager.Instance.OnControllerUpdate += OnControllerUpdate;
        }

        #endregion

        #region GET AND SET

        public virtual int GetNumAxes()
        {
            return _axesValues != null ? _axesValues.Length : 0;
        } 

        public virtual int GetNumButtons()
        {
            return _buttonValues != null ? _buttonValues.Length : 0;
        }

        public float GetAxis(int axisID)
        {
            if (axisID == 0 || axisID == 1)
            {
                //Index/Hand trigger is mapped as a button in WebXR
                WebXRControllerButton b = GetControllerButton(axisID);
                return b != null ? b.value : 0;
            }

            return _axesValues != null && axisID < _axesValues.Length ? _axesValues[axisID] : 0;
        }

        public bool GetButton(int buttonID)
        {
            return _buttonValues != null && buttonID < _buttonValues.Length? _buttonValues[buttonID].pressed : false;
        }

        protected WebXRControllerButton GetControllerButton(int buttonID)
        {
            return buttonID < _buttonValues.Length ? _buttonValues[buttonID] : null;
        }

        #endregion

        #region UPDATE

        protected virtual void OnControllerUpdate(
            string id,
            int index,
            string handValue,
            bool hasOrientation,
            bool hasPosition,
            Quaternion orientation,
            Vector3 position,
            Vector3 linearAcceleration,
            Vector3 linearVelocity,
            WebXRControllerButton[] buttonValues,
            float[] axesValues)
        {
            object hValue = QuickUtils.ParseEnum(typeof(WebXRControllerHand), handValue.ToUpper());
            if (hValue != null && (WebXRControllerHand)hValue == _controller.hand)
            {
                _buttonValues = buttonValues;
                _axesValues = axesValues;
            }
        }

        //protected virtual void Update()
        //{
        //    if (_buttonValues != null)
        //    {
        //        for (int i = 0; i < _buttonValues.Length; i++)
        //        {
        //            WebXRControllerButton b = _buttonValues[i];
        //            if (b.pressed)
        //            {
        //                Debug.Log("Button " + i.ToString() + " PRESSED!!! " + b.value.ToString("f3"));
        //            }
        //        }
        //    }
            
        //    if (_axesValues != null)
        //    {
        //        for (int i = 0; i < _axesValues.Length; i++)
        //        {
        //            float value = _axesValues[i];
        //            if (value != 0) Debug.Log("Axis " + i.ToString() + " = " + value.ToString("f3"));
        //        }
        //    }
        //}

        #endregion

    }

}


