using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageTeleportPlayer : QuickStageTeleport
    {
        public override void Init()
        {
            _sourceTransform = _vrManager.GetAnimatorTarget().transform;

            base.Init();
        }

    }

}
