using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public class QuickStagePreBase : QuickStageBase
    {

        #region PROTECTED ATTRIBUTES

        protected QuickUserGUICalibration _guiCalibration = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            _guiCalibration = QuickSingletonManager.GetInstance<QuickUserGUICalibration>();

            base.Awake();
        }

        #endregion

    }

}


