using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{

    public class QuickVRInteractionManager : MonoBehaviour
    {
        #region PUBLIC ATTRIBUTES

        public ActionBasedController _pfInteractorDirect = null;
        public ActionBasedController _pfInteractorTeleport = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickXRRig _xrRig = null;
        protected QuickVRControllerInteractor _controllerHandLeft = null;
        protected QuickVRControllerInteractor _controllerHandRight = null;
        protected LocomotionSystem _locomotionSystem = null;
        protected TeleportationProvider _teleportProvider = null;
        protected ActionBasedContinuousMoveProvider _continousMoveProvider = null;
        protected ActionBasedContinuousTurnProvider _continousRotationProvider = null;

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

            _controllerHandLeft = transform.CreateChild("__ControllerHandLeft__").GetOrCreateComponent<QuickVRControllerInteractor>();
            _controllerHandLeft._xrNode = XRNode.LeftHand;

            _controllerHandRight = transform.CreateChild("__ControllerHandRight__").GetOrCreateComponent<QuickVRControllerInteractor>();
            _controllerHandRight._xrNode = XRNode.RightHand;

            BaseTeleportationInteractable[] teleportationInteractables = FindObjectsOfType<BaseTeleportationInteractable>();
            foreach (BaseTeleportationInteractable t in teleportationInteractables)
            {
                t.teleportationProvider = _teleportProvider;
            }

            _continousMoveProvider = gameObject.GetOrCreateComponent<ActionBasedContinuousMoveProvider>();
            //_continousMoveProvider.forwardSource = _xrRig.transform;
            
            _continousRotationProvider = gameObject.GetOrCreateComponent<ActionBasedContinuousTurnProvider>();
        }

        protected virtual void Reset()
        {
            _xrRig = gameObject.GetOrCreateComponent<QuickXRRig>();
            _locomotionSystem = gameObject.GetOrCreateComponent<LocomotionSystem>();
            _teleportProvider = gameObject.GetOrCreateComponent<TeleportationProvider>();
        }

        protected virtual IEnumerator Start()
        {
            //Delay the addition of the CharacterControllerDriver and Character controller until the 
            //XRRig cameraGameObject is initialized
            while (!_xrRig.cameraGameObject) yield return null;

            Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
            if (animator)
            {
                _xrRig.GetOrCreateComponent<CharacterControllerDriver>();
                animator.transform.GetOrCreateComponent<CharacterController>();
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
            QuickVRManager.OnTargetAnimatorSet += OnSetAnimatorTarget;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnTargetAnimatorSet -= OnSetAnimatorTarget;
        }

        #endregion

        #region UPDATE

        protected virtual void OnSetAnimatorTarget(Animator animator)
        {
            _xrRig.rig = animator.gameObject;   //Configure the XRRig to act in this animator
            if (_xrRig.cameraGameObject)
            {
                _xrRig.GetOrCreateComponent<CharacterControllerDriver>();
                animator.transform.GetOrCreateComponent<CharacterController>();
            }
            
            _controllerHandLeft.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _controllerHandLeft.transform.ResetTransformation();
            _controllerHandLeft.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), transform.up);

            _controllerHandRight.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _controllerHandRight.transform.ResetTransformation();
            _controllerHandRight.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), transform.up);
        }

        #endregion

    }

}


