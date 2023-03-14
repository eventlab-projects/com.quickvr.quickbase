using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKSolverHead : QuickIKSolver
    {

        protected override Vector3 GetIKTargetHintPosition(float ikAngle)
        {
            return _targetHint.position;
        }

        #region UPDATE

        protected override float CheckIKTargetDistance()
        {
            return GetChainLength();
        }

        #endregion

    }

}
