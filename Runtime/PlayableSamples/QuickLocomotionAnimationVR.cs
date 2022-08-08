using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace QuickVR
{

    public class QuickLocomotionAnimationVR : QuickLocomotionAnimationBase
    {

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            QuickVRManager.OnPreCopyPose += UpdateFeetIKTargets;
            QuickVRManager.OnPostCopyPose += UpdateFeetIKSolvers;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            QuickVRManager.OnPreCopyPose -= UpdateFeetIKTargets;
            QuickVRManager.OnPostCopyPose -= UpdateFeetIKSolvers;
        }

        #endregion

        #region UPDATE

        protected override void LateUpdate()
        {
            
        }

        #endregion

    }

}


