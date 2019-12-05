using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Animations.Rigging;

namespace QuickVR
{

    public class QuickIKSolverTwoBone : TwoBoneIKConstraint, IQuickIKSolver
    {

        #region PUBLIC PARAMETERS

        public HumanBodyBones _boneID
        {
            get
            {
                return m_boneID;
            }
            set
            {
                m_boneID = value;
            }
        }

        public Transform _boneUpper
        {
            get
            {
                return data.root;
            }
            set
            {
                data.root = value;
            }
        }

        public Transform _boneMid
        {
            get
            {
                return data.mid;
            }
            set
            {
                data.mid = value;
            }
        }

        public Transform _boneLimb
        {
            get
            {
                return data.tip;
            }
            set
            {
                data.tip = value;
            }
        }

        public Transform _targetLimb
        {
            get
            {
                return data.target;
            }
            set
            {
                data.target = value;
            }
        }

        public Transform _targetHint
        {
            get
            {
                return data.hint;
            }
            set
            {
                data.hint = value;
            }
        }

        public float _weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }

        public float _weightIKPos
        {
            get
            {
                return data.targetPositionWeight;
            }
            set
            {
                data.targetPositionWeight = value;
            }
        }

        public float _weightIKRot
        {
            get
            {
                return data.targetRotationWeight;
            }
            set
            {
                data.targetRotationWeight = value;
            }
        }

        public float _weightIKHint
        {
            get
            {
                return data.hintWeight;
            }
            set
            {
                data.hintWeight = value;
            }
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected HumanBodyBones m_boneID = HumanBodyBones.LastBone;

        #endregion

    }

}


