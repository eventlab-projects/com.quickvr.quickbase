using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;

namespace QuickVR
{
    public enum InteractorType
    {
        Grab,
        Teleport,
    }

    public class QuickVRController : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public XRNode _xrNode = XRNode.LeftHand;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected InputActionMap _actionMapDefault = null;

        protected QuickVRInteractionManager _interactionManager = null;
        
        protected Dictionary<InteractorType, ActionBasedController> _interactors = new Dictionary<InteractorType, ActionBasedController>();

        #endregion

        #region CONSTANTS

        protected const string ACTION_MAP_CONTROLLER_LEFT = "LeftControllerActions";
        protected const string ACTION_MAP_CONTROLLER_RIGHT = "RightControllerActions";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            _interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();

            //Load the default ActionMap for this controller
            _actionMapDefault = InputManager.GetInputActionsDefault().FindActionMap(_xrNode == XRNode.LeftHand ? ACTION_MAP_CONTROLLER_LEFT : ACTION_MAP_CONTROLLER_RIGHT);

            _interactors[InteractorType.Grab] = CreateInteractor(_interactionManager._pfInteractorDirect, "Grab");

            ActionBasedController interactorTeleport = CreateInteractor(_interactionManager._pfInteractorTeleport, "Teleport");
            interactorTeleport.GetComponent<QuickXRRayInteractor>()._interactionType = InteractorType.Teleport;
            interactorTeleport.hapticDeviceAction = new InputActionProperty(_actionMapDefault.FindAction("Haptic Device"));
            _interactors[InteractorType.Teleport] = interactorTeleport;
        }

        protected virtual ActionBasedController CreateInteractor(ActionBasedController pfInteractor, string defaultActionName)
        {
            //Create the Interactor Direct
            ActionBasedController result = Instantiate(pfInteractor, transform);
            result.enableInputTracking = false;
            
            if (!result.selectAction.action.IsValid())
            {
                //There is no user-defined binding for direct interaction. Load the default one. 
                result.selectAction = new InputActionProperty(_actionMapDefault.FindAction(defaultActionName));
            }

            return result;
        }

        #endregion

        #region GET AND SET

        public virtual ActionBasedController GetInteractor(InteractorType type)
        {
            return _interactors[type];
        }

        public virtual void EnableInteractor(InteractorType t, bool enable)
        {
            GetInteractor(t).gameObject.SetActive(enable);
        }

        #endregion

    }
}


