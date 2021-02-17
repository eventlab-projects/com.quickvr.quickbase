using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public class QuickStagePreBase : QuickStageBase
    {

        #region PROTECTED ATTRIBUTES

        protected QuickUserGUICalibration _guiCalibration = null;
        protected QuickUnityVR _hTracking = null;
        protected QuickVRManager _vrManager = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            _guiCalibration = QuickSingletonManager.GetInstance<QuickUserGUICalibration>();
            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();

            base.Awake();
        }

        public override void Init()
        {
            _hTracking = _gameManager.GetPlayer().GetComponent<QuickUnityVR>();

            base.Init();
        }

        #endregion

    }

}


