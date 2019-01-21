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
            LeftHand, 
            RightHand,
            
            Waist,
            LeftFoot, 
            RightFoot,

            //TrackingReference,  //Represents a stationary physical device that can be used as a point of reference in the tracked area.
        };

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField] protected ulong _id = 0;

        protected QuickTrackedObject _trackedObject = null;

        protected static List<Type> _typeList = new List<Type>();

        protected List<XRNodeState> _vrNodesStates = new List<XRNodeState>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _trackedObject = transform.CreateChild("__TrackedObject__").gameObject.GetOrCreateComponent<QuickTrackedObject>();
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

        protected virtual XRNodeState? GetUnityVRNodeState()
        {
            XRNodeState? result = null;

            foreach (XRNodeState s in _vrNodesStates)
            {
                if (s.uniqueID == _id)
                {
                    result = s;
                    break;
                }
            }
            
            return result;
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
            InputTracking.GetNodeStates(_vrNodesStates);

            if (IsTracked())
            {
                XRNodeState? uState = GetUnityVRNodeState();
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

        #endregion

    }

}
