using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace QuickVR
{

    public class QuickVRInteractionManager : MonoBehaviour
    {
        #region PUBLIC ATTRIBUTES

        public ActionBasedController _pfInteractorGrabDirect = null;
        public ActionBasedController _pfInteractorGrabRay = null;
        public ActionBasedController _pfInteractorTeleportRay = null;
        public ActionBasedController _pfInteractorUIRay = null;

        public enum ControllerNode
        {
            Head,
            LeftHand,
            RightHand,
        }

        public QuickVRInteractor.GrabMode _grabModeHandLeft = QuickVRInteractor.GrabMode.Direct;
        public QuickVRInteractor.GrabMode _grabModeHandRight = QuickVRInteractor.GrabMode.Direct;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRManager _vrManager = null;
        protected XRInteractionManager _xrInteractionManager = null;

        protected QuickXRRig _xrRig = null;
        
        protected QuickVRInteractor _interactorHandLeft = null;
        protected QuickVRInteractor _interactorHandRight = null;

        protected LocomotionSystem _locomotionSystem = null;
        protected TeleportationProvider _teleportProvider = null;
        protected ActionBasedContinuousMoveProvider _continousMoveProvider = null;
        protected ActionBasedContinuousTurnProvider _continousRotationProvider = null;

        protected CharacterController _characterController = null;

        #endregion

        #region CONSTANTS

        protected const string GRAB_PIVOT_NAME = "GrabPivot";

        protected const string PF_INTERACTOR_GRAB_DIRECT = "Prefabs/pf_InteractorGrabDirect";
        protected const string PF_INTERACTOR_GRAB_RAY = "Prefabs/pf_InteractorGrabRay";
        protected const string PF_INTERACTOR_TELEPORT_RAY = "Prefabs/pf_InteractorTeleportRay";
        protected const string PF_INTERACTOR_UI_RAY = "Prefabs/pf_InteractorUIRay";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();
            CheckPrefabs();
            CheckEventSystem();

            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
            _xrInteractionManager = QuickSingletonManager.GetInstance<XRInteractionManager>();

            _interactorHandLeft = transform.CreateChild("__InteractorHandLeft__").GetOrCreateComponent<QuickVRInteractor>();
            _interactorHandLeft._xrNode = XRNode.LeftHand;

            _interactorHandRight = transform.CreateChild("__InteractorHandRight__").GetOrCreateComponent<QuickVRInteractor>();
            _interactorHandRight._xrNode = XRNode.RightHand;

            BaseTeleportationInteractable[] teleportationInteractables = FindObjectsOfType<BaseTeleportationInteractable>();
            foreach (BaseTeleportationInteractable t in teleportationInteractables)
            {
                t.teleportationProvider = _teleportProvider;
            }

            XRGrabInteractable[] grabInteractables = FindObjectsOfType<XRGrabInteractable>();
            foreach (XRGrabInteractable g in grabInteractables)
            {
                if (!g.attachTransform)
                {
                    //Try to find the default attach transform
                    g.attachTransform = g.transform.Find(GRAB_PIVOT_NAME);
                }
                g.selectEntered.AddListener(OnGrabInteractable);
                g.selectExited.AddListener(OnDropInteractable);
            }
        }

        protected virtual void Reset()
        {
            _xrRig = transform.CreateChild("__XRRig__").GetOrCreateComponent<QuickXRRig>();
            _locomotionSystem = _xrRig.GetOrCreateComponent<LocomotionSystem>();
            
            _teleportProvider = gameObject.GetOrCreateComponent<TeleportationProvider>();
            _teleportProvider.system = _locomotionSystem;

            _continousMoveProvider = gameObject.GetOrCreateComponent<ActionBasedContinuousMoveProvider>();
            _continousMoveProvider.system = _locomotionSystem;
            if (!_continousMoveProvider.leftHandMoveAction.action.IsValid())
            {
                _continousMoveProvider.leftHandMoveAction = new InputActionProperty(InputManager.GetInputActionsDefault().FindAction("GeneralActions/Move"));
            }

            _continousRotationProvider = gameObject.GetOrCreateComponent<ActionBasedContinuousTurnProvider>();
            _continousRotationProvider.system = _locomotionSystem;
            if (!_continousRotationProvider.rightHandTurnAction.action.IsValid())
            {
                _continousRotationProvider.rightHandTurnAction = new InputActionProperty(InputManager.GetInputActionsDefault().FindAction("GeneralActions/RotateCamera"));
            }
        }

        protected virtual void CheckPrefabs()
        {
            if (_pfInteractorGrabDirect == null)
            {
                _pfInteractorGrabDirect = Resources.Load<ActionBasedController>(PF_INTERACTOR_GRAB_DIRECT);
            }
            if (_pfInteractorGrabRay == null)
            {
                _pfInteractorGrabRay = Resources.Load<ActionBasedController>(PF_INTERACTOR_GRAB_RAY);
            }
            if (_pfInteractorTeleportRay == null)
            {
                _pfInteractorTeleportRay = Resources.Load<ActionBasedController>(PF_INTERACTOR_TELEPORT_RAY);
            }
            if (_pfInteractorUIRay == null)
            {
                _pfInteractorUIRay = Resources.Load<ActionBasedController>(PF_INTERACTOR_UI_RAY);
            }
        }

        protected virtual void CheckEventSystem()
        {
            //Look if there is an EventSystem already created, and if this is the case, destroy it and 
            //create our own one to be able to interact with the UI in VR. 
            EventSystem eSystem = FindObjectOfType<EventSystem>();
            if (eSystem)
            {
                Destroy(eSystem.gameObject);
            }

            GameObject go = new GameObject("__EventSystem__");
            go.AddComponent<EventSystem>();
            go.AddComponent<XRUIInputModule>();
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCameraUpdate += UpdateCharacterController;
            QuickVRManager.OnTargetAnimatorSet += UpdateNewAnimatorTarget;
            
            //_continousMoveProvider.beginLocomotion += OnEndMove;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCameraUpdate += UpdateCharacterController;
            QuickVRManager.OnTargetAnimatorSet -= UpdateNewAnimatorTarget;

            //_continousMoveProvider.beginLocomotion -= OnEndMove;
        }

        #endregion

        #region UPDATE
        
        protected virtual void OnGrabInteractable(SelectEnterEventArgs args)
        {
            foreach (Collider c in args.interactable.colliders)
            {
                Physics.IgnoreCollision(_characterController, c, true);
            }
        }

        protected virtual void OnDropInteractable(SelectExitEventArgs args)
        {
            foreach (Collider c in args.interactable.colliders)
            {
                Physics.IgnoreCollision(_characterController, c, false);
            }
        }

        protected virtual void Update()
        {
            //Enable the corresponding interactors for the lefthand
            _interactorHandLeft._grabMode = _grabModeHandLeft;
            
            //Enable the corresponding interactors for the righthand
            _interactorHandRight._grabMode = _grabModeHandRight;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _interactorHandLeft.EnableInteractorGrab(!_interactorHandLeft.IsEnabledInteractorGrab());
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _interactorHandLeft.EnableInteractorTeleport(!_interactorHandLeft.IsEnabledInteractorTeleport());
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _interactorHandLeft.EnableInteractorUI(!_interactorHandLeft.IsEnabledInteractorUI());
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _interactorHandRight.EnableInteractorGrab(!_interactorHandRight.IsEnabledInteractorGrab());
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _interactorHandRight.EnableInteractorTeleport(!_interactorHandRight.IsEnabledInteractorTeleport());
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _interactorHandRight.EnableInteractorUI(!_interactorHandRight.IsEnabledInteractorUI());
            }
        }

        protected virtual void UpdateNewAnimatorTarget(Animator animator)
        {
            _xrRig.rig = animator.gameObject;   //Configure the XRRig to act in this animator
            _characterController = animator.transform.GetOrCreateComponent<CharacterController>();
            Debug.Log(_characterController.GetComponent<Collider>().GetType());

            _interactorHandLeft.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _interactorHandLeft.transform.ResetTransformation();
            _interactorHandLeft.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), transform.up);

            _interactorHandRight.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _interactorHandRight.transform.ResetTransformation();
            _interactorHandRight.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), transform.up);

            _continousMoveProvider.forwardSource = animator.transform;
        }
        
        protected virtual void UpdateCharacterController()
        {
            if (_characterController)
            {
                Animator animator = _vrManager.GetAnimatorTarget();

                //Compute the height of the collider
                float h = animator.GetEyeCenterPosition().y - animator.transform.position.y;
                _characterController.height = h;
                _characterController.center = new Vector3(0, h * 0.5f + _characterController.skinWidth, 0);

                //Compute the radius
                Vector3 v = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position - animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
                v = Vector3.ProjectOnPlane(v, animator.transform.up);
                _characterController.radius = v.magnitude / 2;
            }
        }

        protected virtual void OnEndMove(LocomotionSystem lSystem)
        {
            Animator animator = _vrManager.GetAnimatorTarget();
            Camera cam = Camera.main;
            if (animator && cam)
            {
                QuickVRPlayArea playArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
                Transform tmp = playArea.transform.parent;
                playArea.transform.parent = null;

                Vector3 fwd = animator.transform.forward;
                Vector3 rotAxis = animator.transform.up;
                Vector3 targetFwd = Vector3.ProjectOnPlane(cam.transform.forward, rotAxis);

                animator.GetBoneTransform(HumanBodyBones.Head).Rotate(rotAxis, Vector3.SignedAngle(targetFwd, fwd, rotAxis), Space.World);
                animator.transform.forward = targetFwd;

                playArea.transform.parent = tmp;
            }
        }

        #endregion

    }

}


