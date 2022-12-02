using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageHMDAdjustment : QuickStagePreBase
    {

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            //perform an initial calibration. 
            _vrManager.RequestCalibration();

            _guiCalibration.gameObject.SetActive(true);

            if (QuickVRManager.IsXREnabled())
            {

//#if UNITY_ANDROID
//                if (_hTracking._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands)
//                {
//                    _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.HandTrackingMode);

//                    while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
//                    {
//                        yield return null;
//                    }
//                }
//#endif
                _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.HMDAdjustment);
                yield return new WaitForSeconds(0.5f);

                //HMD Adjustment
                while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
                {
                    yield return null;
                }

                yield return null;
            }

            _guiCalibration.gameObject.SetActive(false);
        }

        #endregion

    }

}


