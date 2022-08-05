using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using QuickVR;

public class TestInteractionUI : MonoBehaviour
{

    #region PUBLIC ATTRIBUTES

    [Header("Locomotion")]
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

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Awake()
    {
        _canvasGroup = gameObject.GetOrCreateComponent<CanvasGroup>();
    }

    protected virtual IEnumerator Start()
    {
        QuickVRManager vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
        while (!vrManager.GetAnimatorTarget())
        {
            yield return null;
        }

        transform.parent = vrManager.GetAnimatorTarget().transform;

        _interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();
        _interactorLeftHand = _interactionManager.GetVRInteractorHandLeft();
        _interactorRightHand = _interactionManager.GetVRInteractorHandRight();

        _toggleUIRayRightHand.isOn = true;
    }

    #endregion

    #region UPDATE

    protected virtual void Update()
    {
        if (InputManagerVR.GetKeyDown(InputManagerVR.ButtonCodes.LeftPrimaryPress) || InputManagerKeyboard.GetKeyDown(UnityEngine.InputSystem.Key.A))
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

    #endregion

}
