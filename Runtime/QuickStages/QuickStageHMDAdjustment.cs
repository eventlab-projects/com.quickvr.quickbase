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
            _guiCalibration.gameObject.SetActive(true);

            if (QuickVRManager.IsXREnabled())
            {

#if UNITY_ANDROID
                if (_hTracking._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands)
                {
                    _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.HandTrackingMode, _hTracking._handTrackingMode);

                    while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
                    {
                        yield return null;
                    }
                }
#endif
                _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.HMDAdjustment, _hTracking._handTrackingMode);
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


