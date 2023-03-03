using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using Unity.XR.CoreUtils;

namespace QuickVR
{

    public class QuickVRInteractionManager : MonoBehaviour
    {
        #region PUBLIC ATTRIBUTES

        public XRBaseControllerInteractor _pfInteractorGrabDirect = null;
        public XRBaseControllerInteractor _pfInteractorGrabRay = null;
        public XRBaseControllerInteractor _pfInteractorTeleportRay = null;
        public XRBaseControllerInteractor _pfInteractorUIRay = null;

        public enum DefaultLocomotionProvider
        {
            Teleport, 
            DirectMove,
            DirectTurn,
            ContinuousMove, 
            ContinuousTurn,
        }

        public QuickVRInteractor _interactorHead { get; protected set; }
        public QuickVRInteractorHand _interactorHandLeft { get; protected set; }
        public QuickVRInteractorHand _interactorHandRight { get; protected set; }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRManager _vrManager = null;
        protected XRInteractionManager _xrInteractionManager = null;

        protected XROrigin _xrRig = null;

        protected CharacterController _characterController = null;
        protected QuickVRHandAnimator _handAnimatorLeft = null;
        protected QuickVRHandAnimator _handAnimatorRight = null;
        protected List<XRGrabInteractable> _grabInteractables = new List<XRGrabInteractable>();

        protected Dictionary<DefaultLocomotionProvider, LocomotionProvider> _locomotionProviders = new Dictionary<DefaultLocomotionProvider, LocomotionProvider>();

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

            _interactorHead = transform.CreateChild("__InteractorHead__").GetOrCreateComponent<QuickVRInteractor>();
            _interactorHead._xrNode = XRNode.Head;

            _interactorHandLeft = transform.CreateChild("__InteractorHandLeft__").GetOrCreateComponent<QuickVRInteractorHand>();
            _interactorHandLeft._xrNode = XRNode.LeftHand;

            _interactorHandRight = transform.CreateChild("__InteractorHandRight__").GetOrCreateComponent<QuickVRInteractorHand>();
            _interactorHandRight._xrNode = XRNode.RightHand;

            //By default, disable all the locomotion providers
            foreach (DefaultLocomotionProvider lProvider in QuickUtils.GetEnumValues<DefaultLocomotionProvider>())
            {
                SetEnabledLocomotionSystem(lProvider, false);
            }

            BaseTeleportationInteractable[] teleportationInteractables = FindObjectsOfType<BaseTeleportationInteractable>();
            foreach (BaseTeleportationInteractable t in teleportationInteractables)
            {
                t.teleportationProvider = (TeleportationProvider)_locomotionProviders[DefaultLocomotionProvider.Teleport];
            }

            _grabInteractables = new List<XRGrabInteractable>(FindObjectsOfType<XRGrabInteractable>());
            foreach (XRGrabInteractable g in _grabInteractables)
            {
                if (!g.attachTransform)
                {
                    //Try to find the default attach transform
                    g.attachTransform = g.transform.Find(GRAB_PIVOT_NAME);
                }
            }
        }

        protected virtual void Start()
        {
            QuickVRPlayArea playArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _handAnimatorLeft = ((QuickVRNodeHand)playArea.GetVRNode(HumanBodyBones.LeftHand)).GetHandAnimator();
            _handAnimatorRight = ((QuickVRNodeHand)playArea.GetVRNode(HumanBodyBones.RightHand)).GetHandAnimator();

            _xrRig.Camera = Camera.main;
            _interactorHead.transform.parent = Camera.main.transform;
            _interactorHead.transform.ResetTransformation();
        }

        protected virtual void Reset()
        {
            _xrRig = transform.CreateChild("__XRRig__").GetOrCreateComponent<XROrigin>();
            _xrRig.GetOrCreateComponent<LocomotionSystem>();
            CreateLocomotionProviders();
        }

        protected virtual void CreateLocomotionProviders()
        {
            CreateLocomotionProviderTeleport();
            CreateLocomotionProviderContinuousMove();
            CreateLocomotionProviderContiuousTurn();
            CreateLocomotionProviderDirectMove();
            CreateLocomotionProviderDirectTurn();
        }

        protected virtual T CreateLocomotionProvider<T>(DefaultLocomotionProvider lProvider) where T : LocomotionProvider
        {
            T result = gameObject.GetComponent<T>();
            bool lProviderEnabled = false;
            if (result)
            {
                lProviderEnabled = result.enabled;
                DestroyImmediate(result);
            }

            result = gameObject.AddComponent<T>();
            result.system = _xrRig.GetComponent<LocomotionSystem>();
            result.enabled = lProviderEnabled;

            _locomotionProviders[lProvider] = result;

            return result;
        }

        protected virtual TeleportationProvider CreateLocomotionProviderTeleport()
        {
            TeleportationProvider result = CreateLocomotionProvider<TeleportationProvider>(DefaultLocomotionProvider.Teleport);
            
            result.beginLocomotion += BeginLocomotionTeleport;
            result.endLocomotion += EndLocomotionTeleport;

            return result;
        }

        protected virtual void BeginLocomotionTeleport(LocomotionSystem obj)
        {
            _characterController.enabled = false;
        }

        protected virtual void EndLocomotionTeleport(LocomotionSystem obj)
        {
            _characterController.enabled = true;
        }

        protected virtual ActionBasedContinuousMoveProvider CreateLocomotionProviderContinuousMove()
        {
            ActionBasedContinuousMoveProvider result = CreateLocomotionProvider<ActionBasedContinuousMoveProvider>(DefaultLocomotionProvider.ContinuousMove);
            if (!result.leftHandMoveAction.action.IsValid())
            {
                result.leftHandMoveAction = new InputActionProperty(InputManager.GetInputActionsDefault().FindAction("General/Move"));
            }

            return result;
        }

        protected virtual ActionBasedContinuousTurnProvider CreateLocomotionProviderContiuousTurn()
        {
            ActionBasedContinuousTurnProvider result = CreateLocomotionProvider<ActionBasedContinuousTurnProvider>(DefaultLocomotionProvider.ContinuousTurn);
            if (!result.rightHandTurnAction.action.IsValid())
            {
                result.rightHandTurnAction = new InputActionProperty(InputManager.GetInputActionsDefault().FindAction("General/RotateCamera"));
            }

            return result;
        }

        protected virtual QuickDirectMoveProvider CreateLocomotionProviderDirectMove()
        {
            QuickDirectMoveProvider result = CreateLocomotionProvider<QuickDirectMoveProvider>(DefaultLocomotionProvider.DirectMove);

            return result;
        }

        protected virtual QuickDirectTurnProvider CreateLocomotionProviderDirectTurn()
        {
            QuickDirectTurnProvider result = CreateLocomotionProvider<QuickDirectTurnProvider>(DefaultLocomotionProvider.DirectTurn);

            return result;
        }

        protected virtual void CheckPrefabs()
        {
            if (_pfInteractorGrabDirect == null)
            {
                _pfInteractorGrabDirect = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_GRAB_DIRECT);
            }
            if (_pfInteractorGrabRay == null)
            {
                _pfInteractorGrabRay = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_GRAB_RAY);
            }
            if (_pfInteractorTeleportRay == null)
            {
                _pfInteractorTeleportRay = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_TELEPORT_RAY);
            }
            if (_pfInteractorUIRay == null)
            {
                _pfInteractorUIRay = Resources.Load<XRBaseControllerInteractor>(PF_INTERACTOR_UI_RAY);
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

        #region GET AND SET

        public virtual void SetEnabledLocomotionSystem(DefaultLocomotionProvider lProvider, bool enabled)
        {
            if (_locomotionProviders.TryGetValue(lProvider, out LocomotionProvider result))
            {
                if (enabled != result.enabled)
                {
                    result.enabled = enabled;
                }

                if (lProvider == DefaultLocomotionProvider.DirectMove || lProvider == DefaultLocomotionProvider.ContinuousMove)
                {
                    Animator aTarget = _vrManager.GetAnimatorTarget();
                    if (aTarget)
                    {
                        QuickLocomotionAnimationBase lAnimation = aTarget.GetComponent<QuickLocomotionAnimationBase>();
                        if (lAnimation)
                        {
                            lAnimation.enabled = IsEnabledLocomotionSystem(DefaultLocomotionProvider.ContinuousMove) || IsEnabledLocomotionSystem(DefaultLocomotionProvider.DirectMove);
                        }
                    }
                }
            }
        }

        public virtual bool IsEnabledLocomotionSystem(DefaultLocomotionProvider lProvider)
        {
            bool result = false;
            if (_locomotionProviders.TryGetValue(lProvider, out LocomotionProvider tmp))
            {
                result = tmp.enabled;
            }

            return result;
        }

        #endregion

        #region UPDATE

        protected virtual void OnGrabInteractable(SelectEnterEventArgs args)
        {
            foreach (Collider c in args.interactableObject.colliders)
            {
                Physics.IgnoreCollision(_characterController, c, true);

                foreach (Collider cHand in _handAnimatorLeft.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, true);
                }

                foreach (Collider cHand in _handAnimatorRight.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, true);
                }
            }
        }

        protected virtual void OnDropInteractable(SelectExitEventArgs args)
        {
            foreach (Collider c in args.interactableObject.colliders)
            {
                Physics.IgnoreCollision(_characterController, c, false);

                foreach (Collider cHand in _handAnimatorLeft.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, false);
                }

                foreach (Collider cHand in _handAnimatorRight.GetColliders())
                {
                    Physics.IgnoreCollision(cHand, c, false);
                }
            }
        }

        protected virtual void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Alpha1))
            //{
            //    _interactorHandLeft.EnableInteractorGrab(!_interactorHandLeft.IsEnabledInteractorGrab());
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha2))
            //{
            //    _interactorHandLeft.EnableInteractorTeleport(!_interactorHandLeft.IsEnabledInteractorTeleport());
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha3))
            //{
            //    _interactorHandLeft.EnableInteractorUI(!_interactorHandLeft.IsEnabledInteractorUI());
            //}

            //if (Input.GetKeyDown(KeyCode.Alpha4))
            //{
            //    _interactorHandRight.EnableInteractorGrab(!_interactorHandRight.IsEnabledInteractorGrab());
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha5))
            //{
            //    _interactorHandRight.EnableInteractorTeleport(!_interactorHandRight.IsEnabledInteractorTeleport());
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha6))
            //{
            //    _interactorHandRight.EnableInteractorUI(!_interactorHandRight.IsEnabledInteractorUI());
            //}
        }

        protected virtual void UpdateNewAnimatorTarget(Animator animator)
        {
            _xrRig.Origin = animator.gameObject;   //Configure the XRRig to act in this animator
            _characterController = animator.transform.GetOrCreateComponent<CharacterController>();
            foreach (XRGrabInteractable g in _grabInteractables)
            {
                foreach (Collider c in g.GetComponentsInChildren<Collider>(true))
                {
                    Physics.IgnoreCollision(_characterController, c, true);
                }
            }

            _interactorHandLeft.UpdateNewAnimatorTarget(animator);
            _interactorHandRight.UpdateNewAnimatorTarget(animator);

            //_interactorHandLeft.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            //_interactorHandLeft.transform.ResetTransformation();
            //_interactorHandLeft.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), transform.up);

            //_interactorHandRight.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            //_interactorHandRight.transform.ResetTransformation();
            //_interactorHandRight.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), transform.up);

            //The ContinuousMoveProvider needs to be recreated each time the target animator changes, as 
            //the CharacterController is cached in that behaviour, and as it is a private member, it cannot be accessed
            //in any way outside the class. 
            CreateLocomotionProviderContinuousMove().forwardSource = animator.transform;
            CreateLocomotionProviderDirectMove().forwardSource = animator.transform;
        }

        protected virtual void SetHandInteractorPosition(Animator animator, bool isLeft)
        {
            Transform tInteractor = isLeft ? _interactorHandLeft.transform : _interactorHandRight.transform;
            Transform tHand = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            Transform tMiddle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);

            //Define the position of the interactor
            tInteractor.parent = tHand;
            tInteractor.transform.ResetTransformation();
            tInteractor.transform.LookAt(tMiddle, transform.up);
            tInteractor.transform.position = Vector3.Lerp(tHand.position, tMiddle.position, 0.5f);

            //Define the radius
            tInteractor.GetComponent<SphereCollider>().radius = Vector3.Distance(tHand.position, tMiddle.position) * 0.5f;
        }
        
        protected virtual void UpdateCharacterController()
        {
            if (_characterController)
            {
                Animator animator = _vrManager.GetAnimatorTarget();

                //Compute the height of the collider
                float h = animator.GetEyeCenterVR().position.y - animator.transform.position.y;
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
            Camera cam = QuickVRCameraController.GetCamera();
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


