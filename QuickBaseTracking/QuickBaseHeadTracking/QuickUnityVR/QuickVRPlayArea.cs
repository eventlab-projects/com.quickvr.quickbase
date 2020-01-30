using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{
    public class QuickVRPlayArea : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected Dictionary<ulong, QuickVRNode> _vrNodes = new Dictionary<ulong, QuickVRNode>();
        protected Dictionary<QuickVRNode.Type, QuickVRNode> _vrNodeRoles = new Dictionary<QuickVRNode.Type, QuickVRNode>();
        protected List<QuickVRNode> _vrHardwareTrackers = new List<QuickVRNode>();
        protected List<XRNodeState> _vrNodeStates = new List<XRNodeState>();

        protected Transform _calibrationPoseRoot = null;
        
        protected bool _isHandsSwaped = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _calibrationPoseRoot = transform.CreateChild("__CalibrationPoseRoot__");
        }

        protected virtual QuickVRNode CreateVRNode(ulong id, XRNode nodeType)
        {
            _vrNodes[id] = transform.CreateChild("VRNode").gameObject.GetOrCreateComponent<QuickVRNode>();
            _vrNodes[id].SetID(id);

            string s = nodeType.ToString();
            QuickVRNode.Type role = QuickUtils.IsEnumValue<QuickVRNode.Type>(s)? QuickUtils.ParseEnum<QuickVRNode.Type>(s) : QuickVRNode.Type.Undefined;
            _vrNodes[id].SetCalibrationPose(_calibrationPoseRoot.CreateChild(QuickVRNode.CALIBRATION_POSE_PREFIX, false));
            _vrNodes[id].SetRole(role);

            if (nodeType == XRNode.HardwareTracker)
            {
                _vrHardwareTrackers.Add(_vrNodes[id]);
            }
            else
            {
                _vrNodeRoles[role] = _vrNodes[id];
            }

            return _vrNodes[id];
        }

        #endregion

        #region GET AND SET

        public virtual QuickVRNode GetVRNode(ulong id)
        {
            return _vrNodes.ContainsKey(id) ? _vrNodes[id] : null;
        }

        public virtual QuickVRNode GetVRNode(QuickVRNode.Type role)
        {
            return _vrNodeRoles.ContainsKey(role)? _vrNodeRoles[role] : null;
        }

        public virtual QuickVRNode GetVRNode(HumanBodyBones boneID)
        {
            return GetVRNode(QuickUtils.ParseEnum<QuickVRNode.Type>(boneID.ToString()));
        }

        protected virtual bool IsValidNode(XRNode n)
        {
            return 
                (
                n == XRNode.HardwareTracker || 
                n == XRNode.Head || 
                n == XRNode.LeftEye || 
                n == XRNode.RightEye || 
                n == XRNode.LeftHand || 
                n == XRNode.RightHand //||
                //n == XRNode.TrackingReference
                );
        }

        public virtual bool IsTrackedNode(QuickVRNode.Type role)
        {
            QuickVRNode node = GetVRNode(role);
            if (node)
            {
                foreach (XRNodeState s in _vrNodeStates)
                {
                    if (s.uniqueID == node.GetID()) return s.tracked;
                }
            }

            return false;
        }

        public virtual bool IsTrackedNode(QuickVRNode vrNode)
        {
            return vrNode ? IsTrackedNode(vrNode.GetRole()) : false;
        }

        public virtual float GetEyeStereoSeparation()
        {
            QuickVRNode eLeft = GetVRNode(QuickVRNode.Type.LeftEye);
            QuickVRNode eRight = GetVRNode(QuickVRNode.Type.RightEye);
            return (eLeft != null && eRight != null) ? Vector3.Distance(eLeft.transform.position, eRight.transform.position) : 0.0f;
        }

        public virtual void Calibrate()
        {
            Debug.Log("NUM HARDWARE TRACKERS = " + _vrHardwareTrackers.Count);
            if (_vrHardwareTrackers.Count == 1)
            {
                //This is the hipsTracker
                _vrHardwareTrackers[0].SetRole(QuickVRNode.Type.Hips);
                _vrNodeRoles[QuickVRNode.Type.Hips] = _vrHardwareTrackers[0];
            }

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                QuickVRNode n = GetVRNode(t);
                if (n) n.Calibrate(); 
            }
            _isHandsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
            Debug.Log("handsSwaped = " + _isHandsSwaped);
        }

        public virtual bool IsVRNodesSwaped(QuickVRNode.Type typeNodeLeft, QuickVRNode.Type typeNodeRight, bool doSwaping = true)
        {
            return IsVRNodesSwaped(GetVRNode(typeNodeLeft), GetVRNode(typeNodeRight), doSwaping);
        }

        public virtual bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        {
            QuickVRNode hmdNode = GetVRNode(QuickVRNode.Type.Head);
            if (!hmdNode || !nodeLeft || !nodeRight) return false;

            float dLeft = IsTrackedNode(nodeLeft) ? Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;
            float dRight = IsTrackedNode(nodeRight) ? Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;

            bool swaped = dLeft > dRight;
            if (swaped && doSwaping)
            {
                SwapQuickVRNode(nodeLeft, nodeRight);
            }

            return swaped;
        }

        protected virtual void SwapQuickVRNode(QuickVRNode.Type typeA, QuickVRNode.Type typeB)
        {
            SwapQuickVRNode(GetVRNode(typeA), GetVRNode(typeB));
        }

        protected virtual void SwapQuickVRNode(QuickVRNode vrNodeA, QuickVRNode vrNodeB)
        {
            QuickVRNode.Type tmp = vrNodeA.GetRole();
            vrNodeA.SetRole(vrNodeB.GetRole());
            vrNodeB.SetRole(tmp);
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            InputTracking.GetNodeStates(_vrNodeStates);
            foreach (XRNodeState s in _vrNodeStates)
            {
                if (!IsValidNode(s.nodeType)) continue;

                QuickVRNode n = GetVRNode(s.uniqueID);
                if (!n)
                {
                    n = CreateVRNode(s.uniqueID, s.nodeType);
                }

                Vector3 pos;
                Quaternion rot;
                if (s.TryGetPosition(out pos))
                {
                    n.transform.localPosition = pos;
                }
                if (s.TryGetRotation(out rot))
                {
                    n.transform.localRotation = rot;
                }
            }
            
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            foreach (var pair in _vrNodes)
            {
                QuickVRNode n = pair.Value;
                QuickVRNode.Type role = n.GetRole();
                if (n.GetRole() == QuickVRNode.Type.Undefined) continue;

                DebugExtension.DrawCoordinatesSystem(n.transform.position, n.transform.right, n.transform.up, n.transform.forward, 0.1f);

                float s = 0.05f;
                Vector3 cSize = Vector3.one * s;
                if (role == QuickVRNode.Type.Head) Gizmos.color = Color.grey;
                else if (role == QuickVRNode.Type.LeftHand) Gizmos.color = Color.blue;
                else if (role == QuickVRNode.Type.RightHand) Gizmos.color = Color.red;
                else if (role == QuickVRNode.Type.TrackingReference) Gizmos.color = Color.magenta;

                Gizmos.matrix = n.transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, cSize);
                QuickTrackedObject tObject = n.GetTrackedObject();
                if (tObject.transform.localPosition != Vector3.zero)
                {
                    Gizmos.DrawSphere(tObject.transform.localPosition, s * 0.5f);
                }
                Gizmos.matrix = Matrix4x4.identity;
            }
        }

        #endregion

    }
}


