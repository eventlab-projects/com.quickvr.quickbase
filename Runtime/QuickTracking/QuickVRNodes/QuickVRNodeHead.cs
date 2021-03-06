using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{
    
    public class QuickVRNodeHead : QuickVRNode
    {

        #region GET AND SET

        protected override string GetVRModelName()
        {
            return "pf_Generic_HMD";
        }

        #endregion

        #region UPDATE

        protected override InputDevice CheckDevice()
        {
            return InputDevices.GetDeviceAtXRNode(XRNode.Head);
        }

        #endregion


    }

}


