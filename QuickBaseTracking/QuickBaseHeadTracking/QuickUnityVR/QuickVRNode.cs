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
        };

        public enum State
        {
            DISCONNECTED,
            CONNECTED,
            TRACKED,
            STANDBY,
        };

        #endregion

        #region PROTECTED PARAMETERS

        protected ulong _id = 0;
        protected State _state = State.DISCONNECTED;

        protected QuickTrackedObject _trackedObject = null;

        protected static List<Type> _typeList = new List<Type>();

        protected List<XRNodeState> _vrNodesStates = new List<XRNodeState>();

        #endregion

        #region EVENTS

        public delegate void ConnectedAction();
        public event ConnectedAction OnConnected;

        public delegate void TrackedAction();
        public event TrackedAction OnTracked;

        public delegate void StandbyAction();
        public event StandbyAction OnStandby;

        public delegate void DisconnectedAction();
        public event DisconnectedAction OnDisconnected;

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
        }

        public virtual State GetState()
        {
            return _state;
        }

        protected virtual void SetState(State newState)
        {
            if (_state == newState) return;

            _state = newState;

            if (_state == State.CONNECTED)
            {
                Update();
                _trackedObject.Reset();
                if (OnConnected != null) OnConnected();
            }
            else if (_state == State.TRACKED)
            {
                if (OnTracked != null) OnTracked();
            }
            else if (_state == State.STANDBY)
            {
                if (OnStandby != null) OnStandby();
            }
            else if (_state == State.DISCONNECTED)
            {
                if (OnDisconnected != null) OnDisconnected();
            }
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
            return _state == State.CONNECTED || _state == State.TRACKED;
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
            UpdateState();

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

        protected virtual void UpdateState()
        {
            XRNodeState? uState = GetUnityVRNodeState();

            if (!uState.HasValue)
            {
                SetState(State.DISCONNECTED);
            }
            else if (_state == State.DISCONNECTED)
            {
                SetState(State.CONNECTED);
            }
            else if (_state == State.CONNECTED || _state == State.TRACKED)
            {
                SetState(uState.Value.tracked ? State.TRACKED : State.STANDBY);
            }
            else if (_state == State.STANDBY && uState.Value.tracked)
            {
                SetState(State.TRACKED);
            }
        }

        #endregion

    }

}
