using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{
    public class QuickXRRayInteractor : XRRayInteractor
    {

        #region PUBLIC ATTRIUBTES

        //Enum that defines what the ray is used for
        public InteractorType _interactionType = InteractorType.Teleport;

        #endregion

        #region GET AND SET
        
        public override void GetValidTargets(List<XRBaseInteractable> validTargets)
        {
            base.GetValidTargets(validTargets);

            //Discard those elements that does not match the interaction type
            for (int i = validTargets.Count - 1; i >= 0; i--)
            {
                XRBaseInteractable t = validTargets[i];
                if (
                    _interactionType == InteractorType.Grab && !t.GetComponent<XRGrabInteractable>() ||
                    _interactionType == InteractorType.Teleport && !t.GetComponent<BaseTeleportationInteractable>()
                    )
                {
                    validTargets.RemoveAt(i);
                }
            }
        }

        #endregion

    }

}


