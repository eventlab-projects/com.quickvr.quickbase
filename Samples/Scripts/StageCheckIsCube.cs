using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

public class StageCheckIsCube : QuickStageCondition
{
    
    protected override bool Condition()
    {
        return SettingsTestVR.GetIsCube();    
    }

}



