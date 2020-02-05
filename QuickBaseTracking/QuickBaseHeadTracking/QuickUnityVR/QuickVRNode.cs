using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickVRNode : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public enum Type
        {
            Head, 
            LeftEye,
            RightEye,

            LeftUpperArm, 
            LeftLowerArm, 
            LeftHand,

            RightUpperArm, 
            RightLowerArm,
            RightHand,

            Hips,

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

        protected static List<Type> _typeList = new List<Type>();

        protected static List<XRNodeState> _vrNodesStates = new List<XRNodeState>();

        protected Transform _model = null;

        protected Type _role = Type.Head;

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

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _trackedObject = transform.CreateChild("__TrackedObject__").gameObject.GetOrCreateComponent<QuickTrackedObject>();
        }

        protected virtual void LoadVRModel()
        {
            if (_model) DestroyImmediate(_model.gameObject);

            string modelName = XRDevice.model.ToLower();
            string pfName = "";
            if (_role == QuickVRNode.Type.Head) pfName = PF_GENERIC_HMD;
            else if (_role == QuickVRNode.Type.LeftHand)
            {
                if (modelName.Contains("vive")) pfName = PF_VIVE_CONTROLLER;
                else if (modelName.Contains("oculus")) pfName = PF_OCULUS_CV1_CONTROLLER_LEFT;
            }
            else if (_role == QuickVRNode.Type.RightHand)
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

        public virtual bool IsTracked()
        {
            return _isTracked;
        }

        public virtual void SetTracked(bool isTracked)
        {
            _isTracked = isTracked;
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

        public virtual Type GetRole()
        {
            return _role;
        }

        public virtual void SetRole(Type role, bool resetUpdateMode = true)
        {
            _role = role;
            name = role.ToString();
            _calibrationPose.name = CALIBRATION_POSE_PREFIX + name;

            LoadVRModel();

            if (resetUpdateMode) ResetUpdateMode();

            _trackedObject.Reset();

            Update();
        }

        protected virtual void ResetUpdateMode()
        {
            if (_role == Type.Head)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromUser;
            }
            else if (_role == Type.Hips)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromCalibrationPose;
            }
            else if (_role == Type.LeftHand || _role == Type.RightHand)
            {
                _updateModePos = UpdateMode.FromUser;
                _updateModeRot = UpdateMode.FromUser;
            }
            else if (_role == Type.LeftFoot || _role == Type.RightFoot)
            {
                _updateModePos = UpdateMode.FromCalibrationPose;
                _updateModeRot = UpdateMode.FromUser;
            }
        }

        public virtual QuickTrackedObject GetTrackedObject()
        {
            return _trackedObject;
        }

        public static List<Type> GetTypeList()
        {
            if (_typeList.Count == 0)
            {
                _typeList = QuickUtils.GetEnumValues<Type>();
            }
            return _typeList;
        }

        public virtual void Calibrate()
        {
            QuickUnityVRBase hTracking = QuickSingletonManager.GetInstance<QuickUnityVRBase>();
            _trackedObject.transform.ResetTransformation();

            if (_role == QuickVRNode.Type.Head)
            {
                _trackedObject.transform.localPosition = hTracking.GetHeadOffset();
            }
            else if (_role == QuickVRNode.Type.Hips)
            {
                QuickVRPlayArea vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
                QuickTrackedObject tObjectHead = vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject();
                _trackedObject.transform.position = new Vector3(tObjectHead.transform.position.x, _trackedObject.transform.position.y, tObjectHead.transform.position.z);
            }
            else if (_role == QuickVRNode.Type.LeftHand)
            {
                //if (IsExtraTracker(node.GetID()))
                //{
                //    //tObject.transform.Rotate(tObject.transform.right, 90.0f, Space.World);
                //    //tObject.transform.rotation = _vrNodesOrigin.rotation;
                //    //float d = Vector3.Dot(node.transform.forward, _vrNodesOrigin.up);
                //    //if (d < 0.5f)
                //    //{
                //    //    tObject.transform.Rotate(_vrNodesOrigin.right, 90.0f, Space.World);
                //    //    tObject.transform.Rotate(_vrNodesOrigin.up, nodeType == QuickVRNode.Type.LeftHand? -90.0f : 90.0f, Space.World);
                //    //}
                //}
                //else
                {
                    //This is a controller
                    //float sign = role == QuickVRNode.Type.LeftHand ? 1.0f : -1.0f;
                    //tObject.transform.Rotate(tObject.transform.forward, sign * 90.0f, Space.World);
                    //tObject.transform.localPosition = HAND_CONTROLLER_POSITION_OFFSET;

                    //tObject.transform.LookAt(tObject.transform.position + node.transform.right, -node.transform.up);
                }

                if (hTracking._handTrackingMode == QuickUnityVRBase.HandTrackingMode.Controllers)
                {
                    _trackedObject.transform.Rotate(_trackedObject.transform.forward, 90.0f, Space.World);
                    _trackedObject.transform.localPosition = QuickUnityVRBase.HAND_CONTROLLER_POSITION_OFFSET;
                }
                else
                {
                    _trackedObject.transform.LookAt(_trackedObject.transform.position + transform.right, -transform.up);
                }
            }
            else if (_role == QuickVRNode.Type.RightHand)
            {
                if (hTracking._handTrackingMode == QuickUnityVRBase.HandTrackingMode.Controllers)
                {
                    _trackedObject.transform.Rotate(_trackedObject.transform.forward, -90.0f, Space.World);
                    _trackedObject.transform.localPosition = QuickUnityVRBase.HAND_CONTROLLER_POSITION_OFFSET;
                }
                else
                {
                    _trackedObject.transform.LookAt(_trackedObject.transform.position - transform.right, transform.up);
                }
            }
            else if (_role == QuickVRNode.Type.LeftFoot || _role == QuickVRNode.Type.RightFoot)
            {
                //tObject.transform.rotation = _vrNodesOrigin.rotation;
                //tObject.transform.position += -_vrNodesOrigin.forward * 0.075f;
            }
            //else if (_role == QuickVRNode.Type.LeftLowerArm || _role == QuickVRNode.Type.RightLowerArm)
            //{
            //    tObject.transform.position += (-node.transform.forward * 0.1f);// + (-_vrNodesOrigin.up * 0.1f);
            //}

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

        #endregion

    }

}
