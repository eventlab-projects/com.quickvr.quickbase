using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageCloseApplication : QuickStageBase
    {

        public override void Init()
        {
            base.Init();

            QuickUtils.CloseApplication();
        }

    }

}


