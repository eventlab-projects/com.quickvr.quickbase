using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;

namespace QuickVR
{

    public class QuickIKSolverTwoBone : TwoBoneIKConstraint, IQuickIKSolver
    {

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
    }

}


