using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{
    public class QuickVRControllerInteractor : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public XRNode _xrNode = XRNode.LeftHand;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected XRController _interactorDirect = null;
        protected XRController _interactorTeleport = null;

        #endregion

        #region CONSTANTS

        protected const string PF_INTERACTOR_DIRECT = "Prefabs/pf_InteractorDirect";
        protected const string PF_INTERACTOR_TELEPORT = "Prefabs/pf_InteractorTeleport";

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
            _interactorDirect.controllerNode = _xrNode;
            _interactorTeleport.controllerNode = _xrNode;
        }

        protected XRController CreateInteractorFromPrefab(string pfName)
        {
            XRController controller = Instantiate(Resources.Load<XRController>(pfName), transform);
            controller.enableInputTracking = false;

            return controller;
        }

        #endregion

        #region GET AND SET

        public virtual XRController GetInteractorDirectController()
        {
            return _interactorDirect;
        }

        #endregion

    }
}


