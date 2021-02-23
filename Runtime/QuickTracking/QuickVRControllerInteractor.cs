using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;

namespace QuickVR
{
    public class QuickVRControllerInteractor : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public XRNode _xrNode = XRNode.LeftHand;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected InputActionMap _actionMapDefault = null;

        protected QuickVRInteractionManager _interactionManager = null;
        
        protected ActionBasedController _interactorDirect = null;
        protected ActionBasedController _interactorTeleport = null;

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

            CreateInteractorDirect();
            CreateInteractorTeleport();
        }

        protected virtual void CreateInteractorDirect()
        {
            //Create the Interactor Direct
            _interactorDirect = Instantiate(_interactionManager._pfInteractorDirect, transform);
            _interactorDirect.enableInputTracking = false;
            if (_interactorDirect.selectAction.action.bindings.Count == 0)
            {
                //There is no user-defined binding for direct interaction. Load the default one. 
                _interactorDirect.selectAction = new InputActionProperty(_actionMapDefault.FindAction("Grab"));
            }
        }

        protected virtual void CreateInteractorTeleport()
        {
            //Create the Interactor Teleport and ensure that the ray only interacts with Teleport objects. 
            _interactorTeleport = Instantiate(_interactionManager._pfInteractorTeleport, transform);
            _interactorTeleport.enableInputTracking = false;
            QuickXRRayInteractor rayInteractor = _interactorTeleport.GetComponent<QuickXRRayInteractor>();
            rayInteractor._interactionType = QuickXRRayInteractor.InteractionType.Teleport;
            if (!_interactorTeleport.selectAction.action.IsValid())
            {
                //There is no user-defined binding for teleport. Load the default one. 
                _interactorTeleport.selectAction = new InputActionProperty(_actionMapDefault.FindAction("Teleport"));
            }
            _interactorTeleport.hapticDeviceAction = new InputActionProperty(_actionMapDefault.FindAction("Haptic Device"));
        }

        #endregion

        #region GET AND SET

        public virtual ActionBasedController GetInteractor()
        {
            return _interactorDirect;
        }

        #endregion

    }
}


