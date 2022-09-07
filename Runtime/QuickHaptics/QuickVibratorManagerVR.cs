using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickVibratorManagerVR : QuickBaseVibratorManager
    {

        #region PUBLIC ATTRIBUTES

        public enum VibratorCodes
        {
            LeftController,
            RightController,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Coroutine _coVibrate = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
#if UNITY_WEBGL
            gameObject.SetActive(false);
#endif
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCalibrate += OnCalibrateHMD;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCalibrate -= OnCalibrateHMD;
        }

        public override void Reset()
        {
            base.Reset();

            //Configure the default axes
            _vibratorMapping[0] = VibratorCodes.LeftController.ToString();
            _vibratorMapping[1] = VibratorCodes.RightController.ToString();
        }

        #endregion

        #region GET AND SET

        protected virtual XRNode? ToXRNode(string vibrator)
        {
            if (vibrator == VibratorCodes.LeftController.ToString()) return XRNode.LeftHand;
            if (vibrator == VibratorCodes.RightController.ToString()) return XRNode.RightHand;

            return null;
        }

        protected virtual void OnCalibrateHMD()
        {
            
        }

        public override string[] GetVibratorCodes()
        {
            List<string> result = new List<string>(base.GetVibratorCodes());
            result.AddRange(QuickUtils.GetEnumValuesToString<VibratorCodes>());

            return result.ToArray();
        }

        public override void ImpVibrate(string vibrator)
        {
            ImpStopVibrating(vibrator);

            XRNode? nodeType = ToXRNode(vibrator);
            if (nodeType.HasValue)
            {
                _coVibrate = StartCoroutine(CoVibrate(nodeType.Value));
            }
        }

        public override void ImpStopVibrating(string vibrator)
        {
            if (_coVibrate != null)
            {
                StopCoroutine(_coVibrate);
                _coVibrate = null;
            }

            XRNode? nodeType = ToXRNode(vibrator);
            if (nodeType.HasValue)
            {
                InputDevices.GetDeviceAtXRNode(nodeType.Value).StopHaptics();
            }
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoVibrate(XRNode nodeType)
        {
            InputDevice iDevice = InputDevices.GetDeviceAtXRNode(nodeType);
            while (true)
            {
                iDevice.SendHapticImpulse(0, 1, 0.1f);
                yield return null;
            }
        }

        #endregion

    }

}
