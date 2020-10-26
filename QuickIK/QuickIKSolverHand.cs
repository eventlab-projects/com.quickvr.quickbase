using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolverHand : QuickIKSolver
    {

        protected Animator _animator
        {
            get
            {
                if (!m_Animator)
                {
                    m_Animator = GetComponentInParent<Animator>();
                }
                return m_Animator;
            }
        }
        protected Animator m_Animator = null;

        #region GET AND SET

        protected override Vector3 GetIKTargetHintPosition()
        {
            float r = GetChainLength();
            float maxY = _boneUpper.position.y + r;

            float t = Mathf.Clamp01((maxY - _targetLimb.position.y) / (r));

            //return Vector3.Lerp(_targetLimb.position - Vector3.up * (GetMidLength() + QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
            //return Vector3.Lerp(_boneUpper.position + Vector3.Lerp(_animator.transform.forward, Vector3.down, 0.5f).normalized * (QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
            return Vector3.Lerp(_boneMid.position - Vector3.up * (GetMidLength() + QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
        }

        #endregion

    }

}
