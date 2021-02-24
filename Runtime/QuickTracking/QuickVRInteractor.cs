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

    public class QuickVRInteractor : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public XRNode _xrNode = XRNode.LeftHand;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected InputActionMap _actionMapDefault = null;

        protected QuickVRInteractionManager _interactionManager = null;

        protected ActionBasedController _interactorDirect = null;
        protected ActionBasedController _interactorRayGrab = null;
        protected ActionBasedController _interactorRayTeleport = null;

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

            //Create the direct interactor
            _interactorDirect = CreateInteractor(_interactionManager._pfInteractorDirect, "Grab");

            //Create the grab ray
            _interactorRayGrab = CreateInteractor(_interactionManager._pfInteractorRayGrab, "Grab");
            _interactorRayGrab.GetComponent<QuickXRRayInteractor>()._interactionType = InteractorType.Grab;
            _interactorRayGrab.hapticDeviceAction = new InputActionProperty(_actionMapDefault.FindAction("Haptic Device"));

            //Create the teleport ray
            _interactorRayTeleport = CreateInteractor(_interactionManager._pfInteractorRayTeleport, "Teleport");
            _interactorRayTeleport.GetComponent<QuickXRRayInteractor>()._interactionType = InteractorType.Teleport;
            _interactorRayTeleport.hapticDeviceAction = new InputActionProperty(_actionMapDefault.FindAction("Haptic Device"));
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

        public virtual ActionBasedController GetInteractorDirect()
        {
            return _interactorDirect;
        }

        public virtual ActionBasedController GetInteractorRayGrab()
        {
            return _interactorRayGrab;
        }

        public virtual ActionBasedController GetInteractorRayTeleport()
        {
            return _interactorRayTeleport;
        }

        public virtual void EnableInteractorDirect(bool enable)
        {
            _interactorDirect.gameObject.SetActive(enable);
        }

        public virtual void EnableInteractorRayGrab(bool enable)
        {
            _interactorRayGrab.gameObject.SetActive(enable);
        }

        public virtual void EnableInteractorRayTeleport(bool enable)
        {
            _interactorRayTeleport.gameObject.SetActive(enable);
        }

        #endregion

    }
}


