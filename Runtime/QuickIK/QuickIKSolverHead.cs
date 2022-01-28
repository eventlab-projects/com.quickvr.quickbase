using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKSolverHead : QuickIKSolver
    {

        #region UPDATE

        protected override float CheckIKTargetDistance()
        {
            return GetChainLength();
        }

        #endregion

    }

}
