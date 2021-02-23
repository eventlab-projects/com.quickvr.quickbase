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
            while (!Camera.main)
            {
                yield return null;
            }

            cameraGameObject = Camera.main.gameObject;
            cameraFloorOffsetObject = Camera.main.transform.parent.gameObject;
        }

    }

}


