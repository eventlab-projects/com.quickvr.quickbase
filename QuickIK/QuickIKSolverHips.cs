using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKSolverHips : QuickIKSolver
    {

        #region PUBLIC PARAMETERS

        public Transform _hips
        {
            get
            {
                if (!m_Hips)
                {
                    m_Hips = GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                    _hipsInitialLocalPos = m_Hips.localPosition;
                }

                return m_Hips;
            }
        }
        
        public override HumanBodyBones _boneID
        {
            get
            {
                return HumanBodyBones.Hips;
            }
            set
            {
                
            }
        }

        public override Transform _boneUpper
        {
            get
            {
                return _hips;
            }
            set
            {
                
            }
        }

        public override Transform _boneMid
        {
            get
            {
                return _hips;
            }
            set
            {
                
            }
        }

        public override Transform _boneLimb
        {
            get
            {
                return _hips;
            }
            set
            {
                
            }
        }

        public override Transform _targetLimb
        {
            get
            {
                return m_targetLimb;
            }
            set
            {
                m_targetLimb = value;
            }
        }

        public override Transform _targetHint
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }

        public override float _weight
        {
            get
            {
                return 1.0f;
            }
            set
            {
                
            }
        }

        public override float _weightIKHint
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        [SerializeField]
        protected Vector3 _hipsInitialLocalPos = Vector3.zero;

        #endregion

        #region PRIVATE ATTRIBUTES

        [SerializeField]
        private Transform m_Hips = null;

        #endregion

        #region GET AND SET

        public override void SetCurrentPoseAsBase()
        {
            base.SetCurrentPoseAsBase();
            
            _hipsInitialLocalPos = _hips.localPosition;
        }

        public override void ResetIKChain()
        {
            _hips.localPosition = _hipsInitialLocalPos;
        }

        #endregion
        
        #region UPDATE

        public override void UpdateIK()
        {
            if (!_enableIK) return;

            _hips.position = _targetLimb.position;
        }

        #endregion

    }

}
