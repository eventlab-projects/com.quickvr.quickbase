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

            EnableInteractorGrab(false);
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

        protected virtual void Start()
        {
            //Load the default ActionMap for this controller
            _actionMapDefault = InputManager.GetInputActionsDefault().FindActionMap(_xrNode == XRNode.LeftHand ? ACTION_MAP_CONTROLLER_LEFT : ACTION_MAP_CONTROLLER_RIGHT);

            ConfigureInteractorGrabDirect();
            ConfigureInteractorGrabRay();
            ConfigureInteractorTeleportRay();
            ConfigureInteractorUIRay();
        }

        protected virtual void ConfigureInteractorGrabDirect()
        {
            //Configure the direct interactor
            CheckInputAction(_interactorGrabDirect, ActionType.Select, "Grab");
            CheckInputAction(_interactorGrabDirect, ActionType.Activate, "Use");
        }

        protected virtual void ConfigureInteractorGrabRay()
        {
            //Configure the grab ray
            CheckInputAction(_interactorGrabRay, ActionType.Select, "Grab");
            CheckInputAction(_interactorGrabRay, ActionType.Activate, "Use");
            CheckInputAction(_interactorGrabRay, ActionType.Haptic, "Haptic Device");

            QuickXRRayInteractor ray = _interactorGrabRay.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Grab;
            ray.enableUIInteraction = false;
        }

        protected virtual void ConfigureInteractorTeleportRay()
        {
            //Configure the teleport ray
            CheckInputAction(_interactorTeleportRay, ActionType.Select, "Teleport");
            CheckInputAction(_interactorTeleportRay, ActionType.Haptic, "Haptic Device");
            
            QuickXRRayInteractor ray = _interactorTeleportRay.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.Teleport;
            ray.enableUIInteraction = false;
        }

        protected virtual void ConfigureInteractorUIRay()
        {
            //Configure the UI ray
            CheckInputAction(_interactorUIRay, ActionType.UI, "Use");

            QuickXRRayInteractor ray = _interactorUIRay.GetComponent<QuickXRRayInteractor>();
            ray._interactionType = InteractorType.UI;
            ray.enableUIInteraction = true;
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
            else if (actionType == ActionType.UI)
            {
                if (!interactor.uiPressAction.action.IsValid())
                {
                    interactor.uiPressAction = new InputActionProperty(defaultAction);
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

        public virtual bool IsEnabledInteractorGrab()
        {
            return GetInteractorGrab().gameObject.activeSelf;
        }

        public virtual void EnableInteractorGrab(bool enable)
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


