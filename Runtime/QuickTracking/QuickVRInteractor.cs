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
        UI,
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

        protected QuickVRInteractionManager _interactionManager = null;

        protected ActionBasedController _interactorGrabDirect = null;
        protected ActionBasedController _interactorGrabRay = null;
        protected ActionBasedController _interactorTeleportRay = null;
        protected ActionBasedController _interactorUIRay = null;

        #endregion

        #region CONSTANTS

        protected const string ACTION_MAP_CONTROLLER_LEFT = "LeftController";
        protected const string ACTION_MAP_CONTROLLER_RIGHT = "RightController";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();

            _interactorGrabDirect = CreateInteractor(_interactionManager._pfInteractorGrabDirect);
            _interactorGrabRay = CreateInteractor(_interactionManager._pfInteractorGrabRay);
            _interactorTeleportRay = CreateInteractor(_interactionManager._pfInteractorTeleportRay);
            _interactorUIRay = CreateInteractor(_interactionManager._pfInteractorUIRay);

            EnableInteractorGrabRay(false);
            EnableInteractorGrabDirect(false);
            EnableInteractorTeleport(false);
            EnableInteractorUI(false);
        }

        protected virtual ActionBasedController CreateInteractor(ActionBasedController pfInteractor)
        {
            //Create the Interactor Direct
            ActionBasedController result = Instantiate(pfInteractor, transform);
            result.enableInputTracking = false;

            return result;
        }

        protected virtual void ConfigureInteractorGrabDirect()
        {
            //Configure the direct interactor
            SetInputAction(_interactorGrabDirect, ActionType.Select, "Grab");
            SetInputAction(_interactorGrabDirect, ActionType.Activate, "Use");
        }

        protected virtual void ConfigureInteractorGrabRay()
        {
            //Configure the grab ray
            SetInputAction(_interactorGrabRay, ActionType.Select, "Grab");
            SetInputAction(_interactorGrabRay, ActionType.Activate, "Use");
            SetInputAction(_interactorGrabRay, ActionType.Haptic, "Haptic Device");

            QuickXRRayInteractor ray = _interactorGrabRay.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Grab;
            ray.enableUIInteraction = false;
        }

        protected virtual void ConfigureInteractorTeleportRay()
        {
            //Configure the teleport ray
            SetInputAction(_interactorTeleportRay, ActionType.Select, "Teleport");
            SetInputAction(_interactorTeleportRay, ActionType.Haptic, "Haptic Device");
            
            QuickXRRayInteractor ray = _interactorTeleportRay.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Teleport;
            ray.enableUIInteraction = false;
        }

        protected virtual void ConfigureInteractorUIRay()
        {
            //Configure the UI ray
            SetInputAction(_interactorUIRay, ActionType.UI, "Use");

            QuickXRRayInteractor ray = _interactorUIRay.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.UI;
            ray.enableUIInteraction = true;
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

        public virtual void SetInputAction(InteractorType interactorType, ActionType actionType, string actionName)
        {
            if (interactorType == InteractorType.Grab)
            {
                SetInputAction(_interactorGrabDirect, actionType, actionName);
                SetInputAction(_interactorGrabRay, actionType, actionName);
            }
            else if (interactorType == InteractorType.Teleport)
            {
                SetInputAction(_interactorTeleportRay, actionType, actionName);
            }
            else if (interactorType == InteractorType.UI)
            {
                SetInputAction(_interactorUIRay, actionType, actionName);
            }
        }

        #endregion

        #region GET AND SET

        public virtual ActionBasedController GetInteractorTeleport()
        {
            return _interactorTeleportRay;
        }

        public virtual bool IsEnabledInteractorGrabDirect()
        {
            return _interactorGrabDirect.gameObject.activeSelf;
        }

        public virtual bool IsEnabledInteractorGrabRay()
        {
            return _interactorGrabRay.gameObject.activeSelf;
        }

        public virtual void EnableInteractorGrabDirect(bool enable)
        {
            _interactorGrabDirect.gameObject.SetActive(enable);
        }

        public virtual void EnableInteractorGrabRay(bool enable)
        {
            _interactorGrabRay.gameObject.SetActive(enable);
        }

        public virtual bool IsEnabledInteractorTeleport()
        {
            return _interactorTeleportRay.gameObject.activeSelf;
        }

        public virtual void EnableInteractorTeleport(bool enable)
        {
            _interactorTeleportRay.gameObject.SetActive(enable);
        }

        public virtual bool IsEnabledInteractorUI()
        {
            return _interactorUIRay.gameObject.activeSelf;
        }

        public virtual void EnableInteractorUI(bool enable)
        {
            _interactorUIRay.gameObject.SetActive(enable);
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
            Transform tAttach = _interactorGrabDirect.GetComponent<XRDirectInteractor>().attachTransform;
            tAttach.position = Vector3.Lerp(tHand.position, tMiddle.position, 0.5f);

            Transform tIndex = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal);
            Transform tLittle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal);
            CapsuleCollider collider = _interactorGrabDirect.GetComponent<CapsuleCollider>();
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


