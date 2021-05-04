using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolverEye : QuickIKSolver
    {

        #region PUBLIC PARAMETERS

        public override Transform _boneUpper
        {
            get
            {
                if (m_boneUpper == null)
                {
                    m_boneUpper = _boneMid;
                }

                return m_boneUpper;
            }
            set
            {
                
            }
        }

        public override Transform _boneMid
        {
            get
            {
                if (m_boneMid == null)
                {
                    Animator animator = GetComponentInParent<Animator>();
                    m_boneMid = animator.GetBoneTransform(HumanBodyBones.Head);
                }

                return m_boneMid;
            }
            set
            {
                
            }
        }

        public override Transform _targetHint
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, Range(-1.0f, 1.0f)]
        protected float m_leftRight = 0;

        [SerializeField, Range(-1.0f, 1.0f)]
        protected float m_downUp = 0;

        #endregion

        #region UPDATE

        public override void UpdateIK()
        {
            //base.UpdateIK();
        }

        //public virtual void UpdateIK()
        //{
        //    if (!_enableIK || !_boneUpper || !_boneMid || !_boneLimb || !_targetLimb) return;

        //    Quaternion animBoneUpperRot = _boneUpper.rotation;
        //    Quaternion animBoneMidRot = _boneMid.rotation;
        //    Quaternion animBoneLimbRot = _boneLimb.rotation;

        //    ResetIKChain();

        //    Quaternion ikBoneUpperRot, ikBoneMidRot, ikBoneLimbRot;

        //    ComputeIKPosition(out ikBoneUpperRot, out ikBoneMidRot);
        //    ComputeIKRotation(out ikBoneLimbRot);

        //    _boneUpper.rotation = Quaternion.Lerp(animBoneUpperRot, ikBoneUpperRot, _weightIKPos);
        //    _boneMid.rotation = Quaternion.Lerp(animBoneMidRot, ikBoneMidRot, _weightIKPos);
        //    _boneLimb.rotation = Quaternion.Lerp(animBoneLimbRot, ikBoneLimbRot, _weightIKRot);
        //}

        #endregion

    }

}
