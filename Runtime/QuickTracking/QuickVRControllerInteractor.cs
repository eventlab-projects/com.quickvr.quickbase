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

        protected ActionBasedController _interactorDirect = null;
        protected ActionBasedController _interactorTeleport = null;

        #endregion

        #region CONSTANTS

        protected const string PF_INTERACTOR_DIRECT = "Prefabs/pf_InteractorDirect";
        protected const string PF_INTERACTOR_TELEPORT = "Prefabs/pf_InteractorTeleport";

        protected const string ACTION_MAP_CONTROLLER_LEFT = "LeftControllerActions";
        protected const string ACTION_MAP_CONTROLLER_RIGHT = "RightControllerActions";
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            //Create the Interactor Direct
            _interactorDirect = CreateInteractorFromPrefab(PF_INTERACTOR_DIRECT);

            //Create the Interactor Teleport and ensure that the ray only interacts with Teleport objects. 
            _interactorTeleport = CreateInteractorFromPrefab(PF_INTERACTOR_TELEPORT);
            QuickXRRayInteractor rayInteractor = _interactorTeleport.GetComponent<QuickXRRayInteractor>();
            rayInteractor._interactionType = QuickXRRayInteractor.InteractionType.Teleport;
        }

        protected virtual void Start()
        {
            //_interactorDirect.controllerNode = _xrNode;
            //_interactorTeleport.controllerNode = _xrNode;

            //Load the default actions if necessary
            InputActionAsset asset = Resources.Load<InputActionAsset>("QuickDefaultInputActions");
            InputActionMap aMap = asset.FindActionMap(_xrNode == XRNode.LeftHand ? ACTION_MAP_CONTROLLER_LEFT : ACTION_MAP_CONTROLLER_RIGHT);

            if (_interactorDirect.selectAction.action.bindings.Count == 0)
            {
                //There is no user-defined binding for direct interaction. Load the default one. 
                _interactorDirect.selectAction = new InputActionProperty(aMap.FindAction("Grab"));
            }

            if (_interactorTeleport.selectAction.action.bindings.Count == 0)
            {
                //There is no user-defined binding for teleport. Load the default one. 
                _interactorTeleport.selectAction = new InputActionProperty(aMap.FindAction("Teleport"));
            }
            _interactorTeleport.hapticDeviceAction = new InputActionProperty(aMap.FindAction("Haptic Device"));
        }

        protected ActionBasedController CreateInteractorFromPrefab(string pfName)
        {
            ActionBasedController controller = Instantiate(Resources.Load<ActionBasedController>(pfName), transform);
            controller.enableInputTracking = false;

            return controller;
        }

        #endregion

        #region GET AND SET

        public virtual ActionBasedController GetInteractorDirectController()
        {
            return _interactorDirect;
        }

        #endregion

    }
}


