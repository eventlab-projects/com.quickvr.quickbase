using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageCheckInternetConnection : QuickStagePreBase
    {

        #region CREATION AND DESTRUCTIOn

        protected override void Awake()
        {
            base.Awake();

            _avoidable = false;
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            if (!QuickUtils.IsInternetConnection())
            {
                _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.InternetConnectionRequired);

                while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
                {
                    yield return null;
                }

                QuickUtils.CloseApplication();
            }
        }

        #endregion

    }

}