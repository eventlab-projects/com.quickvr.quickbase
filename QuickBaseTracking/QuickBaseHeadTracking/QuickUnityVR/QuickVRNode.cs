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

            //TrackingReference,  //Represents a stationary physical device that can be used as a point of reference in the tracked area.
        };

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, ReadOnly] protected ulong _id = 0;

        protected QuickTrackedObject _trackedObject = null;

        protected static List<Type> _typeList = new List<Type>();

        protected static List<XRNodeState> _vrNodesStates = new List<XRNodeState>();

        protected Transform _model = null;

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
            LoadVRModel();
        }

        protected virtual void LoadVRModel()
        {
            Type nodeType = GetNodeType();
            string modelName = XRDevice.model.ToLower();
            string pfName = "";
            if (nodeType == QuickVRNode.Type.Head) pfName = PF_GENERIC_HMD;
            else if (nodeType == QuickVRNode.Type.LeftHand)
            {
                if (modelName.Contains("vive")) pfName = PF_VIVE_CONTROLLER;
                else if (modelName.Contains("oculus")) pfName = PF_OCULUS_CV1_CONTROLLER_LEFT;
            }
            else if (nodeType == QuickVRNode.Type.RightHand)
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
                Debug.Log("pfName = " + pfName);
                _model = Instantiate<Transform>(Resources.Load<Transform>("Prefabs/" + pfName));
                _model.parent = _trackedObject.transform;
                _model.ResetTransformation();
                _model.name = "Model";
            }
        }

        #endregion

        #region GET AND SET

        public virtual ulong GetID()
        {
            return _id;
        }

        public virtual void SetID(ulong id)
        {
            if (id == _id) return;

            _id = id;
            _trackedObject.Reset();

            Update();
        }

        protected virtual void SetModelVisible(bool v)
        {
            if (!_model) return;

            _model.gameObject.SetActive(v);
        }

        public static XRNodeState? GetUnityVRNodeState(XRNode nodeType)
        {
            InputTracking.GetNodeStates(_vrNodesStates);
            XRNodeState? result = null;

            for (int i = 0; i < _vrNodesStates.Count && !result.HasValue; i++)
            {
                if (_vrNodesStates[i].nodeType == nodeType) result = _vrNodesStates[i];
            }
            
            return result;
        }

        public static XRNodeState? GetUnityVRNodeState(ulong id)
        {
            InputTracking.GetNodeStates(_vrNodesStates);
            XRNodeState? result = null;

            for (int i = 0; i < _vrNodesStates.Count && !result.HasValue; i++)
            {
                if (_vrNodesStates[i].uniqueID == id) result = _vrNodesStates[i];
            }

            return result;
        }

        protected virtual XRNodeState? GetUnityVRNodeState()
        {
            return GetUnityVRNodeState(_id);
        }

        public virtual bool IsTracked()
        {
            return _id != 0;
        }

        public Type GetNodeType()
        {
            return QuickUtils.ParseEnum<Type>(name);
        }

        public virtual QuickTrackedObject GetTrackedObject()
        {
            return _trackedObject;
        }

        public static List<Type> GetTypeList()
        {
            if (_typeList.Count == 0) _typeList = QuickUtils.GetEnumValues<Type>();

            return _typeList;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            SetModelVisible(IsTracked());

            InputTracking.GetNodeStates(_vrNodesStates);

            if (IsTracked())
            {
                XRNodeState? uState = GetUnityVRNodeState();
                if (uState.HasValue)
                {
                    Vector3 pos;
                    Quaternion rot;
                    if (uState.Value.TryGetPosition(out pos))
                    {
                        transform.localPosition = pos;
                    }
                    if (uState.Value.TryGetRotation(out rot))
                    {
                        transform.localRotation = rot;
                    }
                }
            }
        }

        #endregion

    }

}
