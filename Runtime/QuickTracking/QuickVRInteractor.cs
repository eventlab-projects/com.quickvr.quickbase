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

        public enum GrabMode
        {
            Direct,
            Ray,
        }
        public GrabMode _grabMode = GrabMode.Direct;

        protected enum ActionType
        {
            Select,
            Activate,
            Haptic,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected InputActionMap _actionMapDefault = null;

        protected QuickVRInteractionManager _interactionManager = null;

        protected ActionBasedController _interactorGrabDirect = null;
        protected ActionBasedController _interactorGrabRay = null;
        protected ActionBasedController _interactorTeleportRay = null;

        #endregion

        #region CONSTANTS

        protected const string ACTION_MAP_CONTROLLER_LEFT = "LeftControllerActions";
        protected const string ACTION_MAP_CONTROLLER_RIGHT = "RightControllerActions";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();

            _interactorGrabDirect = CreateInteractor(_interactionManager._pfInteractorGrabDirect);
            _interactorGrabRay = CreateInteractor(_interactionManager._pfInteractorGrabRay);
            _interactorTeleportRay = CreateInteractor(_interactionManager._pfInteractorTeleportRay);

            //EnableGrab(false);
            //EnableTeleport(false);
        }

        protected virtual void Start()
        {
            //Load the default ActionMap for this controller
            _actionMapDefault = InputManager.GetInputActionsDefault().FindActionMap(_xrNode == XRNode.LeftHand ? ACTION_MAP_CONTROLLER_LEFT : ACTION_MAP_CONTROLLER_RIGHT);

            //Configure the direct interactor
            CheckInputAction(_interactorGrabDirect, ActionType.Select, "Grab");
            CheckInputAction(_interactorGrabDirect, ActionType.Activate, "Use");

            //Configure the grab ray
            CheckInputAction(_interactorGrabRay, ActionType.Select, "Grab");
            CheckInputAction(_interactorGrabRay, ActionType.Activate, "Use");
            CheckInputAction(_interactorGrabRay, ActionType.Haptic, "Haptic Device");
            _interactorGrabRay.GetComponent<QuickXRRayInteractor>()._interactionType = InteractorType.Grab;

            //Configure the teleport ray
            CheckInputAction(_interactorTeleportRay, ActionType.Select, "Teleport");
            CheckInputAction(_interactorTeleportRay, ActionType.Haptic, "Haptic Device");
        }

        protected virtual ActionBasedController CreateInteractor(ActionBasedController pfInteractor)
        {
            //Create the Interactor Direct
            ActionBasedController result = Instantiate(pfInteractor, transform);
            result.enableInputTracking = false;
            
            return result;
        }

        protected virtual void CheckInputAction(ActionBasedController interactor, ActionType actionType, string defaultActionName)
        {
            InputAction defaultAction = _actionMapDefault.FindAction(defaultActionName);
            if (actionType == ActionType.Activate)
            {
                if (!interactor.activateAction.action.IsValid())
                {
                    interactor.activateAction = new InputActionProperty(defaultAction);
                }
            }
            else if (actionType == ActionType.Select)
            {
                if (!interactor.selectAction.action.IsValid())
                {
                    interactor.selectAction = new InputActionProperty(defaultAction);
                }
            }
            else if (actionType == ActionType.Haptic)
            {
                if (!interactor.hapticDeviceAction.action.IsValid())
                {
                    interactor.hapticDeviceAction = new InputActionProperty(defaultAction);
                }
            }
        }

        #endregion

        #region GET AND SET

        public virtual ActionBasedController GetInteractorGrab()
        {
            return _grabMode == GrabMode.Direct ? _interactorGrabDirect : _interactorGrabRay;
        }

        public virtual ActionBasedController GetInteractorTeleport()
        {
            return _interactorTeleportRay;
        }

        public virtual void EnableGrab(bool enable)
        {
            if (enable)
            {
                if (_grabMode == GrabMode.Direct)
                {
                    _interactorGrabDirect.gameObject.SetActive(true);
                    _interactorGrabRay.gameObject.SetActive(false);
                }
                else
                {
                    _interactorGrabDirect.gameObject.SetActive(false);
                    _interactorGrabRay.gameObject.SetActive(true);
                }
            }
            else
            {
                _interactorGrabDirect.gameObject.SetActive(false);
                _interactorGrabRay.gameObject.SetActive(false);
            }
        }

        public virtual void EnableTeleport(bool enable)
        {
            _interactorTeleportRay.gameObject.SetActive(enable);
        }

        #endregion

    }
}


