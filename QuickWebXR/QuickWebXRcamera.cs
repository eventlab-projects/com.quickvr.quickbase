using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WebXR;

namespace QuickVR
{
    public class QuickWebXRCamera : WebXRCamera
    {

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Camera camLeft = transform.GetChild(1).GetComponent<Camera>();
            camLeft.stereoTargetEye = StereoTargetEyeMask.Left;
            camLeft.tag = "MainCamera";

            transform.GetChild(2).GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.Right;
        }

        #endregion

    }

}


