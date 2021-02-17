using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

using QuickVR;

public class TestVibration : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(CoUpdate());
    }

    protected virtual IEnumerator CoUpdate()
    {
        while (true)
        {
            QuickVibratorManager.Vibrate(QuickVibratorManager.DEFAULT_VIBRATOR_LEFT_HAND);
            yield return new WaitForSeconds(2.0f);
        }

        //while (true)
        //{
        //    InputDevice iDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        //    if (iDevice != null)
        //    {
        //        iDevice.SendHapticImpulse(0, 1);
        //    }
        //    yield return null;
        //}

    }

}
