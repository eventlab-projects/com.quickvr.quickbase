using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace QuickVR
{

    public class QuickLocomotionAnimationVR : QuickLocomotionAnimationBase
    {

        #region PROTECTED ATTRIBUTES

        protected QuickUnityVR _unityVR
        {
            get
            {
                if (!m_UnityVR)
                {
                    m_UnityVR = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickUnityVR>();
                }

                return m_UnityVR;
            }
        }
        protected QuickUnityVR m_UnityVR = null;

        #endregion

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

        protected override void Update()
        {
            _weight = _unityVR._isSitting ? 0 : 1;

            base.Update();
        }

        protected override void LateUpdate()
        {
            
        }

        #endregion

    }

}


