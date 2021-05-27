using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickVRNode : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public enum Type
        {
            Hips,

            Head, 
            LeftEye,
            RightEye,

            LeftUpperArm, 
            LeftLowerArm, 
            LeftHand,

            RightUpperArm, 
            RightLowerArm,
            RightHand,

            LeftUpperLeg,
            LeftLowerLeg,
            LeftFoot, 

            RightUpperLeg,
            RightLowerLeg,
            RightFoot,

            LeftThumbProximal,
            LeftThumbIntermediate,
            LeftThumbDistal,
            LeftThumbTip,

            LeftIndexProximal,
            LeftIndexIntermediate,
            LeftIndexDistal,
            LeftIndexTip,
            
            LeftMiddleProximal,
            LeftMiddleIntermediate,
            LeftMiddleDistal,
            LeftMiddleTip,
            
            LeftRingProximal,
            LeftRingIntermediate,
            LeftRingDistal,
            LeftRingTip,
            
            LeftLittleProximal,
            LeftLittleIntermediate,
            LeftLittleDistal,
            LeftLittleTip,

            RightThumbProximal,
            RightThumbIntermediate,
            RightThumbDistal,
            RightThumbTip,

            RightIndexProximal,
            RightIndexIntermediate,
            RightIndexDistal,
            RightIndexTip,

            RightMiddleProximal,
            RightMiddleIntermediate,
            RightMiddleDistal,
            RightMiddleTip,

            RightRingProximal,
            RightRingIntermediate,
            RightRingDistal,
            RightRingTip,

            RightLittleProximal,
            RightLittleIntermediate,
            RightLittleDistal,
            RightLittleTip,
        };

        public bool _showModel = false;

        public enum UpdateMode
        {
            FromUser,
            FromCalibrationPose,
        }

        public UpdateMode _updateModePos = UpdateMode.FromUser;
        public UpdateMode _updateModeRot = UpdateMode.FromUser;

        public InputDevice _inputDevice { get; set; }

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickTrackedObject _trackedObject = null;

        protected static List<QuickHumanBodyBones> _typeList = new List<QuickHumanBodyBones>();

        protected Transform _model = null;

        protected QuickHumanBodyBones _role = QuickHumanBodyBones.Head;

        protected Transform _calibrationPose = null;
        
        [SerializeField, ReadOnly]
        protected bool _isTracked = false;

        #endregion

        #region CONSTANTS

        public static string CALIBRATION_POSE_PREFIX = "__CalibrationPose__";

        #endregion

        #region EVENTS

        public delegate void CalibrateVRNodeAction(QuickVRNode vrNode);
        public event CalibrateVRNodeAction OnCalibrateVRNode;

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            _typeList = QuickUtils.ParseEnum<QuickHumanBodyBones, Type>(QuickUtils.GetEnumValues<Type>());
        }

        protected virtual void Awake()
        {
            _trackedObject = transform.CreateChild("__TrackedObject__").gameObject.GetOrCreateComponent<QuickTrackedObject>();
        }

        protected virtual string GetVRModelName()
        {
            return "pf_VIVE_Tracker";
        }

        protected virtual void LoadVRModel()
        {
            if (_model)
            {
                DestroyImmediate(_model.gameObject);
            }

            string pfName = GetVRModelName();

            //if (pfName.Length != 0)
            //{
            //    _model = Instantiate<Transform>(Resources.Load<Transform>("Prefabs/" + pfName));
            //    _model.parent = transform;
            //    _model.ResetTransformation();
            //    _model.name = "Model";
            //}

            SetModelVisible(_showModel);
        }

        #endregion

        #region GET AND SET

        public virtual void SetTracked(bool isTracked)
        {
            _isTracked = isTracked;
        }

        public virtual bool IsTracked()
        {
            return _isTracked;
        }

        public virtual Transform GetCalibrationPose()
        {
            return _calibrationPose;
        }

        public virtual void SetCalibrationPose(Transform calibrationPose)
        {
            _calibrationPose = calibrationPose;
        }

        public virtual Transform GetModel()
        {
            return _model;
        }

        protected virtual void SetModelVisible(bool v)
        {
            if (_model && (_showModel != _model.gameObject.activeSelf))
            {
                _model.gameObject.SetActive(v);
            }
        }

        public virtual QuickHumanBodyBones GetRole()
        {
            return _role;
        }

        public virtual void SetRole(QuickHumanBodyBones role, bool resetUpdateMode = true)
        {
            _role = role;
            _calibrationPose.name = CALIBRATION_POSE_PREFIX + name;

            LoadVRModel();

            if (resetUpdateMode) ResetUpdateMode();

            _trackedObject.Reset();
        }

        protected virtual void ResetUpdateMode()
        {
            if (_role == QuickHumanBodyBones.Hips)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromCalibrationPose;
            }
            else if (_role == QuickHumanBodyBones.LeftFoot || _role == QuickHumanBodyBones.RightFoot)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromUser;
            }
        }

        public virtual QuickTrackedObject GetTrackedObject()
        {
            return _trackedObject;
        }

        public static List<QuickHumanBodyBones> GetTypeList()
        {
            return _typeList;
        }

        public virtual void Calibrate()
        {
            _trackedObject.transform.ResetTransformation();

            if (OnCalibrateVRNode != null)
            {
                OnCalibrateVRNode(this);
            }

            //Save the calibration pose
            _calibrationPose.position = _trackedObject.transform.position;
            _calibrationPose.rotation = _trackedObject.transform.rotation;
            _trackedObject.Reset();
        }

        protected virtual bool GetDevicePosition(out Vector3 pos)
        {
            return _inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos);
        }

        protected virtual bool GetDeviceRotation(out Quaternion rot)
        {
            return _inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot);
        }

        #endregion

        #region UPDATE

        protected virtual InputDevice CheckDevice()
        {
            return new InputDevice();
        }

        public virtual void UpdateState()
        {
            //try to find a valid inputdevice for the key roles
            if (!_inputDevice.isValid)
            {
                _inputDevice = CheckDevice();
            }
            
            if (_inputDevice.isValid)
            {
                UpdateTracking();
            }
            else
            {
                _isTracked = false;
            }
        }

        protected virtual void UpdateTracking()
        {
            if (GetDevicePosition(out Vector3 pos))
            {
                transform.localPosition = pos;
            }
            if (GetDeviceRotation(out Quaternion rot))
            {
                transform.localRotation = rot;
            }

            _inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out _isTracked);
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            SetModelVisible(_showModel);
        }

        #endregion

    }

}
