using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

public class TestInteraction : MonoBehaviour
{

    protected virtual void Start()
    {
        QuickVRInteractionManager interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();
        //interactionManager.EnableLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.ContinuousMove, true);
        //interactionManager.EnableLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.ContinuousTurn, true);

        //QuickVRInteractor lhInteractor = interactionManager.GetVRInteractorHandLeft();
        //lhInteractor.EnableInteractorGrabRay(true);
        //lhInteractor.SetInputAction(InteractorType.Grab, QuickVRInteractor.ActionType.Select, "Use");

        QuickVRInteractor rhInteractor = interactionManager.GetVRInteractorHandRight();
        //rhInteractor.SetInteractorEnabled(InteractorType.Grab, true);
        //rhInteractor.SetInputAction(InteractorType.Grab, QuickVRInteractor.ActionType.Select, "Use");

        //To enable the Teleport, first enable the specific locomotion system. Then define if you are going to use the 
        //left or right controller for the teleport. 
        interactionManager.SetEnabledLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.Teleport, true);
        rhInteractor.SetInteractorEnabled(InteractorType.Teleport, true);

        rhInteractor.SetInteractorEnabled(InteractorType.UI, true);

        //interactionManager.GetVRInteractorHandRight().EnableInteractorUI(true);
    }

}
