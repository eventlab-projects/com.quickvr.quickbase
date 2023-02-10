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
        GrabDirect,
        Grab,
        Teleport,
        UI,
        Generic, 
    }

    public class QuickVRInteractor : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public XRNode _xrNode
        {
            get
            {
                return m_XRNode;
            }
            set
            {
                m_XRNode = value;

                //Load the default ActionMap for this controller
                _actionMapDefault = InputManager.GetInputActionsDefault().FindActionMap(_xrNode == XRNode.LeftHand ? ACTION_MAP_CONTROLLER_LEFT : ACTION_MAP_CONTROLLER_RIGHT);

                ConfigureInteractorGrabDirect();
                ConfigureInteractorGrabRay();
                ConfigureInteractorTeleportRay();
                ConfigureInteractorUIRay();
                //ConfigureInteractorUIGeneric();
            }
        } 
            
        protected XRNode m_XRNode = XRNode.LeftHand;

        public enum ActionType
        {
            Select,
            Activate,
            Haptic,
            UI,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected InputActionMap _actionMapDefault = null;

        protected Dictionary<InteractorType, XRBaseControllerInteractor> _interactors = new Dictionary<InteractorType, XRBaseControllerInteractor>();

        #endregion

        #region CONSTANTS

        protected const string ACTION_MAP_CONTROLLER_LEFT = "LeftController";
        protected const string ACTION_MAP_CONTROLLER_RIGHT = "RightController";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            CreateInteractors();

            //By default, disable all the interactors
            foreach (var pair in _interactors)
            {
                SetInteractorEnabled(pair.Key, false);
            }
        }

        protected virtual void CreateInteractors()
        {
            QuickVRInteractionManager interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();

            _interactors[InteractorType.GrabDirect] = CreateInteractor(interactionManager._pfInteractorGrabDirect);
            _interactors[InteractorType.Grab] = CreateInteractor(interactionManager._pfInteractorGrabRay);
            _interactors[InteractorType.Teleport] = CreateInteractor(interactionManager._pfInteractorTeleportRay);
            _interactors[InteractorType.UI] = CreateInteractor(interactionManager._pfInteractorUIRay);
        }

        protected virtual XRBaseControllerInteractor CreateInteractor(XRBaseControllerInteractor pfInteractor)
        {
            //Create the Interactor Direct
            XRBaseControllerInteractor result = Instantiate(pfInteractor, transform);
            ActionBasedController aController = result.GetComponent<ActionBasedController>();
            if (aController)
            {
                aController.enableInputTracking = false;
            }

            return result;
        }

        /// <summary>
        /// Configures a Direct Interactor to interact with objects containing the XRGrabInteractable component.  
        /// </summary>
        protected virtual void ConfigureInteractorGrabDirect()
        {
            //Configure the direct interactor
            ActionBasedController interactor = GetInteractor(InteractorType.GrabDirect).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.Select, "Grab");
            SetInputAction(interactor, ActionType.Activate, "Use");
        }

        /// <summary>
        /// Configures a Ray Interactor to interact with objects containing the XRGrabInteractable component.  
        /// </summary>
        protected virtual void ConfigureInteractorGrabRay()
        {
            //Configure the grab ray
            ActionBasedController interactor = GetInteractor(InteractorType.Grab).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.Select, "Grab");
            SetInputAction(interactor, ActionType.Activate, "Use");
            SetInputAction(interactor, ActionType.Haptic, "Haptic Device");

            QuickXRRayInteractor ray = interactor.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Grab;
            ray.enableUIInteraction = false;
        }

        /// <summary>
        /// Configures a Ray Interactor to interact with objects containing the BaseTeleportationInteractable component.  
        /// </summary>
        protected virtual void ConfigureInteractorTeleportRay()
        {
            //Configure the teleport ray
            ActionBasedController interactor = GetInteractor(InteractorType.Teleport).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.Select, "Teleport");
            SetInputAction(interactor, ActionType.Haptic, "Haptic Device");
            
            QuickXRRayInteractor ray = interactor.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Teleport;
            ray.enableUIInteraction = false;
        }

        /// <summary>
        /// Configures a Ra Interactor to interact with the UI elements. 
        /// </summary>
        protected virtual void ConfigureInteractorUIRay()
        {
            //Configure the UI ray
            ActionBasedController interactor = GetInteractor(InteractorType.UI).GetComponent<ActionBasedController>();
            SetInputAction(interactor, ActionType.UI, "Use");

            QuickXRRayInteractor ray = interactor.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.UI;
            ray.enableUIInteraction = true;
        }

        protected virtual void ConfigureInteractorGenericRay()
        {

        }

        protected virtual void SetInputAction(ActionBasedController interactor, ActionType actionType, string actionName)
        {
            InputAction action = _actionMapDefault.FindAction(actionName);
            if (actionType == ActionType.Activate)
            {
                interactor.activateAction = new InputActionProperty(action);
            }
            else if (actionType == ActionType.Select)
            {
                interactor.selectAction = new InputActionProperty(action);
            }
            else if (actionType == ActionType.Haptic)
            {
                interactor.hapticDeviceAction = new InputActionProperty(action);
            }
            else if (actionType == ActionType.UI)
            {
                interactor.uiPressAction = new InputActionProperty(action);
            }
        }

        #endregion

        #region GET AND SET

        public virtual XRBaseControllerInteractor GetInteractor(InteractorType type)
        {
            _interactors.TryGetValue(type, out XRBaseControllerInteractor result);

            return result;
        }

        public virtual void SetInteractorEnabled(InteractorType type, bool enabled)
        {
            //ActionBasedController interactor = GetInteractor(type);
            //if (interactor && interactor.gameObject.activeSelf != enabled)
            //{
            //    interactor.gameObject.SetActive(enabled);
            //}

            XRBaseControllerInteractor interactor = GetInteractor(type);
            if (interactor && interactor.gameObject.activeSelf != enabled)
            {
                interactor.gameObject.SetActive(enabled);

                //Disable all the interactors
                HashSet<XRBaseControllerInteractor> enabledInteractors = new HashSet<XRBaseControllerInteractor>();
                foreach (var pair in _interactors)
                {
                    if (pair.Value.gameObject.activeSelf)
                    {
                        enabledInteractors.Add(pair.Value);
                    }
                    pair.Value.gameObject.SetActive(false);
                }

                //Reenable the interactors that were enabled at the begining. 
                foreach (XRBaseControllerInteractor tmp in enabledInteractors)
                {
                    tmp.gameObject.SetActive(true);
                }
            }
        }

        public virtual bool IsEnabledInteractor(InteractorType type)
        {
            XRBaseControllerInteractor interactor = GetInteractor(type);
            return interactor ? interactor.gameObject.activeSelf : false;
        }

        public virtual void UpdateNewAnimatorTarget(Animator animator)
        {
            bool isLeft = _xrNode == XRNode.LeftHand;
            Transform tHand = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            Transform tMiddle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);

            //Set this interactor to be child of the corresponding hand. 
            transform.parent = tHand;
            transform.ResetTransformation();
            transform.LookAt(tMiddle, transform.up);

            //Configure the DirectInteractor
            XRBaseControllerInteractor interactor = GetInteractor(InteractorType.GrabDirect);
            Transform tAttach = interactor.GetComponent<XRDirectInteractor>().attachTransform;
            tAttach.position = Vector3.Lerp(tHand.position, tMiddle.position, 0.5f);

            Transform tIndex = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal);
            Transform tLittle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal);
            CapsuleCollider collider = interactor.GetComponent<CapsuleCollider>();
            collider.height = Vector3.Distance(tIndex.position, tLittle.position);
            collider.center = tAttach.localPosition;
            collider.radius = Vector3.Distance(tHand.position, tMiddle.position) * 0.5f;

            //Define the radius
            //SphereCollider sCollider = _interactorGrabDirect.GetComponent<SphereCollider>();
            //sCollider.center = tAttach.localPosition;
            //sCollider.radius = Vector3.Distance(tHand.position, tMiddle.position) * 0.5f;
        }

        #endregion

    }
}


