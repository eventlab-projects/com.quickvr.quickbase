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
        
        public override void GetValidTargets(List<IXRInteractable> validTargets)
        {
            base.GetValidTargets(validTargets);

            //Discard those elements that does not match the interaction type
            for (int i = validTargets.Count - 1; i >= 0; i--)
            {
                IXRInteractable t = validTargets[i];
                if (
                    //The interactable is not a component
                    t.transform == null ||

                    //The Grab ray only interacts with XRGrabInteractables and XRSimpleInteractables
                    _interactionType == InteractorType.Grab && (!t.transform.GetComponent<XRGrabInteractable>() && !t.transform.GetComponent<XRSimpleInteractable>()) ||

                    //The Teleport ray only interacts with BaseTeleportationInteractables.
                    _interactionType == InteractorType.Teleport && !t.transform.GetComponent<BaseTeleportationInteractable>() ||

                    //The UI ray only interacts with objects in the UILayer. 
                    _interactionType == InteractorType.UI && t.transform.gameObject.layer != LayerMask.NameToLayer("UI")
                    )
                {
                    validTargets.RemoveAt(i);
                }
            }
        }

        #endregion

    }

}


