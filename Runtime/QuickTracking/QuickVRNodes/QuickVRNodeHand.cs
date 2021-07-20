using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{
    
    public class QuickVRNodeHand : QuickVRNode
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeft = true;

        #endregion

        #region GET AND SET

        protected override string GetVRModelName()
        {
            string modelName = "";
            string hmdName = QuickVRManager.GetHMDName();

            if (hmdName.Contains("vive"))
            {
                modelName = "pf_VIVE_Controller";
            }
            else if (hmdName.Contains("oculus"))
            {
                modelName = _isLeft? "pf_OculusCV1_Controller_Left" : "pf_OculusCV1_Controller_Right";
            }
            
            return modelName;
        }

        protected override bool GetDevicePosition(out Vector3 pos)
        {
            return _inputDevice.TryGetFeatureValue(QuickVRUsages.pointerPosition, out pos) || base.GetDevicePosition(out pos);
        }

        protected override bool GetDeviceRotation(out Quaternion rot)
        {
            return _inputDevice.TryGetFeatureValue(QuickVRUsages.pointerRotation, out rot) || base.GetDeviceRotation(out rot);
        }

        #endregion

        #region UPDATE

        protected override InputDevice CheckDevice()
        {
            return InputDevices.GetDeviceAtXRNode(_isLeft? XRNode.LeftHand : XRNode.RightHand);
        }

        #endregion


    }

}


