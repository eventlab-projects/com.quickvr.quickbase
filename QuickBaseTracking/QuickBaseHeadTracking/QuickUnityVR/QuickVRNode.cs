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
            Undefined = -1,

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

            TrackingReference,  //Represents a stationary physical device that can be used as a point of reference in the tracked area.
        };

        public bool _showModel = true;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, ReadOnly]
        protected ulong _id = 0;

        protected QuickTrackedObject _trackedObject = null;

        protected static List<Type> _typeList = new List<Type>();

        protected static List<XRNodeState> _vrNodesStates = new List<XRNodeState>();

        protected Transform _model = null;

        protected Type _role = Type.Undefined;

        #endregion

        #region CONSTANTS

        protected static string PF_GENERIC_HMD = "pf_Generic_HMD";
        protected static string PF_VIVE_CONTROLLER = "pf_VIVE_Controller";
        protected static string PF_OCULUS_CV1_CONTROLLER_LEFT = "pf_OculusCV1_Controller_Left";
        protected static string PF_OCULUS_CV1_CONTROLLER_RIGHT = "pf_OculusCV1_Controller_Right";
        protected static string PF_VIVE_TRACKER = "pf_VIVE_Tracker";

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

        public virtual void SetID(ulong id)
        {
            _id = id;
        }

        public virtual ulong GetID()
        {
            return _id;
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

        public virtual void SetRole(Type role)
        {
            _role = role;
            name = role.ToString();

            LoadVRModel();

            _trackedObject.Reset();

            Update();
        }

        public virtual void SetRole(XRNode role)
        {
            string s = role.ToString();
            if (QuickUtils.IsEnumValue<Type>(s)) SetRole(QuickUtils.ParseEnum<Type>(s));
            else SetRole(Type.Undefined);
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
                _typeList.Remove(Type.Undefined);
                _typeList.Remove(Type.TrackingReference);
            }
            return _typeList;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            //SetModelVisible(IsTracked() && Application.isEditor && _showModel);

            SetModelVisible(Application.isEditor && _showModel);
        }

        #endregion

    }

}
