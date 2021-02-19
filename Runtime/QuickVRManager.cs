using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace QuickVR
{

    public class QuickXRRig : XRRig
    {

        protected new void Awake()
        {

        }

        protected new IEnumerator Start()
        {
            while (!Camera.main)
            {
                yield return null;
            }

            cameraGameObject = Camera.main.gameObject;
            cameraFloorOffsetObject = Camera.main.transform.parent.gameObject;
        }

    }

    public class QuickVRManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _showFPS = false;

        public enum XRMode
        {
            LegacyXRSettings,
            XRPlugin
        }
        public XRMode _XRMode = XRMode.LegacyXRSettings;

        public enum HMDModel
        {
            Generic, 
            OculusQuest
        }
        public static HMDModel _hmdModel = HMDModel.Generic;

        #endregion

        #region PROTECTED PARAMETERS

        protected Animator _animatorTarget = null;
        protected Animator _animatorSource = null;

        protected QuickVRControllerInteractor _controllerHandLeft = null;
        protected QuickVRControllerInteractor _controllerHandRight = null;

        protected QuickUnityVR _unityVR = null;
        protected QuickBaseTrackingManager _handTracking = null;
        protected List<QuickBaseTrackingManager> _bodyTrackingSystems = new List<QuickBaseTrackingManager>();
        protected List<QuickBaseTrackingManager> _ikManagerSystems = new List<QuickBaseTrackingManager>();

        protected QuickXRRig _xrRig = null;
        protected LocomotionSystem _locomotionSystem = null;
        protected TeleportationProvider _teleportProvider = null;
        protected DeviceBasedContinuousMoveProvider _continousMoveProvider = null;

        protected QuickVRPlayArea _vrPlayArea
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            }
        }

        protected static InputManager _inputManager
        {
            get
            {
                return QuickSingletonManager.GetInstance<InputManager>();
            }
        }

        protected QuickCopyPoseBase _copyPose
        {
            get
            {
                return gameObject.GetOrCreateComponent<QuickCopyPoseBase>();
            }
        }

        protected static PerformanceFPS _fpsCounter
        {
            get
            {
                return QuickSingletonManager.GetInstance<PerformanceFPS>();
            }
        }

        protected QuickVRCameraController _cameraController = null;

        protected bool _isCalibrationRequired = false;

        protected static string _hmdName = "";
        
        #endregion

        #region EVENTS

        public delegate void QuickVRManagerAction();

        public static event QuickVRManagerAction OnPreCalibrate;
        public static event QuickVRManagerAction OnPostCalibrate;

        public static event QuickVRManagerAction OnPreUpdateTrackingEarly;
        public static event QuickVRManagerAction OnPostUpdateTrackingEarly;

        public static event QuickVRManagerAction OnPreUpdateTracking;
        public static event QuickVRManagerAction OnPostUpdateTracking;

        public static event QuickVRManagerAction OnPreCopyPose;
        public static event QuickVRManagerAction OnPostCopyPose;

        public static event QuickVRManagerAction OnPreCameraUpdate;
        public static event QuickVRManagerAction OnPostCameraUpdate;

        public static event QuickVRManagerAction OnSourceAnimatorSet;
        public static event QuickVRManagerAction OnTargetAnimatorSet;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _copyPose.enabled = false;
            _cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();

            _xrRig = new GameObject("__XRRig__").AddComponent<QuickXRRig>();
            _controllerHandLeft = _xrRig.transform.CreateChild("__ControllerHandLeft__").GetOrCreateComponent<QuickVRControllerInteractor>();
            _controllerHandLeft._xrNode = XRNode.LeftHand;

            _controllerHandRight = _xrRig.transform.CreateChild("__ControllerHandRight__").GetOrCreateComponent<QuickVRControllerInteractor>();
            _controllerHandRight._xrNode = XRNode.RightHand;

            _locomotionSystem = _xrRig.GetOrCreateComponent<LocomotionSystem>();
            _teleportProvider = _xrRig.GetOrCreateComponent<TeleportationProvider>();
            BaseTeleportationInteractable[] teleportationInteractables = FindObjectsOfType<BaseTeleportationInteractable>();
            foreach (BaseTeleportationInteractable t in teleportationInteractables)
            {
                t.teleportationProvider = _teleportProvider;
            }

            _continousMoveProvider = _xrRig.GetOrCreateComponent<DeviceBasedContinuousMoveProvider>();
            _continousMoveProvider.forwardSource = _xrRig.transform;
            _continousMoveProvider.controllers.Add(_controllerHandLeft.GetInteractorDirectController());

            _xrRig.GetOrCreateComponent<CharacterControllerDriver>();
            _xrRig.GetOrCreateComponent<CharacterController>();

            //Legacy XR Mode is deprecated on 2020 onwards. 
#if UNITY_2020_1_OR_NEWER
            _XRMode = XRMode.XRPlugin;
#endif

        }

        #endregion

        #region GET AND SET

        public static string GetHMDName()
        {
            if (_hmdName.Length == 0 && IsXREnabled())
            {
                List<InputDevice> devices = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);

                _hmdName = devices.Count > 0 ? devices[0].name.ToLower() : "";
            }

            return _hmdName;
        }

        public static bool IsOculusQuest()
        {
            string hmdName = GetHMDName();
            bool isQuest = hmdName.Contains("quest");

#if UNITY_ANDROID
            isQuest = isQuest || hmdName.Contains("oculus");
#endif

            return isQuest;
        }

        public static bool IsHandTrackingSupported()
        {
#if UNITY_WEBGL
            return false;
#else
            return IsOculusQuest();
#endif
        }

        public static bool IsXREnabled()
        {
            return UnityEngine.XR.XRSettings.enabled;
        }

        public virtual Animator GetAnimatorTarget()
        {
            return _animatorTarget;
        }

        public virtual void SetAnimatorTarget(Animator animator)
        {
            if (_animatorTarget != null)
            {
                _animatorTarget.transform.parent = null;
            }
            _animatorTarget = animator;
            
            QuickCameraZNearDefiner zNearDefiner = _animatorTarget.GetComponent<QuickCameraZNearDefiner>();
            if (zNearDefiner)
            {
                _cameraController._cameraNearPlane = zNearDefiner._zNear;
            }

            //Configure the XRRig to act in this animator
            _xrRig.transform.position = animator.transform.position;
            _xrRig.transform.rotation = animator.transform.rotation;
            animator.transform.parent = _xrRig.transform;
            _cameraController.transform.parent = _xrRig.transform;

            _controllerHandLeft.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _controllerHandLeft.transform.ResetTransformation();
            _controllerHandLeft.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), transform.up);

            _controllerHandRight.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _controllerHandRight.transform.ResetTransformation();
            _controllerHandRight.transform.LookAt(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), transform.up);

            _copyPose.SetAnimatorDest(_animatorTarget);
            if (OnTargetAnimatorSet != null) OnTargetAnimatorSet();
        }

        protected virtual void ActionTargetAnimatorSet()
        {
            
        }

        public virtual Animator GetAnimatorSource()
        {
            return _animatorSource;
        }

        protected virtual void SetAnimatorSource(Animator animator)
        {
            _animatorSource = animator;

            _copyPose.SetAnimatorSource(_animatorSource);
            if (OnSourceAnimatorSet != null) OnSourceAnimatorSet();
        }

        public virtual void AddUnityVRTrackingSystem(QuickUnityVR unityVR)
        {
            _unityVR = unityVR;

            Animator animator = _unityVR.GetComponent<Animator>();
            SetAnimatorSource(animator);
            SetAnimatorTarget(animator);
        }

        public virtual void AddBodyTrackingSystem(QuickBaseTrackingManager bTracking)
        {
            _bodyTrackingSystems.Add(bTracking);
        }

        public virtual void AddIKManagerSystem(QuickBaseTrackingManager ikManager)
        {
            _ikManagerSystems.Add(ikManager);
        }

        public virtual void AddHandTrackingSystem(QuickBaseTrackingManager handTracking)
        {
            _handTracking = handTracking;
        }

        protected virtual List<QuickBaseTrackingManager> GetAllTrackingSystems()
        {
            List<QuickBaseTrackingManager> result = new List<QuickBaseTrackingManager>();
            result.Add(_unityVR);
            result.AddRange(_bodyTrackingSystems);
            result.AddRange(_ikManagerSystems);

            return result;
        }

        public virtual void RequestCalibration()
        {
            _isCalibrationRequired = true;
        }

        protected virtual void Calibrate()
        {
            if (OnPreCalibrate != null) OnPreCalibrate();

            foreach (QuickBaseTrackingManager tm in GetAllTrackingSystems())
            {
                if (tm.gameObject.activeInHierarchy)
                {
                    tm.Calibrate();
                }
            }

            _isCalibrationRequired = false;

            if (OnPostCalibrate != null) OnPostCalibrate();
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            _fpsCounter.gameObject.SetActive(_showFPS);

            //Calibrate the TrackingManagers that needs to be calibrated. 
            if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CALIBRATE) || _isCalibrationRequired)
            {
                Calibrate();
            }

            //Update the VRNodes
            _vrPlayArea.UpdateVRNodes();

            if (OnPreUpdateTrackingEarly != null) OnPreUpdateTrackingEarly();
            UpdateTracking(true);
            if (OnPostUpdateTrackingEarly != null) OnPostUpdateTrackingEarly();
        }

        protected virtual void LateUpdate()
        {
            //Update the TrackingManagers
            if (OnPreUpdateTracking != null) OnPreUpdateTracking();
            UpdateTracking(false);
            if (OnPostUpdateTracking != null) OnPostUpdateTracking();

            //Copy the pose of the source avatar to the target avatar
            if (OnPreCopyPose != null) OnPreCopyPose();
            _copyPose.CopyPose();
            if (OnPostCopyPose != null) OnPostCopyPose();

            //Update the Camera position
            if (OnPreCameraUpdate != null) OnPreCameraUpdate();
            _cameraController.UpdateCameraPosition(_animatorTarget);
            if (OnPostCameraUpdate != null) OnPostCameraUpdate();

            //Update the InputState
            _inputManager.UpdateState();
        }

        protected virtual void UpdateTracking(bool isEarly)
        {
            //1) Update the HeadTracking systems
            UpdateTracking(_unityVR, isEarly);
            
            //2) Update the BodyTracking systems
            foreach (QuickBaseTrackingManager bTracking in _bodyTrackingSystems)
            {
                UpdateTracking(bTracking, isEarly);
            }

            //3) Update the IKManager systems
            foreach (QuickBaseTrackingManager ikManager in _ikManagerSystems)
            {
                UpdateTracking(ikManager, isEarly);
            }

            //4) Update the hand tracking system
            UpdateTracking(_handTracking, isEarly);
        }

        protected virtual void UpdateTracking(QuickBaseTrackingManager tManager, bool isEarly)
        {
            if (tManager && tManager.enabled)
            {
                if (isEarly) tManager.UpdateTrackingEarly();
                else tManager.UpdateTrackingLate();
            }
        }

        #endregion

    }

}

