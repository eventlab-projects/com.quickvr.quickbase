using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR.Samples.Workflow
{

    public class StageCheckIsCube : QuickStageCondition
    {

        public bool _spawnCubes = true;

        protected override bool Condition()
        {
            return _spawnCubes;
        }

    }

}



