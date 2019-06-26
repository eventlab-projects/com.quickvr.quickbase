using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public interface IQuickIKSolver
    {
        //The bone chain hierarchy
        Transform _boneUpper { get; }
        Transform _boneMid { get; }
        Transform _boneLimb { get; }

        //The IK parameters
        Transform _targetLimb { get; set; }
        Transform _targetHint { get; set; }

        float _weight { get; set; }

    }

}


