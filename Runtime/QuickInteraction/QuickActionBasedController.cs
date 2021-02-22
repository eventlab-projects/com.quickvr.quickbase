using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{
    public class QuickActionBasedController : ActionBasedController
    {

        protected override void UpdateInput(XRControllerState controllerState)
        {
            if (controllerState == null)
                return;

            base.UpdateInput(controllerState);

            controllerState.selectInteractionState.activatedThisFrame |= InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE);
            controllerState.selectInteractionState.active |= InputManager.GetButton(InputManager.DEFAULT_BUTTON_CONTINUE);
            controllerState.selectInteractionState.deactivatedThisFrame |= InputManager.GetButtonUp(InputManager.DEFAULT_BUTTON_CONTINUE);
        }

    }

}


