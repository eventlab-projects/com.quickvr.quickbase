using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{

    public class QuickXRRig : XRRig
    {

        protected new void Awake()
        {

        }

        protected new IEnumerator Start()
        {
            while (!QuickVRCameraController.GetCamera())
            {
                yield return null;
            }

            Camera cam = QuickVRCameraController.GetCamera();
            cameraGameObject = cam.gameObject;
            cameraFloorOffsetObject = cam.transform.parent.gameObject;
        }

    }

}


