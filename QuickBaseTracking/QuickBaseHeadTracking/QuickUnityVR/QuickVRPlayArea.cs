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
        protected List<XRNodeState> _vrNodeStates = new List<XRNodeState>();

        protected Transform _calibrationPoseRoot = null;
        
        protected bool _isHandsSwaped = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _calibrationPoseRoot = transform.CreateChild("__CalibrationPoseRoot__");
            foreach (QuickVRNode.Type role in QuickVRNode.GetTypeList())
            {
                CreateVRNode(role);
            }
        }

        protected virtual QuickVRNode CreateVRNode(QuickVRNode.Type role)
        {
            _vrNodeRoles[role] = transform.CreateChild("VRNode").gameObject.GetOrCreateComponent<QuickVRNode>();
            _vrNodeRoles[role].SetCalibrationPose(_calibrationPoseRoot.CreateChild(QuickVRNode.CALIBRATION_POSE_PREFIX, false));
            _vrNodeRoles[role].SetRole(role);

            return _vrNodeRoles[role];
        }

        #endregion

        #region GET AND SET

        public virtual Transform GetCalibrationPoseRoot()
        {
            return _calibrationPoseRoot;
        }

        public virtual QuickVRNode GetVRNodeMain()
        {
            QuickVRNode nodeHips = GetVRNode(QuickVRNode.Type.Hips);
            QuickVRNode nodeHead = GetVRNode(QuickVRNode.Type.Head);

            return nodeHips.IsTracked() ? nodeHips : nodeHead;
        }

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

        public virtual float GetEyeStereoSeparation()
        {
            QuickVRNode eLeft = GetVRNode(QuickVRNode.Type.LeftEye);
            QuickVRNode eRight = GetVRNode(QuickVRNode.Type.RightEye);
            return (eLeft != null && eRight != null) ? Vector3.Distance(eLeft.transform.position, eRight.transform.position) : 0.0f;
        }

        protected virtual XRNodeState GetLowerTracker(List<XRNodeState> nodeList)
        {
            XRNodeState result = nodeList[0];
            Vector3 minPos;
            result.TryGetPosition(out minPos);
            for (int i = 1; i < nodeList.Count; i++)
            {
                Vector3 tmpPos;
                nodeList[i].TryGetPosition(out tmpPos);
                if (tmpPos.y < minPos.y)
                {
                    result = nodeList[i];
                    minPos = tmpPos;
                }
            }

            return result;
        }

        protected virtual bool IsNodeLeftSide(QuickVRNode vrNode)
        {
            QuickVRNode nodeHead = GetVRNode(QuickVRNode.Type.Head);
            Vector3 fwd = Vector3.ProjectOnPlane(nodeHead.transform.forward, transform.up);
            Vector3 v = Vector3.ProjectOnPlane(vrNode.transform.position - nodeHead.transform.position, transform.up);

            return Vector3.SignedAngle(fwd, v, transform.up) < 0;
        }

        public virtual void Calibrate()
        {
            _vrNodes.Clear();
            List<XRNodeState> hardwareTrackers = new List<XRNodeState>();
            foreach (XRNodeState s in _vrNodeStates)
            {
                if (s.nodeType == XRNode.Head)
                {
                    _vrNodes[s.uniqueID] = GetVRNode(QuickVRNode.Type.Head);
                }
                else if (s.nodeType == XRNode.LeftHand)
                {
                    _vrNodes[s.uniqueID] = GetVRNode(QuickVRNode.Type.LeftHand);
                }
                else if (s.nodeType == XRNode.RightHand)
                {
                    _vrNodes[s.uniqueID] = GetVRNode(QuickVRNode.Type.RightHand);
                }
                else if (s.nodeType == XRNode.HardwareTracker)
                {
                    hardwareTrackers.Add(s);
                }
            }

            int numTrackers = hardwareTrackers.Count;
            Debug.Log("NUM HARDWARE TRACKERS = " + numTrackers);

            
            if (numTrackers == 1)
            {
                //This is the hipsTracker
                XRNodeState hTracker = hardwareTrackers[0];
                _vrNodes[hTracker.uniqueID] = GetVRNode(QuickVRNode.Type.Hips);
            }
            //else if (numTrackers == 3)
            //{
            //    //Hips + feet
            //    XRNodeState footTracker = GetLowerTracker(hardwareTrackers);
            //    QuickVRNode.Type footRole = IsNodeLeftSide(foot) ? QuickVRNode.Type.LeftFoot : QuickVRNode.Type.RightFoot;
            //    foot.SetRole(footRole);
            //    _vrNodeRoles[footRole] = foot;
            //    tmp.Remove(foot);

            //    foot = GetLowerTracker(tmp);
            //    footRole = IsNodeLeftSide(foot) ? QuickVRNode.Type.LeftFoot : QuickVRNode.Type.RightFoot;
            //    foot.SetRole(footRole);
            //    _vrNodeRoles[footRole] = foot;
            //    tmp.Remove(foot);

            //    tmp[0].SetRole(QuickVRNode.Type.Hips);
            //    _vrNodeRoles[QuickVRNode.Type.Hips] = tmp[0];
            //}

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

            float dLeft = nodeLeft.IsTracked() ? Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;
            float dRight = nodeRight.IsTracked() ? Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;

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
                if (n)
                {
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

                    n.SetTracked(s.tracked);
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

                DebugExtension.DrawCoordinatesSystem(n.transform.position, n.transform.right, n.transform.up, n.transform.forward, 0.1f);

                float s = 0.05f;
                Vector3 cSize = Vector3.one * s;
                if (role == QuickVRNode.Type.Head) Gizmos.color = Color.grey;
                else if (role == QuickVRNode.Type.LeftHand) Gizmos.color = Color.blue;
                else if (role == QuickVRNode.Type.RightHand) Gizmos.color = Color.red;

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


