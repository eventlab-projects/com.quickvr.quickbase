using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{
    public class QuickVRPlayArea : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected static Dictionary<XRNode, HumanBodyBones> _essentialNodes = new Dictionary<XRNode, HumanBodyBones>();
        protected Dictionary<ulong, QuickVRNode> _vrNodes = new Dictionary<ulong, QuickVRNode>();
        protected Dictionary<QuickHumanBodyBones, QuickVRNode> _vrNodeRoles = new Dictionary<QuickHumanBodyBones, QuickVRNode>();
        protected List<XRNodeState> _vrNodeStates = new List<XRNodeState>();

        protected Transform _calibrationPoseRoot = null;
        
        protected bool _isHandsSwaped = false;

        protected Vector3 _customUserForward = Vector3.zero;  //A custom user forward provided by the application. 

        protected XRController _controllerHandLeft = null;
        protected XRController _controllerHandRight = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            XRNode[] eNodes = { XRNode.Head, XRNode.LeftHand, XRNode.RightHand, XRNode.LeftEye, XRNode.RightEye };
            foreach (XRNode n in eNodes)
            {
                _essentialNodes[n] = QuickUtils.ParseEnum<HumanBodyBones>(n.ToString());
            }
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnTargetAnimatorSet += ActionTargetAnimatorSet;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnTargetAnimatorSet += ActionTargetAnimatorSet;
        }

        protected virtual void Awake()
        {
            _calibrationPoseRoot = transform.CreateChild("__CalibrationPoseRoot__");
            foreach (QuickHumanBodyBones role in QuickVRNode.GetTypeList())
            {
                CreateVRNode(role);
            }

            _controllerHandLeft = CreateHandController(XRNode.LeftHand);
            _controllerHandRight = CreateHandController(XRNode.RightHand);
        }

        protected virtual XRController CreateHandController(XRNode controllerNode)
        {
            Transform tController = transform.CreateChild("__Controller" + controllerNode.ToString());
            XRController controller = tController.gameObject.AddComponent<XRController>();
            controller.enableInputTracking = false;
            controller.controllerNode = controllerNode;

            //Add the components to be able to catch close objects. 
            SphereCollider collider = controller.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.2f;
            controller.gameObject.AddComponent<XRDirectInteractor>();
            
            return controller;
        }

        protected virtual QuickVRNode CreateVRNode(QuickHumanBodyBones role)
        {
            _vrNodeRoles[role] = transform.CreateChild("VRNode").gameObject.GetOrCreateComponent<QuickVRNode>();
            _vrNodeRoles[role].SetCalibrationPose(_calibrationPoseRoot.CreateChild(QuickVRNode.CALIBRATION_POSE_PREFIX, false));
            _vrNodeRoles[role].SetRole(role);

            return _vrNodeRoles[role];
        }

        #endregion

        #region GET AND SET

        protected virtual void ActionTargetAnimatorSet()
        {
            Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
            _controllerHandLeft.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _controllerHandLeft.transform.ResetTransformation();

            _controllerHandRight.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _controllerHandRight.transform.ResetTransformation();
        }

        public virtual Vector3 GetUserForward()
        {
            if (_customUserForward == Vector3.zero)
            {
                return Vector3.ProjectOnPlane(GetVRNodeMain().transform.forward, transform.up);
            }
            return _customUserForward;
        }

        public virtual void SetUserForward(Vector3 fwd)
        {
            _customUserForward = fwd;
        }

        public virtual void ResetUserForward()
        {
            _customUserForward = Vector3.zero;
        }

        public virtual Transform GetCalibrationPoseRoot()
        {
            return _calibrationPoseRoot;
        }

        public virtual QuickVRNode GetVRNodeMain()
        {
            QuickVRNode nodeHips = GetVRNode(HumanBodyBones.Hips);
            QuickVRNode nodeHead = GetVRNode(HumanBodyBones.Head);

            return nodeHips.IsTracked() ? nodeHips : nodeHead;
        }

        protected virtual QuickVRNode GetVRNode(ulong id)
        {
            return _vrNodes.ContainsKey(id) ? _vrNodes[id] : null;
        }

        public virtual QuickVRNode GetVRNode(QuickHumanBodyBones role)
        {
            return _vrNodeRoles[role];
        }

        public virtual QuickVRNode GetVRNode(HumanBodyBones role)
        {
            return GetVRNode((QuickHumanBodyBones)role);
        }

        public virtual float GetEyeStereoSeparation()
        {
            QuickVRNode eLeft = GetVRNode(HumanBodyBones.LeftEye);
            QuickVRNode eRight = GetVRNode(HumanBodyBones.RightEye);
            return (eLeft != null && eRight != null) ? Vector3.Distance(eLeft.transform.position, eRight.transform.position) : 0.0f;
        }

        protected virtual bool IsNodeLeftSide(QuickVRNode vrNode)
        {
            QuickVRNode nodeHead = GetVRNode(HumanBodyBones.Head);
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

            //InputDevice dHead = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            //Debug.Log("dHead = " + dHead.name);
            ////Vector3 pos;
            ////if (dHead.TryGetFeatureValue(CommonUsages.devicePosition, out pos))
            ////{
            ////    Debug.Log("pos = " + pos.ToString("f3"));
            ////}

            ////InputDevice dLeftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            ////Debug.Log("dLeftHand = " + dLeftHand.name);

            ////InputDevice dRightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            ////Debug.Log("dRightHand = " + dRightHand.name);

            if (numTrackers == 1 || numTrackers == 3 || numTrackers == 4 || numTrackers == 6 || numTrackers == 10)
            {
                //The head will always be the upper body tracker
                InitVRNode(HumanBodyBones.Head, bodyTrackers[0]);

                if (numTrackers == 3)
                {
                    //Head + Hands
                    InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[1]);
                    InitVRNode(HumanBodyBones.RightHand, bodyTrackers[2]);
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
                    InitVRNode(HumanBodyBones.LeftFoot, bodyTrackers[5]);
                    InitVRNode(HumanBodyBones.RightFoot, bodyTrackers[4]);

                    //2) Remove the unnecessary nodes and proceed as in the previous case
                    bodyTrackers.RemoveAt(5);
                    bodyTrackers.RemoveAt(4);
                    bodyTrackers.RemoveAt(0);

                    InitHipsAndHands(bodyTrackers);
                }

                UpdateVRNodes();

                IsVRNodesSwaped(HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot);
                _isHandsSwaped = IsVRNodesSwaped(HumanBodyBones.LeftHand, HumanBodyBones.RightHand);

                Debug.Log("handsSwaped = " + _isHandsSwaped);

                UpdateVRNodes();
            }
            else
            {
                Debug.LogWarning("BAD NUMBER OF BODY TRACKERS!!!");
            }

            foreach (HumanBodyBones t in QuickVRNode.GetTypeList())
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
                InitVRNode(HumanBodyBones.Hips, bodyTrackers[0]);
                InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[1]);
                InitVRNode(HumanBodyBones.RightHand, bodyTrackers[2]);
            }
            else if (Vector3.Dot(pos0 - pos1, pos2 - pos1) < 0)
            {
                //1 is the hips tracker
                InitVRNode(HumanBodyBones.Hips, bodyTrackers[1]);
                InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[0]);
                InitVRNode(HumanBodyBones.RightHand, bodyTrackers[2]);
            }
            else
            {
                //2 is the hips tracker
                InitVRNode(HumanBodyBones.Hips, bodyTrackers[2]);
                InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[0]);
                InitVRNode(HumanBodyBones.RightHand, bodyTrackers[1]);
            }
        }

        protected virtual void InitVRNode(HumanBodyBones nodeType, XRNodeState initialState)
        {
            QuickVRNode node = GetVRNode(nodeType);
            _vrNodes[initialState.uniqueID] = node;
            node.UpdateState(initialState);
        }

        public virtual bool IsVRNodesSwaped(HumanBodyBones typeNodeLeft, HumanBodyBones typeNodeRight, bool doSwaping = true)
        {
            return IsVRNodesSwaped(GetVRNode(typeNodeLeft), GetVRNode(typeNodeRight), doSwaping);
        }

        public virtual bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        {
            QuickVRNode hmdNode = GetVRNode(HumanBodyBones.Head);
            float dLeft = nodeLeft.IsTracked() ? Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;
            float dRight = nodeRight.IsTracked() ? Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;

            bool swaped = dLeft > dRight;
            if (swaped && doSwaping)
            {
                SwapQuickVRNode(nodeLeft, nodeRight);
            }

            return swaped;
        }

        protected virtual void SwapQuickVRNode(HumanBodyBones typeA, HumanBodyBones typeB)
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

        public virtual void UpdateVRNodes()
        {
            InputTracking.GetNodeStates(_vrNodeStates);
            foreach (XRNodeState s in _vrNodeStates)
            {
                if (_essentialNodes.ContainsKey(s.nodeType))
                {
                    _vrNodes[s.uniqueID] = GetVRNode(_essentialNodes[s.nodeType]);
                }
                QuickVRNode n = GetVRNode(s.uniqueID);
                if (n)
                {
                    n.UpdateState(s);
                }
            }
            
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var pair in _vrNodeRoles)
            {
                QuickVRNode n = pair.Value;
                if (n.IsTracked())
                {
                    DebugExtension.DrawCoordinatesSystem(n.transform.position, n.transform.right, n.transform.up, n.transform.forward, 0.05f);

                    float s = 0.0125f;
                    Vector3 cSize = Vector3.one * s;
                    
                    Gizmos.matrix = n.transform.localToWorldMatrix;
                    Gizmos.DrawCube(Vector3.zero, cSize);
                    QuickTrackedObject tObject = n.GetTrackedObject();
                    if (tObject.transform.localPosition != Vector3.zero)
                    {
                        Gizmos.DrawSphere(tObject.transform.localPosition, s * 0.5f);
                        Gizmos.DrawLine(Vector3.zero, tObject.transform.localPosition);
                    }
                    Gizmos.matrix = Matrix4x4.identity;
                }
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


