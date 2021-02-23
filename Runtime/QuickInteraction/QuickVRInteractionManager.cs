using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace QuickVR
{

    public class QuickVRInteractionManager : MonoBehaviour
    {
        #region PUBLIC ATTRIBUTES

        public ActionBasedController _pfInteractorDirect = null;
        public ActionBasedController _pfInteractorTeleport = null;

        public enum ControllerNode
        {
            Head,
            LeftHand,
            RightHand,
        }

        [BitMask(typeof(ControllerNode))]
        public int _maskGrab = 0;

        [BitMask(typeof(ControllerNode))]
        public int _maskTeleport = 0;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRManager _vrManager = null;

        protected QuickXRRig _xrRig = null;
        protected QuickVRController _controllerHandLeft = null;
        protected QuickVRController _controllerHandRight = null;
        protected LocomotionSystem _locomotionSystem = null;
        protected TeleportationProvider _teleportProvider = null;
        protected ActionBasedContinuousMoveProvider _continousMoveProvider = null;
        protected ActionBasedContinuousTurnProvider _continousRotationProvider = null;

        protected CharacterController _characterController = null;

        #endregion

        #region CONSTANTS

        protected const string PF_INTERACTOR_DIRECT = "Prefabs/pf_InteractorDirect";
        protected const string PF_INTERACTOR_TELEPORT = "Prefabs/pf_InteractorTeleport";

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();
            CheckPrefabs();

            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();

            _controllerHandLeft = transform.CreateChild("__ControllerHandLeft__").GetOrCreateComponent<QuickVRController>();
            _controllerHandLeft._xrNode = XRNode.LeftHand;

            _controllerHandRight = transform.CreateChild("__ControllerHandRight__").GetOrCreateComponent<QuickVRController>();
            _controllerHandRight._xrNode = XRNode.RightHand;

            BaseTeleportationInteractable[] teleportationInteractables = FindObjectsOfType<BaseTeleportationInteractable>();
            foreach (BaseTeleportationInteractable t in teleportationInteractables)
            {
                t.teleportationProvider = _teleportProvider;
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
            if (_pfInteractorDirect == null)
            {
                _pfInteractorDirect = Resources.Load<ActionBasedController>(PF_INTERACTOR_DIRECT);
            }
            if (_pfInteractorTeleport == null)
            {
                _pfInteractorTeleport = Resources.Load<ActionBasedController>(PF_INTERACTOR_TELEPORT);
            }
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

        protected virtual void Update()
        {
            //Enable the corresponding interactors for the lefthand
            _controllerHandLeft.EnableInteractor(InteractorType.Grab, (_maskGrab & (1 << (int)ControllerNode.LeftHand)) != 0);
            _controllerHandLeft.EnableInteractor(InteractorType.Teleport, (_maskTeleport & (1 << (int)ControllerNode.LeftHand)) != 0);

            //Enable the corresponding interactors for the righthand
            _controllerHandRight.EnableInteractor(InteractorType.Grab, (_maskGrab & (1 << (int)ControllerNode.RightHand)) != 0);
            _controllerHandRight.EnableInteractor(InteractorType.Teleport, (_maskTeleport & (1 << (int)ControllerNode.RightHand)) != 0);
        }

        protected virtual void UpdateNewAnimatorTarget(Animator animator)
        {
            _xrRig.rig = animator.gameObject;   //Configure the XRRig to act in this animator
            _characterController = animator.transform.GetOrCreateComponent<CharacterController>();

            _controllerHandLeft.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _controllerHandLeft.transform.ResetTransformation();
            _controllerHandLeft.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), transform.up);

            _controllerHandRight.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _controllerHandRight.transform.ResetTransformation();
            _controllerHandRight.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), transform.up);

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


