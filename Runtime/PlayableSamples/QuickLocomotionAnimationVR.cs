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

            QuickVRManager.OnPostUpdateIKTargets += UpdateFeetIKTargets;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            QuickVRManager.OnPostUpdateIKTargets -= UpdateFeetIKTargets;
        }

        #endregion

        #region UPDATE

        protected override void UpdateFootIKTarget(bool isLeft)
        {
            base.UpdateFootIKTarget(isLeft);

            QuickIKSolver ikSolver = _ikManager.GetIKSolver(isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
            QuickIKManager ikManagerSrc = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickIKManager>();
            QuickIKSolver ikSolverSrc = ikManagerSrc.GetIKSolver(isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
            ikSolverSrc._targetLimb.localPosition = ikSolver._targetLimb.localPosition;
            ikSolverSrc._targetLimb.localRotation = ikSolver._targetLimb.localRotation;
        }

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


