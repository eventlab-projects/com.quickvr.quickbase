using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public interface IQuickIKSolver
    {
        HumanBodyBones _boneID { get; set; }

        //The bone chain hierarchy
        Transform _boneUpper { get; set; }
        Transform _boneMid { get; set; }
        Transform _boneLimb { get; set; }

        //The IK parameters
        Transform _targetLimb { get; set; }
        Transform _targetHint { get; set; }

        float _weight { get; set; }

        float _weightIKPos { get; set; }
        float _weightIKRot { get; set; }
        float _weightIKHint { get; set; }

    }

}


