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

        protected virtual QuickVRNode GetVRNode(ulong id)
        {
            return _vrNodes.ContainsKey(id) ? _vrNodes[id] : null;
        }

        public virtual QuickVRNode GetVRNode(QuickVRNode.Type role)
        {
            return _vrNodeRoles[role];
        }

        public virtual QuickVRNode GetVRNode(HumanBodyBones boneID)
        {
            return GetVRNode(QuickUtils.ParseEnum<QuickVRNode.Type>(boneID.ToString()));
        }

        public virtual float GetEyeStereoSeparation()
        {
            QuickVRNode eLeft = GetVRNode(QuickVRNode.Type.LeftEye);
            QuickVRNode eRight = GetVRNode(QuickVRNode.Type.RightEye);
            return (eLeft != null && eRight != null) ? Vector3.Distance(eLeft.transform.position, eRight.transform.position) : 0.0f;
        }

        protected virtual bool IsNodeLeftSide(QuickVRNode vrNode)
        {
            QuickVRNode nodeHead = GetVRNode(QuickVRNode.Type.Head);
            Vector3 fwd = Vector3.ProjectOnPlane(nodeHead.transform.forward, transform.up);
            Vector3 v = Vector3.ProjectOnPlane(vrNode.transform.position - nodeHead.transform.position, transform.up);

            return Vector3.SignedAngle(fwd, v, transform.up) < 0;
        }

        protected virtual List<XRNodeState> GetBodyTrackers()
        {
            List<XRNodeState> result = new List<XRNodeState>();
            foreach (XRNodeState s in _vrNodeStates)
            {
                if
                    (
                        s.nodeType == XRNode.Head ||
                        s.nodeType == XRNode.LeftHand ||
                        s.nodeType == XRNode.RightHand ||
                        s.nodeType == XRNode.HardwareTracker
                    )
                {
                    result.Add(s);
                }
            }

            result.Sort(CompareXRNodeStates);

            return result;
        }

        public virtual void Calibrate()
        {
            _vrNodes.Clear();
            List<XRNodeState> bodyTrackers = GetBodyTrackers();
            
            //POSSIBLE TRACKER CONFIGURATIONS

            //1     ->  Head
            //3     ->  Head + Hands
            //4     ->  Head + Hands + Hips
            //6     ->  Head + Hands + Hips + Feet
            //10    ->  Head + Hands + Hips + Feet + Elbows + Knees

            int numTrackers = bodyTrackers.Count;
            Debug.Log("NUM BODY TRACKERS = " + numTrackers);
            
            if (numTrackers == 1 || numTrackers == 3 || numTrackers == 4 || numTrackers == 6 || numTrackers == 10)
            {
                //The head will always be the upper body tracker
                InitVRNode(QuickVRNode.Type.Head, bodyTrackers[0]);

                if (numTrackers == 3)
                {
                    //Head + Hands
                    InitVRNode(QuickVRNode.Type.LeftHand, bodyTrackers[1]);
                    InitVRNode(QuickVRNode.Type.RightHand, bodyTrackers[2]);
                }
                else if (numTrackers == 4)
                {
                    //Head + Hands + Hips
                    //1) Remove the head node from the list
                    bodyTrackers.RemoveAt(0);

                    //2) The hips is the node that is "in the middle", i.e., the hands are in opposite sides of the hips node. 
                    InitHipsAndHands(bodyTrackers);
                }
                else if (numTrackers == 6)
                {
                    //Head + Hands + Hips + Feet
                    //1) The Feet are the trackers with the lower y
                    InitVRNode(QuickVRNode.Type.LeftFoot, bodyTrackers[5]);
                    InitVRNode(QuickVRNode.Type.RightFoot, bodyTrackers[4]);

                    //2) Remove the unnecessary nodes and proceed as in the previous case
                    bodyTrackers.RemoveAt(5);
                    bodyTrackers.RemoveAt(4);
                    bodyTrackers.RemoveAt(0);

                    InitHipsAndHands(bodyTrackers);
                }

                Update();

                IsVRNodesSwaped(QuickVRNode.Type.LeftFoot, QuickVRNode.Type.RightFoot);
                _isHandsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);

                Debug.Log("handsSwaped = " + _isHandsSwaped);

                Update();
            }
            else
            {
                Debug.LogError("BAD NUMBER OF BODY TRACKERS!!!");
            }

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                QuickVRNode n = GetVRNode(t);
                if (n) n.Calibrate();
            }
        }

        protected virtual void InitHipsAndHands(List<XRNodeState> bodyTrackers)
        {
            if (bodyTrackers.Count != 3)
            {
                Debug.LogError("BODY TRACKERS LIST MUST CONTAIN EXACTLY 3 ELEMENTS");
                return;
            }

            Vector3 pos0;
            Vector3 pos1;
            Vector3 pos2;
            bodyTrackers[0].TryGetPosition(out pos0);
            bodyTrackers[1].TryGetPosition(out pos1);
            bodyTrackers[2].TryGetPosition(out pos2);

            Vector3 scale = new Vector3(1, 0, 1);
            pos0 = Vector3.Scale(pos0, scale);
            pos1 = Vector3.Scale(pos1, scale);
            pos2 = Vector3.Scale(pos2, scale);

            if (Vector3.Dot(pos1 - pos0, pos2 - pos0) < 0)
            {
                //0 is the hips tracker
                InitVRNode(QuickVRNode.Type.Hips, bodyTrackers[0]);
                InitVRNode(QuickVRNode.Type.LeftHand, bodyTrackers[1]);
                InitVRNode(QuickVRNode.Type.RightHand, bodyTrackers[2]);
            }
            else if (Vector3.Dot(pos0 - pos1, pos2 - pos1) < 0)
            {
                //1 is the hips tracker
                InitVRNode(QuickVRNode.Type.Hips, bodyTrackers[1]);
                InitVRNode(QuickVRNode.Type.LeftHand, bodyTrackers[0]);
                InitVRNode(QuickVRNode.Type.RightHand, bodyTrackers[2]);
            }
            else
            {
                //2 is the hips tracker
                InitVRNode(QuickVRNode.Type.Hips, bodyTrackers[2]);
                InitVRNode(QuickVRNode.Type.LeftHand, bodyTrackers[0]);
                InitVRNode(QuickVRNode.Type.RightHand, bodyTrackers[1]);
            }
        }

        protected virtual void InitVRNode(QuickVRNode.Type nodeType, XRNodeState initialState)
        {
            QuickVRNode node = GetVRNode(nodeType);
            _vrNodes[initialState.uniqueID] = node;
            node.Update(initialState);
        }

        public virtual bool IsVRNodesSwaped(QuickVRNode.Type typeNodeLeft, QuickVRNode.Type typeNodeRight, bool doSwaping = true)
        {
            return IsVRNodesSwaped(GetVRNode(typeNodeLeft), GetVRNode(typeNodeRight), doSwaping);
        }

        public virtual bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        {
            QuickVRNode hmdNode = GetVRNode(QuickVRNode.Type.Head);
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
            ulong idNodeA = 0;
            ulong idNodeB = 0;
            foreach (var pair in _vrNodes)
            {
                if (pair.Value == vrNodeA) idNodeA = pair.Key;
                else if (pair.Value == vrNodeB) idNodeB = pair.Key;
            }

            if (idNodeA != 0 && idNodeB != 0)
            {
                _vrNodes[idNodeA] = vrNodeB;
                _vrNodes[idNodeB] = vrNodeA;
            }
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            InputTracking.GetNodeStates(_vrNodeStates);
            foreach (XRNodeState s in _vrNodeStates)
            {
                bool isEssentialNode = s.nodeType == XRNode.Head || s.nodeType == XRNode.LeftHand || s.nodeType == XRNode.RightHand;
                if (isEssentialNode && !_vrNodes.ContainsKey(s.uniqueID))
                {
                    _vrNodes[s.uniqueID] = GetVRNode(QuickUtils.ParseEnum<QuickVRNode.Type>(s.nodeType.ToString()));
                }
                QuickVRNode n = GetVRNode(s.uniqueID);
                if (n)
                {
                    n.Update(s);
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

        #region HELPERS

        private static int CompareXRNodeStates(XRNodeState stateA, XRNodeState stateB)
        {
            Vector3 posA;
            Vector3 posB;
            stateA.TryGetPosition(out posA);
            stateB.TryGetPosition(out posB);

            return -posA.y.CompareTo(posB.y);
        }

        #endregion

    }
}


