using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

public class StageCheckIsCube : QuickStageCondition
{

    public bool _spawnCubes = true;

    protected override bool Condition()
    {
        return _spawnCubes;
    }

}


