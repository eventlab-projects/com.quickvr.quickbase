using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace QuickVR.SampleInteraction
{

    public class TestInteractionUI : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        [Header("General Settings")]
        public float _guiDistance = 2.5f;

        /// <summary>
        /// The gui will update its orientation once the angle formed by the forward of the target avatar and the forward of 
        /// the gui is greater than _guiDeadAngle. 
        /// </summary>
        public float _guiDeadAngle = 45.0f; 

        [Header("Locomotion")]
        public Toggle _toggleDirectMove = null;
        public Toggle _toggleDirectTurn = null;
        public Toggle _toggleContinuousMove = null;
        public Toggle _toggleContinuousTurn = null;
        public Toggle _toggleTeleportLeftHand = null;
        public Toggle _toggleTeleportRightHand = null;

        [Header("Interaction")]
        public Toggle _toggleGrabDirectLeftHand = null;
        public Toggle _toggleGrabDirectRightHand = null;
        public Toggle _toggleGrabRayLeftHand = null;
        public Toggle _toggleGrabRayRightHand = null;
        public Toggle _toggleUIRayLeftHand = null;
        public Toggle _toggleUIRayRightHand = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRInteractionManager _interactionManager = null;
        protected QuickVRInteractor _interactorLeftHand = null;
        protected QuickVRInteractor _interactorRightHand = null;

        protected bool _show = true;

        protected CanvasGroup _canvasGroup = null;

        protected Coroutine _coUpdatePosition = null;

        protected Animator _animator = null;
        protected Coroutine _coUpdateTargetFwd = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            QuickVRManager.OnTargetAnimatorSet += OnTargetAnimatorSetAction;
        }

        protected virtual void OnDestroy()
        {
            QuickVRManager.OnTargetAnimatorSet -= OnTargetAnimatorSetAction;
        }

        protected virtual void OnEnable()
        {
            _coUpdatePosition = StartCoroutine(CoUpdatePosition());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            _coUpdatePosition = null;
            _coUpdateTargetFwd = null;
        }

        protected virtual void Start()
        {
            _canvasGroup = gameObject.GetOrCreateComponent<CanvasGroup>();
            _interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();
            _interactorLeftHand = _interactionManager._interactorHandLeft;
            _interactorRightHand = _interactionManager._interactorHandRight;

            _toggleUIRayRightHand.isOn = true;
        }

        #endregion

        #region GET AND SET

        protected virtual void OnTargetAnimatorSetAction(Animator animator)
        {
            _animator = animator;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (InputManagerVR.GetKeyDown(InputManagerVR.ButtonCodes.LeftPrimaryPress) || InputManagerKeyboard.GetKeyDown(UnityEngine.InputSystem.Key.X))
            {
                _show = !_show;

                _canvasGroup.alpha = _show ? 1 : 0;
                _canvasGroup.blocksRaycasts = _show;
                _canvasGroup.interactable = _show;

                if (_show)
                {
                    //By default, enable the ui interactor for the right hand. 
                    _toggleUIRayRightHand.isOn = true;
                }
            }

            if (_show && _interactionManager)
            {
                //Locomotion
                _interactionManager.SetEnabledLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.DirectMove, _toggleDirectMove.isOn);
                _interactionManager.SetEnabledLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.DirectTurn, _toggleDirectTurn.isOn);
                _interactionManager.SetEnabledLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.ContinuousMove, _toggleContinuousMove.isOn);
                _interactionManager.SetEnabledLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.ContinuousTurn, _toggleContinuousTurn.isOn);
                _interactionManager.SetEnabledLocomotionSystem(QuickVRInteractionManager.DefaultLocomotionProvider.Teleport, _toggleTeleportLeftHand.isOn || _toggleTeleportRightHand.isOn);
                _interactorLeftHand.SetInteractorEnabled(InteractorType.Teleport, _toggleTeleportLeftHand.isOn);
                _interactorRightHand.SetInteractorEnabled(InteractorType.Teleport, _toggleTeleportRightHand.isOn);

                //Interaction
                _interactorLeftHand.SetInteractorEnabled(InteractorType.GrabDirect, _toggleGrabDirectLeftHand.isOn);
                _interactorRightHand.SetInteractorEnabled(InteractorType.GrabDirect, _toggleGrabDirectRightHand.isOn);
                _interactorLeftHand.SetInteractorEnabled(InteractorType.Grab, _toggleGrabRayLeftHand.isOn);
                _interactorRightHand.SetInteractorEnabled(InteractorType.Grab, _toggleGrabRayRightHand.isOn);
                _interactorLeftHand.SetInteractorEnabled(InteractorType.UI, _toggleUIRayLeftHand.isOn);
                _interactorRightHand.SetInteractorEnabled(InteractorType.UI, _toggleUIRayRightHand.isOn);
            }
        }

        protected virtual IEnumerator CoUpdatePosition()
        {
            while (true)
            {
                if (_animator)
                {
                    if ((Vector3.Angle(transform.forward, _animator.transform.forward) > 45) && _coUpdateTargetFwd == null)
                    {
                        _coUpdateTargetFwd = StartCoroutine(CoUpdateTargetForward());
                    }

                    Vector3 offset = transform.forward * _guiDistance + _animator.transform.up;
                    transform.position = _animator.transform.position + offset;
                }

                yield return null;
            }
        }

        protected virtual IEnumerator CoUpdateTargetForward()
        {
            while (Vector3.Angle(transform.forward, _animator.transform.forward) > 1)
            {
                transform.forward = Vector3.Lerp(transform.forward, _animator.transform.forward, Time.deltaTime);

                yield return null;
            }

            _coUpdateTargetFwd = null;
        }

        #endregion

    }

}


