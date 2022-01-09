using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKSolverHead : QuickIKSolver
    {

        #region UPDATE

        protected override float CheckIKTargetDistance(Vector3 v)
        {
            float targetDistance = GetChainLength();
            _targetLimb.position = _boneUpper.position + v.normalized * targetDistance;

            return targetDistance;
        }

        #endregion

    }

}
