﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKSolverHips : QuickIKSolver
    {

        #region PUBLIC PARAMETERS

        public override Transform _boneUpper
        {
            get
            {
                return _boneLimb;
            }
            set
            {
                
            }
        }

        public override Transform _boneMid
        {
            get
            {
                return _boneLimb;
            }
            set
            {
                
            }
        }

        public override Transform _boneLimb
        {
            get
            {
                if (!m_boneLimb)
                {
                    m_boneLimb = GetComponentInParent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                    _hipsInitialLocalPos = m_boneLimb.localPosition;
                }

                return m_boneLimb;
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

        #region GET AND SET

        public override void SaveCurrentPose()
        {
            base.SaveCurrentPose();
            
            _hipsInitialLocalPos = _boneLimb.localPosition;
        }

        public override void ResetIKChain()
        {
            _boneLimb.localPosition = _hipsInitialLocalPos;
        }

        #endregion
        
        #region UPDATE

        public override void UpdateIK()
        {
            if (!_enableIK) return;

            _boneLimb.position = _targetLimb.position;
        }

        #endregion

    }

}