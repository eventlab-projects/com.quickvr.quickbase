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

        protected static string PF_GENERIC_HMD = "pf_Generic_HMD";
        protected static string PF_VIVE_CONTROLLER = "pf_VIVE_Controller";
        protected static string PF_OCULUS_CV1_CONTROLLER_LEFT = "pf_OculusCV1_Controller_Left";
        protected static string PF_OCULUS_CV1_CONTROLLER_RIGHT = "pf_OculusCV1_Controller_Right";
        protected static string PF_VIVE_TRACKER = "pf_VIVE_Tracker";

        public static string CALIBRATION_POSE_PREFIX = "__CalibrationPose__";

        #endregion

        #region EVENTS

        public delegate void CalibrateVRNodeAction(QuickVRNode vrNode);
        public static event CalibrateVRNodeAction OnCalibrateVRNodeHead;
        public static event CalibrateVRNodeAction OnCalibrateVRNodeHips;
        public static event CalibrateVRNodeAction OnCalibrateVRNodeLeftHand;
        public static event CalibrateVRNodeAction OnCalibrateVRNodeRightHand;
        public static event CalibrateVRNodeAction OnCalibrateVRNodeLeftFoot;
        public static event CalibrateVRNodeAction OnCalibrateVRNodeRightFoot;

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

        protected virtual void LoadVRModel()
        {
            if (_model) DestroyImmediate(_model.gameObject);

            string modelName = QuickVRManager.GetHMDName();
            string pfName = "";

            if (_role == QuickHumanBodyBones.Head) pfName = PF_GENERIC_HMD;
            else if (_role == QuickHumanBodyBones.LeftHand)
            {
                if (modelName.Contains("vive")) pfName = PF_VIVE_CONTROLLER;
                else if (modelName.Contains("oculus")) pfName = PF_OCULUS_CV1_CONTROLLER_LEFT;
            }
            else if (_role == QuickHumanBodyBones.RightHand)
            {
                if (modelName.Contains("vive")) pfName = PF_VIVE_CONTROLLER;
                else if (modelName.Contains("oculus")) pfName = PF_OCULUS_CV1_CONTROLLER_RIGHT;
            }
            else
            {
                pfName = PF_VIVE_TRACKER;
            }

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
            name = ("VRNode" + role.ToString());
            _calibrationPose.name = CALIBRATION_POSE_PREFIX + name;

            LoadVRModel();

            if (resetUpdateMode) ResetUpdateMode();

            _trackedObject.Reset();
        }

        protected virtual void ResetUpdateMode()
        {
            if (_role == QuickHumanBodyBones.Head)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromUser;
            }
            else if (_role == QuickHumanBodyBones.Hips)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromCalibrationPose;
            }
            else if (_role == QuickHumanBodyBones.LeftHand || _role == QuickHumanBodyBones.RightHand)
            {
                _updateModePos = UpdateMode.FromUser;
                _updateModeRot = UpdateMode.FromUser;
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

            if (_role == QuickHumanBodyBones.Head)
            {
                if (OnCalibrateVRNodeHead != null) OnCalibrateVRNodeHead(this);
            }
            else if (_role == QuickHumanBodyBones.Hips)
            {
                if (OnCalibrateVRNodeHips != null) OnCalibrateVRNodeHips(this);
            }
            else if (_role == QuickHumanBodyBones.LeftHand)
            {
                if (OnCalibrateVRNodeLeftHand != null) OnCalibrateVRNodeLeftHand(this);
                //if (IsExtraTracker(node.GetID()))
                //{
                //    //tObject.transform.Rotate(tObject.transform.right, 90.0f, Space.World);
                //    //tObject.transform.rotation = _vrNodesOrigin.rotation;
                //    //float d = Vector3.Dot(node.transform.forward, _vrNodesOrigin.up);
                //    //if (d < 0.5f)
                //    //{
                //    //    tObject.transform.Rotate(_vrNodesOrigin.right, 90.0f, Space.World);
                //    //    tObject.transform.Rotate(_vrNodesOrigin.up, nodeType == Type.LeftHand? -90.0f : 90.0f, Space.World);
                //    //}
                //}
                //else
                {
                    //This is a controller
                    //float sign = role == Type.LeftHand ? 1.0f : -1.0f;
                    //tObject.transform.Rotate(tObject.transform.forward, sign * 90.0f, Space.World);
                    //tObject.transform.localPosition = HAND_CONTROLLER_POSITION_OFFSET;

                    //tObject.transform.LookAt(tObject.transform.position + node.transform.right, -node.transform.up);
                }
            }
            else if (_role == QuickHumanBodyBones.RightHand)
            {
                if (OnCalibrateVRNodeRightHand != null) OnCalibrateVRNodeRightHand(this);
            }
            else if (_role == QuickHumanBodyBones.LeftFoot)
            {
                if (OnCalibrateVRNodeLeftFoot != null) OnCalibrateVRNodeLeftFoot(this);
            }
            else if (_role == QuickHumanBodyBones.RightFoot)
            {
                if (OnCalibrateVRNodeRightFoot != null) OnCalibrateVRNodeRightFoot(this);
            }
            
            //Save the calibration pose
            _calibrationPose.position = _trackedObject.transform.position;
            _calibrationPose.rotation = _trackedObject.transform.rotation;
            _trackedObject.Reset();
        }

        #endregion

        #region UPDATE

        public virtual void UpdateState()
        {
            //try to find a valid inputdevice for the key roles
            if (!_inputDevice.isValid)
            {
                if (_role == QuickHumanBodyBones.Head)
                {
                    _inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                }
                else if (_role == QuickHumanBodyBones.LeftHand)
                {
                    _inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                }
                else if (_role == QuickHumanBodyBones.RightHand)
                {
                    _inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                }
                else if (_role == QuickHumanBodyBones.LeftEye || _role == QuickHumanBodyBones.RightEye)
                {
                    List<InputDevice> devices = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.HeadMounted, devices);

                    if (devices.Count > 0)
                    {
                        _inputDevice = devices[0];
                    }
                }
            }
            
            if (_inputDevice.isValid)
            {
                if (_inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                {
                    transform.localPosition = pos;
                }
                if (_inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                {
                    transform.localRotation = rot;
                }

                if (_role == QuickHumanBodyBones.LeftEye || _role == QuickHumanBodyBones.RightEye)
                {
                    //bool isLeft = _role == QuickHumanBodyBones.LeftEye;
                    //if (_inputDevice.TryGetFeatureValue(isLeft? QuickVRUsages.leftEyeVector : QuickVRUsages.rightEyeVector, out Vector3 vEye))
                    if (_inputDevice.TryGetFeatureValue(QuickVRUsages.combineEyeVector, out Vector3 vEye))
                    {
                        //Transform target = QuickSingletonManager.GetInstance<QuickVRPlayArea>().GetVRNode(HumanBodyBones.Head).transform;

                        Vector3 r = transform.TransformDirection(vEye);
                        Vector3 rotAxis = Vector3.Cross(transform.forward, r);
                        float rotAngle = Vector3.Angle(transform.forward, r);
                        transform.Rotate(rotAxis, rotAngle, Space.World);
                    }
                }

                _inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out _isTracked);
            }
            else
            {
                _isTracked = false;
            }
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
