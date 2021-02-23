using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{

    public class QuickContinuousMoveProvider : ActionBasedContinuousMoveProvider
    {

        public virtual void UpdateLocomotion()
        {
            Update();
        }

    }

}


