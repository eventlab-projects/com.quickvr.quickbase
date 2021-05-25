using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{
    
    public class QuickVRNodeEye : QuickVRNode
    {

        #region GET AND SET

        

        #endregion

        #region UPDATE

        protected override InputDevice CheckDevice()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.HeadMounted, devices);

            return devices.Count > 0 ? devices[0] : new InputDevice();
        }

        protected override bool GetDeviceRotation(out Quaternion rot)
        {
            if (base.GetDeviceRotation(out rot)) 
            {
                if (_inputDevice.TryGetFeatureValue(QuickVRUsages.combineEyeVector, out Vector3 vEye))
                {
                    Vector3 r = transform.TransformDirection(vEye);
                    Vector3 rotAxis = Vector3.Cross(transform.forward, r);
                    float rotAngle = Vector3.Angle(transform.forward, r);
                    transform.Rotate(rotAxis, rotAngle, Space.World);
                }

                return true;
            }

            return false;
        }

        #endregion

    }

}


