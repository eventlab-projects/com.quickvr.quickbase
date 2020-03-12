using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickCopyPoseVR : QuickCopyPoseBase
    {

        //TODO: ADD MASK

        #region PROTECTED PARAMETERS

        protected QuickVRCameraController _cameraController = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            QuickHeadTracking hTracking = FindObjectOfType<QuickHeadTracking>();
            if (hTracking)
            {
                SetAnimatorSource(hTracking.GetComponent<Animator>());
            }

            _cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();
        }

        #endregion

        #region UPDATE

        protected override void CopyPose()
        {
            base.CopyPose();

            if (_dest == null)
                return;

            _cameraController.SetAnimator(_dest);
        }

        #endregion

    }

}
