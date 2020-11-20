using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace QuickVR
{

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

        #endregion

        #region PROTECTED PARAMETERS

        protected Animator _animatorTarget = null;
        protected Animator _animatorSource = null;

        protected QuickUnityVR _unityVR = null;
        protected QuickBaseTrackingManager _handTracking = null;
        protected List<QuickBaseTrackingManager> _bodyTrackingSystems = new List<QuickBaseTrackingManager>();
        protected List<QuickBaseTrackingManager> _ikManagerSystems = new List<QuickBaseTrackingManager>();

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
        
        #endregion

        #region EVENTS

        public delegate void QuickVRManagerAction();

        public static event QuickVRManagerAction OnPreCalibrate;
        public static event QuickVRManagerAction OnPostCalibrate;

        public static event QuickVRManagerAction OnPreUpdateTrackingEarly;
        public static event QuickVRManagerAction OnPostUpdateTrackingEarly;

        public static event QuickVRManagerAction OnPreUpdateTracking;
        public static event QuickVRManagerAction OnPostUpdateTracking;

        public static event QuickVRManagerAction OnPreCameraUpdate;
        public static event QuickVRManagerAction OnPostCameraUpdate;

        public static event QuickVRManagerAction OnSourceAnimatorSet;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _copyPose.enabled = false;
            _cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();

            //Legacy XR Mode is deprecated on 2020 onwards. 
#if UNITY_2020_1_OR_NEWER
            _XRMode = XRMode.XRPlugin;
#endif

        }

        #endregion

        #region GET AND SET

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
            _animatorTarget = animator;
            _animatorTarget.CreateEyes();

            _copyPose.SetAnimatorDest(_animatorTarget);
        }

        public virtual Animator GetAnimatorSource()
        {
            return _animatorSource;
        }

        protected virtual void SetAnimatorSource(Animator animator)
        {
            _animatorSource = animator;
            _animatorSource.CreateEyes();

            _copyPose.SetAnimatorSource(_animatorSource);
            if (OnSourceAnimatorSet != null) OnSourceAnimatorSet();
        }

        public virtual QuickVRCameraController GetCameraController()
        {
            return _cameraController;
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

            _copyPose.CopyPose();

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

