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
        };

        public bool _showModel = false;

        public enum UpdateMode
        {
            FromUser,
            FromCalibrationPose,
        }

        public UpdateMode _updateModePos = UpdateMode.FromUser;
        public UpdateMode _updateModeRot = UpdateMode.FromUser;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickTrackedObject _trackedObject = null;

        protected static List<HumanBodyBones> _typeList = new List<HumanBodyBones>();

        protected static List<XRNodeState> _vrNodesStates = new List<XRNodeState>();

        protected Transform _model = null;

        protected HumanBodyBones _role = HumanBodyBones.Head;

        protected Transform _calibrationPose = null;

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
            _typeList = QuickUtils.ParseEnum<HumanBodyBones, Type>(QuickUtils.GetEnumValues<Type>());
        }

        protected virtual void Awake()
        {
            _trackedObject = transform.CreateChild("__TrackedObject__").gameObject.GetOrCreateComponent<QuickTrackedObject>();
        }

        protected virtual void LoadVRModel()
        {
            if (_model) DestroyImmediate(_model.gameObject);

            string modelName = InputDevices.GetDeviceAtXRNode(XRNode.Head).name.ToLower();
            string pfName = "";
            if (_role == HumanBodyBones.Head) pfName = PF_GENERIC_HMD;
            else if (_role == HumanBodyBones.LeftHand)
            {
                if (modelName.Contains("vive")) pfName = PF_VIVE_CONTROLLER;
                else if (modelName.Contains("oculus")) pfName = PF_OCULUS_CV1_CONTROLLER_LEFT;
            }
            else if (_role == HumanBodyBones.RightHand)
            {
                if (modelName.Contains("vive")) pfName = PF_VIVE_CONTROLLER;
                else if (modelName.Contains("oculus")) pfName = PF_OCULUS_CV1_CONTROLLER_RIGHT;
            }
            else
            {
                pfName = PF_VIVE_TRACKER;
            }

            if (pfName.Length != 0)
            {
                _model = Instantiate<Transform>(Resources.Load<Transform>("Prefabs/" + pfName));
                _model.parent = transform;
                _model.ResetTransformation();
                _model.name = "Model";
            }
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
            if (!_model) return;

            _model.gameObject.SetActive(v);
        }

        public virtual HumanBodyBones GetRole()
        {
            return _role;
        }

        public virtual void SetRole(HumanBodyBones role, bool resetUpdateMode = true)
        {
            _role = role;
            name = ("VRNode" + role.ToString());
            _calibrationPose.name = CALIBRATION_POSE_PREFIX + name;

            LoadVRModel();

            if (resetUpdateMode) ResetUpdateMode();

            _trackedObject.Reset();

            Update();
        }

        protected virtual void ResetUpdateMode()
        {
            if (_role == HumanBodyBones.Head)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromUser;
            }
            else if (_role == HumanBodyBones.Hips)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromCalibrationPose;
            }
            else if (_role == HumanBodyBones.LeftHand || _role == HumanBodyBones.RightHand)
            {
                _updateModePos = UpdateMode.FromUser;
                _updateModeRot = UpdateMode.FromUser;
            }
            else if (_role == HumanBodyBones.LeftFoot || _role == HumanBodyBones.RightFoot)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromUser;
            }
        }

        public virtual QuickTrackedObject GetTrackedObject()
        {
            return _trackedObject;
        }

        public static List<HumanBodyBones> GetTypeList()
        {
            return _typeList;
        }

        public virtual void Calibrate()
        {
            _trackedObject.transform.ResetTransformation();

            if (_role == HumanBodyBones.Head)
            {
                if (OnCalibrateVRNodeHead != null) OnCalibrateVRNodeHead(this);
            }
            else if (_role == HumanBodyBones.Hips)
            {
                if (OnCalibrateVRNodeHips != null) OnCalibrateVRNodeHips(this);
            }
            else if (_role == HumanBodyBones.LeftHand)
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
            else if (_role == HumanBodyBones.RightHand)
            {
                if (OnCalibrateVRNodeRightHand != null) OnCalibrateVRNodeRightHand(this);
            }
            else if (_role == HumanBodyBones.LeftFoot)
            {
                if (OnCalibrateVRNodeLeftFoot != null) OnCalibrateVRNodeLeftFoot(this);
            }
            else if (_role == HumanBodyBones.RightFoot)
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

        protected virtual void Update()
        {
            //SetModelVisible(IsTracked() && Application.isEditor && _showModel);

            //SetModelVisible(Application.isEditor && _showModel);
            SetModelVisible(_showModel);
        }

        public virtual void Update(XRNodeState s)
        {
            Vector3 pos;
            Quaternion rot;
            if (s.TryGetPosition(out pos))
            {
                transform.localPosition = pos;
            }
            if (s.TryGetRotation(out rot))
            {
                transform.localRotation = rot;
            }

            _isTracked = s.tracked;
        }

        #endregion

    }

}
