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

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual QuickVRNode CreateVRNode(ulong id)
        {
            //Create the VRNode if necessary
            if (!_vrNodes.ContainsKey(id))
            {
                _vrNodes[id] = transform.CreateChild("VRNode").gameObject.GetOrCreateComponent<QuickVRNode>(); 
            }

            return _vrNodes[id];
        }

        #endregion

        #region GET AND SET

        public virtual QuickVRNode GetVRNode(ulong id)
        {
            return _vrNodes.ContainsKey(id) ? _vrNodes[id] : null;
        }

        public virtual QuickVRNode GetVRNode(QuickVRNode.Type nodeType)
        {
            QuickVRNode result = null;
            foreach (var pair in _vrNodes)
            {
                if (pair.Value.GetRole() == nodeType)
                {
                    result = pair.Value;
                    break;
                }
            }

            return result;
        }

        protected virtual bool IsValidNode(XRNode n)
        {
            return (n == XRNode.HardwareTracker || n == XRNode.Head || n == XRNode.LeftHand || n == XRNode.RightHand || n == XRNode.TrackingReference);
        }

        protected virtual void SetVRNodeRole(ulong id, XRNode n)
        {
            QuickVRNode vrNode = GetVRNode(id);
            if (vrNode)
            {
                vrNode.SetRole(n);
            }
        }
        
        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            List<XRNodeState> nodeStates = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodeStates);
            foreach (XRNodeState s in nodeStates)
            {
                if (!IsValidNode(s.nodeType)) continue;

                QuickVRNode n = GetVRNode(s.uniqueID);
                if (!n)
                {
                    n = CreateVRNode(s.uniqueID);
                    SetVRNodeRole(s.uniqueID, s.nodeType);
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

            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("=========================");
                Debug.Log("NODES INFO");
                Debug.Log("=========================");

                foreach (XRNodeState s in nodeStates)
                {
                    Debug.Log("nodeType = " + s.nodeType);
                    Debug.Log("nodeID = " + s.uniqueID);
                    Debug.Log("tracked = " + s.tracked);
                }

                Debug.Log("=========================");
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

                Vector3 cSize = Vector3.one * 0.05f;
                if (role == QuickVRNode.Type.Head) Gizmos.color = Color.grey;
                else if (role == QuickVRNode.Type.LeftHand) Gizmos.color = Color.blue;
                else if (role == QuickVRNode.Type.RightHand) Gizmos.color = Color.red;
                else if (role == QuickVRNode.Type.TrackingReference) Gizmos.color = Color.magenta;

                Gizmos.DrawCube(n.transform.position, cSize);
                DebugExtension.DrawCoordinatesSystem(n.transform.position, n.transform.right, n.transform.up, n.transform.forward, 0.1f);

            }
        }

        #endregion

    }
}


