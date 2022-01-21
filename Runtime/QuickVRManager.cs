using UnityEngine;
using UnityEngine.XR;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    [DefaultExecutionOrder(-1000)]
    public class QuickVRManager : MonoBehaviour
    {

        //#region EXECUTION ORDER MANAGERS

        //[DefaultExecutionOrder(-1000)] //Execute as soon as possible
        //protected class QuickVRManagerEarly : MonoBehaviour
        //{
        //    public static event QuickVRManagerAction OnUpdate;

        //    protected virtual void Update()
        //    {
        //        if (OnUpdate != null)
        //        {
        //            OnUpdate();
        //        }
        //    }
        //}

        //[DefaultExecutionOrder(1000)] //Execute as late as possible
        //protected class QuickVRManagerLate : MonoBehaviour
        //{
        //    public static event QuickVRManagerAction OnLateUpdate;

        //    protected virtual void LateUpdate()
        //    {
        //        if (OnLateUpdate != null)
        //        {
        //            OnLateUpdate();
        //        }
        //    }
        //}

        //#endregion

        #region PUBLIC ATTRIBUTES

        public enum LogMode
        {
            Message,
            Warning,
            Error,
        }
        [BitMask(typeof(LogMode))]
        public int _logMode = -1;

        public bool _showFPS = false;

        public enum XRMode
        {
            LegacyXRSettings,
            XRPlugin
        }
        public XRMode _XRMode = XRMode.LegacyXRSettings;

        public enum HMDModel
        {
            None, 

            OculusQuest,
            OculusQuest2,

            HTCVive,

            PicoNeo2,
            PicoNeo3,
        }

        public enum HandTrackingMode
        {
            Controllers,
            Hands,
        }
        public static HandTrackingMode _handTrackingMode { get; set; }

        #endregion

        #region PROTECTED PARAMETERS

        protected static QuickVRManager _instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = QuickSingletonManager.GetInstance<QuickVRManager>();
                }

                return m_Instance;
            }
        }
        private static QuickVRManager m_Instance = null;

        public static HMDModel _hmdModel
        {
            get 
            {

                if (IsXREnabled() && m_HMDModel == HMDModel.None)
                {
                    List<InputDevice> devices = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);

                    string hmdName = devices.Count > 0 ? devices[0].name.Replace(" ", "").ToLower() : "";
                    if (hmdName.Contains("quest"))
                    {
                        if (hmdName.Contains("2"))
                        {
                            m_HMDModel = HMDModel.OculusQuest2;
                        }
                        else
                        {
                            m_HMDModel = HMDModel.OculusQuest;
                        }
                    }
                    else if (hmdName.Contains("vive"))
                    {
                        m_HMDModel = HMDModel.HTCVive;
                    }
                    else if (hmdName.Contains("piconeo"))
                    {
                        if (hmdName.Contains("2"))
                        {
                            m_HMDModel = HMDModel.PicoNeo2;
                        }
                        else
                        {
                            m_HMDModel = HMDModel.PicoNeo3;
                        }
                    }
                }

                return m_HMDModel;
            } 
        }
        private static HMDModel m_HMDModel = HMDModel.None;

        protected Animator _animatorTarget = null;
        protected Animator _animatorSource = null;

        protected QuickUnityVR _unityVR = null;

        protected QuickVRPlayArea _vrPlayArea = null;
        protected QuickVRCameraController _cameraController = null;
        protected InputManager _inputManager = null;
        protected PerformanceFPS _fpsCounter = null;
        protected QuickCopyPoseBase _copyPose = null;
        
        protected QuickVRInteractionManager _interactionManager = null;

        protected bool _isCalibrationRequired = false;

        #endregion

        #region EVENTS

        public delegate void QuickVRManagerAction();

        public static event QuickVRManagerAction OnPreCalibrate;
        public static event QuickVRManagerAction OnPostCalibrate;

        public static event QuickVRManagerAction OnPreUpdateVRNodes;
        public static event QuickVRManagerAction OnPostUpdateVRNodes;

        public static event QuickVRManagerAction OnPreUpdateIKTargets;
        public static event QuickVRManagerAction OnPostUpdateIKTargets;

        public static event QuickVRManagerAction OnPreUpdateTracking;
        public static event QuickVRManagerAction OnPostUpdateTracking;

        public static event QuickVRManagerAction OnPreCopyPose;
        public static event QuickVRManagerAction OnPostCopyPose;

        public static event QuickVRManagerAction OnPreCameraUpdate;
        public static event QuickVRManagerAction OnPostCameraUpdate;

        public delegate void QuickVRManagerActionAnimator(Animator animator);
        public static event QuickVRManagerActionAnimator OnSourceAnimatorSet;
        public static event QuickVRManagerActionAnimator OnTargetAnimatorSet;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            //If we are in the Editor, the UpdateTracking function is called on LateUpdate, as onBeforeRender seems
            //not to be properly called when in Editor, i.e., the Animation is not correctly blended with the tracking. 
            //On Build, we do the UpdateTracking onBeforeRender because this provide the smoothest possible result as we
            //are delaying the application of the tracking as much as possible. onBeforeRender works as expected on Build. 
            if (!Application.isEditor)
            {
                Application.onBeforeRender += UpdateTracking;
            }

            //QuickVRManagerEarly.OnUpdate += UpdateEarly;
            //QuickVRManagerLate.OnLateUpdate += UpdateTracking;
            //Application.onBeforeRender += UpdateCamera;
        }

        protected virtual void OnDisable()
        {
            if (!Application.isEditor)
            {
                Application.onBeforeRender -= UpdateTracking;
            }

            //QuickVRManagerEarly.OnUpdate -= UpdateEarly;
            //QuickVRManagerLate.OnLateUpdate -= UpdateTracking;
            //Application.onBeforeRender -= UpdateCamera;
        }

        protected virtual void Awake()
        {
            Reset();

            _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();
            _inputManager = QuickSingletonManager.GetInstance<InputManager>();
            _fpsCounter = QuickSingletonManager.GetInstance<PerformanceFPS>();

            _copyPose = gameObject.GetOrCreateComponent<QuickCopyPoseBase>();
            //_copyPose = gameObject.GetOrCreateComponent<QuickCopyPoseDirect>();
            _copyPose.enabled = false;

            //Legacy XR Mode is deprecated on 2020 onwards. 
#if UNITY_2020_1_OR_NEWER
            _XRMode = XRMode.XRPlugin;
#endif
        }

        protected virtual void Reset()
        {
            name = "__QuickVRManager__";
            transform.ResetTransformation();
            _interactionManager = gameObject.GetOrCreateComponent<QuickVRInteractionManager>();
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
            if (_animatorTarget != null)
            {
                _animatorTarget.transform.parent = null;
            }
            _animatorTarget = animator;

            animator.CreateMissingBones();

            QuickCameraZNearDefiner zNearDefiner = _animatorTarget.GetComponent<QuickCameraZNearDefiner>();
            if (zNearDefiner)
            {
                _cameraController._cameraNearPlane = zNearDefiner._zNear;
            }

            _copyPose.SetAnimatorDest(animator);
            if (OnTargetAnimatorSet != null)
            {
                OnTargetAnimatorSet(animator);
            }
        }

        public virtual Animator GetAnimatorSource()
        {
            return _animatorSource;
        }

        protected virtual void SetAnimatorSource(Animator animator)
        {
            _animatorSource = animator;

            _copyPose.SetAnimatorSource(animator);
            if (OnSourceAnimatorSet != null)
            {
                OnSourceAnimatorSet(animator);
            }
        }

        public virtual void AddUnityVRTrackingSystem(QuickUnityVR unityVR)
        {
            _unityVR = unityVR;

            Animator animator = _unityVR.GetComponent<Animator>();
            SetAnimatorSource(animator);
            SetAnimatorTarget(animator);
        }

        public virtual void RequestCalibration()
        {
            _isCalibrationRequired = true;
        }

        protected virtual void Calibrate()
        {
            if (OnPreCalibrate != null) OnPreCalibrate();

            if (_unityVR)
            {
                _unityVR.Calibrate();
            }

            _isCalibrationRequired = false;

            if (OnPostCalibrate != null) OnPostCalibrate();
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        //protected virtual void UpdateEarly()
        {
            //Update the InputState
            _inputManager.UpdateState();

            if (_fpsCounter.gameObject.activeSelf != _showFPS)
            {
                _fpsCounter.gameObject.SetActive(_showFPS);
            }
            
            //Calibrate the TrackingManagers that needs to be calibrated. 
            if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CALIBRATE) || _isCalibrationRequired)
            {
                Calibrate();
            }
        }

#if UNITY_EDITOR
        protected virtual void LateUpdate()
        {
            UpdateTracking();
        }
#endif

        protected virtual void UpdateTracking()
        {
            Vector3 tmpPos = _animatorSource.transform.position;
            _animatorSource.transform.position = Vector3.zero;

            //Update the VRNodes
            if (OnPreUpdateVRNodes != null) OnPreUpdateVRNodes();
            _vrPlayArea.UpdateVRNodes();
            if (OnPostUpdateVRNodes != null) OnPostUpdateVRNodes();

            //Update the IKTargets with the tracking information
            if (OnPreUpdateIKTargets != null) OnPreUpdateIKTargets();
            if (_unityVR)
            {
                _unityVR.UpdateIKTargets();
            }
            if (OnPostUpdateIKTargets != null) OnPostUpdateIKTargets();
            
            //Apply the tracking of the VRNodes
            if (OnPreUpdateTracking != null) OnPreUpdateTracking();
            if (_unityVR)
            {
                _unityVR.UpdateTracking();
            }
            _animatorSource.transform.position = tmpPos;
            if (OnPostUpdateTracking != null) OnPostUpdateTracking();

            //Copy the pose of the source avatar to the target avatar
            if (OnPreCopyPose != null) OnPreCopyPose();
            _copyPose.CopyPose();
            if (OnPostCopyPose != null) OnPostCopyPose();

            UpdateCamera();
        }

        protected virtual void UpdateCamera()
        {
            //Update the Camera position
            if (OnPreCameraUpdate != null) OnPreCameraUpdate();
            _cameraController.UpdateCameraPosition(_animatorTarget);
            if (OnPostCameraUpdate != null) OnPostCameraUpdate();
        }

        #endregion

        public static void Log(object message)
        {
            if ((_instance._logMode & (1 << (int)LogMode.Message)) != 0)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if ((_instance._logMode & (1 << (int)LogMode.Warning)) != 0)
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogError(object message)
        {
            if ((_instance._logMode & (1 << (int)LogMode.Error)) != 0)
            {
                Debug.LogError(message);
            }
        }

    }

}

