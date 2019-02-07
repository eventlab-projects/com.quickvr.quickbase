using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

using QuickExtraTracker = System.Collections.Generic.KeyValuePair<UnityEngine.XR.XRNodeState, UnityEngine.Vector3>;
using System.Linq;

namespace QuickVR
{

    public abstract class QuickUnityVRBase : QuickHeadTracking
    {

        #region CONSTANTS

        protected static float HUMAN_HEADS_TALL = 7.5f;
        protected static float HUMAN_HEADS_TALL_EYES = HUMAN_HEADS_TALL - 0.5f;
        protected static float HUMAN_HEADS_TALL_HEAD = HUMAN_HEADS_TALL - 1.0f;

        #endregion

        #region PUBLIC ATTRIBUTES

        public Vector3 _handControllerPositionOffset = new Vector3(0, 0, -0.1f);

        [BitMask(typeof(IKLimbBones))]
        public int _trackedJoints = -1;

        public bool _useFootprints = true;
        public Transform _footprints = null;

        public static bool _handsSwaped = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Transform _vrNodesRoot = null;
        protected Transform _vrNodesOrigin = null;

        protected Vector3 _userDisplacement = Vector3.zero; //The accumulated real displacement of the user

        protected Vector3 _initialPosition = Vector3.zero;
        protected Quaternion _initialRotation = Quaternion.identity;

        protected QuickCharacterControllerManager _characterControllerManager = null;

        #endregion

        #region EVENTS

        public delegate void CalibrateAction();
        public static event CalibrateAction OnCalibrate;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _characterControllerManager = gameObject.GetOrCreateComponent<QuickCharacterControllerManager>();

            _footprints = Instantiate<GameObject>(Resources.Load<GameObject>("Footprints/Footprints")).transform;
            _footprints.gameObject.SetActive(_useFootprints);

            _initialPosition = transform.position;
            _initialRotation = transform.rotation;

            CreateVRNodes();
        }

        protected override void CreateVRCursors()
        {
            base.CreateVRCursors();

            CreateVRCursorHand(VRCursorType.LEFT, _vrHandLeft._handBone, _vrHandLeft._handBoneIndexDistal);
            CreateVRCursorHand(VRCursorType.RIGHT, _vrHandRight._handBone, _vrHandRight._handBoneIndexDistal);
        }

        protected virtual void CreateVRCursorHand(VRCursorType cType, Transform tHand, Transform tDistal)
        {
            Transform tIntermediate = tDistal.parent;
            Transform tProximal = tIntermediate.parent;
            float l1 = Vector3.Distance(tDistal.position, tIntermediate.position);
            float l2 = Vector3.Distance(tIntermediate.position, tProximal.position);
            Transform cursorOrigin = tHand.CreateChild("__CursorOrigin__");
            cursorOrigin.forward = (tIntermediate.position - tProximal.position).normalized;
            cursorOrigin.position = tProximal.position + cursorOrigin.forward * (l1 + l2 + (l2 - l1));

            CreateVRCursor(cType, cursorOrigin);
        }

        protected virtual void CreateVRNodes()
        {
            //Create the VRNodes
            _vrNodesRoot = new GameObject("__VRNodesRoot__").transform;
            _vrNodesOrigin = _vrNodesRoot.CreateChild("__Origin__");

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                CreateVRNode(t);
            }
        }

        protected virtual QuickVRNode CreateVRNode(QuickVRNode.Type n)
        {
            return _vrNodesRoot.CreateChild(n.ToString()).gameObject.GetOrCreateComponent<QuickVRNode>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InputTracking.trackingAcquired += OnXRNodeTracked;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            InputTracking.trackingAcquired -= OnXRNodeTracked;
        }

        protected virtual void OnXRNodeTracked(XRNodeState state)
        {
            StartCoroutine(CoXRNodeTracked(state));
        }

        protected virtual IEnumerator CoXRNodeTracked(XRNodeState state)
        {
            yield return null;

            List<XRNodeState> xRNodeStates = new List<XRNodeState>();
            InputTracking.GetNodeStates(xRNodeStates);
            foreach (XRNodeState s in xRNodeStates)
            {
                if (s.nodeType == state.nodeType)
                {
                    if (s.nodeType == XRNode.Head) OnHMDConnected(s);
                    else if (s.nodeType == XRNode.LeftHand) OnLeftHandConnected(s); 
                    else if (s.nodeType == XRNode.RightHand) OnRightHandConnected(s); 
                }
            }
        }

        public virtual void InitVRNodeFootPrints()
        {
            _userDisplacement = Vector3.zero;
        }

        #endregion

        #region GET AND SET

        protected virtual bool IsHMDPresent()
        {
            return (XRDevice.isPresent && !XRDevice.model.ToLower().StartsWith("null"));
        }

        protected virtual List<XRNodeState> GetXRNodeStates()
        {
            List<XRNodeState> xRNodeStates = new List<XRNodeState>();
            InputTracking.GetNodeStates(xRNodeStates);

            return xRNodeStates;
        }

        protected virtual Vector3 ToAvatarSpace(Vector3 v)
        {
            //return transform.rotation * Quaternion.Inverse(_vrNodesOrigin.rotation) * v;
            return transform.TransformVector(_vrNodesOrigin.InverseTransformVector(v));
        }

        protected virtual Quaternion ToAvatarSpace(Quaternion q)
        {
            return transform.rotation * Quaternion.Inverse(_vrNodesOrigin.rotation) * q;
        }

        public virtual void SetInitialPosition(Vector3 initialPosition)
        {
            _initialPosition = initialPosition;
        }

        public virtual void SetInitialRotation(Quaternion initialRotation)
        {
            _initialRotation = initialRotation;
        }

        protected List<QuickExtraTracker> GetExtraTrackers()
        {
            List<QuickExtraTracker> extraTrackers = new List<QuickExtraTracker>();
            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (s.tracked && s.nodeType == XRNode.HardwareTracker)
                {
                    Vector3 pos;
                    s.TryGetPosition(out pos);
                    extraTrackers.Add(new QuickExtraTracker(s, pos));
                }
            }

            return extraTrackers;
        }

        public virtual int GetNumExtraTrackers()
        {
            return GetExtraTrackers().Count;
        }

        public abstract Vector3 GetDisplacement();
        protected abstract float GetRotationOffset();

        protected virtual void CheckVRHands()
        {
            //Check if the hands are reversed
            QuickVRNode leftHandNode = GetQuickVRNode(QuickVRNode.Type.LeftHand);
            QuickVRNode rightHandNode = GetQuickVRNode(QuickVRNode.Type.RightHand);

            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (!s.tracked) continue;

                if (s.nodeType == XRNode.LeftHand)
                {
                    leftHandNode.SetID(s.uniqueID);
                }
                else if (s.nodeType == XRNode.RightHand)
                {
                    rightHandNode.SetID(s.uniqueID);
                }
            }

            _handsSwaped = IsVRNodesSwaped(leftHandNode, rightHandNode);
        }

        protected bool IsVRNodesSwaped(QuickVRNode.Type typeNodeLeft, QuickVRNode.Type typeNodeRight, bool doSwaping = true)
        {
            return IsVRNodesSwaped(GetQuickVRNode(typeNodeLeft), GetQuickVRNode(typeNodeRight), doSwaping);
        }

        protected bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        {
            QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);

            float dLeft = nodeLeft.IsTracked() ? Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;
            float dRight = nodeRight.IsTracked() ? Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;

            bool swaped = dLeft > dRight;
            if (swaped && doSwaping)
            {
                SwapQuickVRNode(nodeLeft, nodeRight);
            }

            return swaped;
        }

        protected virtual void CheckVRExtraTrackers()
        {
            //Get all the tracked extratrackers and sort them by Y, from lowest to higher. 
            List<QuickExtraTracker> extraTrackers = GetExtraTrackers();
            extraTrackers = extraTrackers.OrderBy(o => o.Value.y).ToList();

            Debug.Log("NUM EXTRA TRACKERS = " + extraTrackers.Count());
            //Debug.Log("??????????????????????????");
            //foreach (QuickExtraTracker t in extraTrackers)
            //{
            //    Debug.Log(t.Value.y.ToString("f3"));
            //}

            if (extraTrackers.Count > 0)
            {
                if (IsHMDPresent()) ConfigureVRExtraTrackersHMD(extraTrackers);
                else ConfigureVRExtraTrackersNoHMD(extraTrackers);
            }

            //Check if left/right nodes are set in the correct side, and swap them if necessary. 
            IsVRNodesSwaped(QuickVRNode.Type.LeftFoot, QuickVRNode.Type.RightFoot);
            IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
        }

        protected virtual void ConfigureVRExtraTrackersHMD(List<QuickExtraTracker> extraTrackers)
        {
            int numTrackers = extraTrackers.Count;
            QuickVRNode HipsNode = GetQuickVRNode(QuickVRNode.Type.Hips);
            QuickVRNode leftFootNode = GetQuickVRNode(QuickVRNode.Type.LeftFoot);
            QuickVRNode rightFootNode = GetQuickVRNode(QuickVRNode.Type.RightFoot);

            if (numTrackers == 1)
            {
                //We guess the extra tracker is the Hips. 
                HipsNode.SetID(extraTrackers[0].Key.uniqueID);
            }
            else if (numTrackers == 2)
            {
                //We guess the extra trackers are the two feet. 
                leftFootNode.SetID(extraTrackers[0].Key.uniqueID);
                rightFootNode.SetID(extraTrackers[1].Key.uniqueID);
            }
            else if (numTrackers == 3)
            {
                //The tracker with the higher Y position is the Hips. The other 2 are the feet. 
                HipsNode.SetID(extraTrackers[2].Key.uniqueID);
                leftFootNode.SetID(extraTrackers[0].Key.uniqueID);
                rightFootNode.SetID(extraTrackers[1].Key.uniqueID);
            }
        }

        protected virtual void ConfigureVRExtraTrackersNoHMD(List<QuickExtraTracker> extraTrackers)
        {
            int numTrackers = extraTrackers.Count;

            //The head is always considered to be the tracker on the top. 
            GetQuickVRNode(QuickVRNode.Type.Head).SetID(extraTrackers[numTrackers - 1].Key.uniqueID);
            extraTrackers.RemoveAt(numTrackers - 1);

            if (numTrackers == 1) return;

            if (numTrackers == 2)
            {
                //Head + Hips
                GetQuickVRNode(QuickVRNode.Type.Hips).SetID(extraTrackers[0].Key.uniqueID);
            }
            else if (numTrackers == 3)
            {
                //Head + Hands
                GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(extraTrackers[0].Key.uniqueID);
                GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(extraTrackers[1].Key.uniqueID);
            }
            else if (numTrackers == 4 || numTrackers == 6)
            {
                //4 =>  Head + Hips + Hands
                //6 =>  Head + Hips + Hands + Feet 
                //The Hips is the node more "centered" with respect to the head node. 
                int HipsIndex = GetHipsIndex(extraTrackers);
                GetQuickVRNode(QuickVRNode.Type.Hips).SetID(extraTrackers[HipsIndex].Key.uniqueID);
                extraTrackers.RemoveAt(HipsIndex);

                //The hands are the nodes which has a higher Y position from the remaining ones
                GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(extraTrackers[extraTrackers.Count - 1].Key.uniqueID);
                GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(extraTrackers[extraTrackers.Count - 2].Key.uniqueID);

                if (numTrackers == 6)
                {
                    //The feet are the other remaining nodes
                    GetQuickVRNode(QuickVRNode.Type.LeftFoot).SetID(extraTrackers[0].Key.uniqueID);
                    GetQuickVRNode(QuickVRNode.Type.RightFoot).SetID(extraTrackers[1].Key.uniqueID);
                }

            }
            else
            {
                Debug.LogError("Invalid number of Trackers!!! = " + numTrackers);
            }
        }

        protected int GetHipsIndex(List<QuickExtraTracker> extraTrackers)
        {
            QuickVRNode nodeHead = GetQuickVRNode(QuickVRNode.Type.Head);
            Vector3 n = Vector3.ProjectOnPlane(nodeHead.transform.forward, transform.up);
            Vector3 p = nodeHead.transform.position;
            
            int HipsIndex = 0;
            float dMin = Mathf.Infinity;
            for (int i = 0; i < extraTrackers.Count; i++)
            {
                Vector3 v = Math3d.ProjectPointOnPlane(Vector3.up, p, Math3d.ProjectPointOnPlane(n, p, extraTrackers[i].Value));
                float d = Vector3.Distance(v, p);
                if (d < dMin)
                {
                    dMin = d;
                    HipsIndex = i;
                }
            }

            return HipsIndex;
        }

        protected virtual void SwapQuickVRNode(QuickVRNode vrNodeA, QuickVRNode vrNodeB)
        {
            ulong tmp = vrNodeA.GetID();
            vrNodeA.SetID(vrNodeB.GetID());
            vrNodeB.SetID(tmp);
        }

        public override void Calibrate()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;

            CalibrateVRNodes();
            CalibrateCameraForward();

            base.Calibrate();

            if (OnCalibrate != null) OnCalibrate();
        }

        protected virtual void CalibrateVRNodes()
        {
            //Reset the IDs of the VRNodes
            List<QuickVRNode.Type> nodeTypes = QuickUtils.GetEnumValues<QuickVRNode.Type>();
            foreach (QuickVRNode.Type t in nodeTypes) GetQuickVRNode(t).SetID(0);
            
            CheckVRExtraTrackers();
            CheckVRHands();

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                CalibrateVRNode(t);
            }
        }

        protected virtual void CalibrateVRNode(QuickVRNode.Type nodeType)
        {
            QuickVRNode node = GetQuickVRNode(nodeType);
            QuickTrackedObject tObject = node.GetTrackedObject();
            tObject.transform.rotation = node.transform.rotation;
            
            if (nodeType == QuickVRNode.Type.Head)
            {
                CalibrateVRNodeHead(node);
            }
            else if (nodeType == QuickVRNode.Type.Hips)
            {
                QuickTrackedObject tObjectHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
                if (node.IsTracked())
                {
                    tObject.transform.position = new Vector3(tObjectHead.transform.position.x, tObject.transform.position.y, tObjectHead.transform.position.z);
                }
                else
                {
                    tObject.transform.localPosition = Vector3.zero;
                    node.transform.position = new Vector3(tObjectHead.transform.position.x, tObjectHead.transform.position.y - 0.8f, tObjectHead.transform.position.z);
                }
            }
            else if (nodeType == QuickVRNode.Type.LeftHand || nodeType == QuickVRNode.Type.RightHand)
            {
                tObject.transform.localPosition = _handControllerPositionOffset;
                tObject.transform.rotation = _vrNodesOrigin.rotation;
                tObject.transform.Rotate(tObject.transform.right, 90.0f, Space.World);

                float sign = nodeType == QuickVRNode.Type.LeftHand? 1.0f : -1.0f;
                tObject.transform.Rotate(tObject.transform.forward, sign * 90.0f, Space.World);
            }
            else if (nodeType == QuickVRNode.Type.LeftFoot || nodeType == QuickVRNode.Type.RightFoot)
            {
                tObject.transform.rotation = _vrNodesOrigin.rotation;
            }

        }

        protected virtual void CalibrateVRNodeHead(QuickVRNode node)
        {
            _vrNodesOrigin.forward = Vector3.ProjectOnPlane(node.transform.forward, transform.up);
            _footprints.rotation = ToAvatarSpace(_vrNodesOrigin.rotation);
        }

        protected virtual void CalibrateCameraForward()
        {
            //Calculate the camera rotation offset
            _cameraControllerRoot.rotation = transform.rotation;
            Vector3 fwdCam = Vector3.ProjectOnPlane(_camera.transform.forward, transform.up).normalized;
        }

        public virtual QuickVRNode GetQuickVRNode(QuickVRNode.Type node)
        {
            return _vrNodesRoot ? _vrNodesRoot.Find(node.ToString()).GetComponent<QuickVRNode>() : null;
        }

        public virtual QuickVRHand GetVRHand(QuickVRNode.Type nType)
        {
            if (nType == QuickVRNode.Type.LeftHand) return _vrHandLeft;
            if (nType == QuickVRNode.Type.RightHand) return _vrHandRight;

            return null;
        }

        #endregion

        #region UPDATE

        public override void UpdateTracking()
        {
            base.UpdateTracking();

            UpdateQuickVRNodeIDs();

            UpdateTransformRoot();
            UpdateTransformNodes();

            UpdateCameraPosition();

            UpdateFootPrints();

            UpdateVRCursors();
        }

        protected virtual void UpdateQuickVRNodeIDs()
        {
            List<XRNodeState> xRNodeStates = new List<XRNodeState>();
            InputTracking.GetNodeStates(xRNodeStates);
            foreach (XRNodeState s in xRNodeStates)
            {
                if (!s.tracked) continue;

                if (IsHMDPresent() && (s.nodeType == XRNode.Head)) GetQuickVRNode(QuickVRNode.Type.Head).SetID(s.uniqueID);
                else if (s.nodeType == XRNode.LeftHand) GetQuickVRNode(!_handsSwaped? QuickVRNode.Type.LeftHand : QuickVRNode.Type.RightHand).SetID(s.uniqueID);
                else if (s.nodeType == XRNode.RightHand) GetQuickVRNode(!_handsSwaped? QuickVRNode.Type.RightHand : QuickVRNode.Type.LeftHand).SetID(s.uniqueID);
            }
        }

        protected virtual void UpdateFootPrints()
        {
            _footprints.position = transform.position - _userDisplacement;
        }

        protected virtual void UpdateTransformRoot()
        {
            //Update the rotation
            float rotOffset = GetRotationOffset();
            transform.Rotate(transform.up, rotOffset, Space.World);
            _vrNodesOrigin.Rotate(_vrNodesOrigin.up, rotOffset, Space.World);

            //Update the position
            Vector3 disp = Vector3.Scale(GetDisplacement(), Vector3.right + Vector3.forward);
            _vrNodesOrigin.Translate(_vrNodesOrigin.InverseTransformVector(disp), Space.Self);

            Vector3 userDisp = ToAvatarSpace(disp);
            transform.Translate(userDisp, Space.World);
            _characterControllerManager.SetStepVelocity(userDisp / Time.deltaTime);
            _userDisplacement += userDisp;
        }

        protected virtual void UpdateTransformNodes()
        {
            QuickTrackedObject tHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            GetQuickVRNode(QuickVRNode.Type.LeftUpperArm).transform.position = tHead.transform.position - _vrNodesOrigin.right * 0.15f;
            GetQuickVRNode(QuickVRNode.Type.RightUpperArm).transform.position = tHead.transform.position + _vrNodesOrigin.right * 0.15f;

            QuickTrackedObject tHips = GetQuickVRNode(QuickVRNode.Type.Hips).GetTrackedObject();
            GetQuickVRNode(QuickVRNode.Type.LeftUpperLeg).transform.position = tHips.transform.position - _vrNodesOrigin.right * 0.10f;
            GetQuickVRNode(QuickVRNode.Type.RightUpperLeg).transform.position = tHips.transform.position + _vrNodesOrigin.right * 0.10f;
        }

        protected virtual void UpdateCameraPosition()
        {
            //Apply the correct rotation to the cameracontrollerroot:
            //1) Align the camera with the current avatar's forward
            //2) Apply the rotation offset defined by the head node
            QuickVRNode nodeHead = GetQuickVRNode(QuickVRNode.Type.Head);
            Vector3 fwdCam = Vector3.ProjectOnPlane(_camera.transform.forward, transform.up).normalized;
            Vector3 fwdHead = Vector3.ProjectOnPlane(nodeHead.GetTrackedObject().transform.forward, transform.up).normalized;
            float rotOffset = Vector3.SignedAngle(fwdCam, transform.forward, transform.up) + Vector3.SignedAngle(_vrNodesOrigin.forward, fwdHead, transform.up);
            _cameraControllerRoot.Rotate(transform.up, rotOffset, Space.World);
            
            //This forces the camera to be in the Avatar's eye center. 
            Vector3 offset = GetEyeCenterPosition() - _camera.transform.position;
            _cameraControllerRoot.position += offset;
        }

        protected virtual void UpdateVRCursors()
        {
            GetVRCursor(VRCursorType.LEFT).transform.position = _vrHandLeft._handBoneIndexDistal.position;
            GetVRCursor(VRCursorType.RIGHT).transform.position = _vrHandRight._handBoneIndexDistal.position;
        }

        protected virtual void OnHMDConnected(XRNodeState state)
        {
            GetQuickVRNode(QuickVRNode.Type.Head).SetID(state.uniqueID);
            Calibrate();
            InitVRNodeFootPrints();
        }

        protected virtual void OnLeftHandConnected(XRNodeState state)
        {
            GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(state.uniqueID);
            CheckVRHands();
            CalibrateVRNode(QuickVRNode.Type.LeftHand);
        }

        protected virtual void OnRightHandConnected(XRNodeState state)
        {
            GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(state.uniqueID);
            CheckVRHands();
            CalibrateVRNode(QuickVRNode.Type.RightHand);
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                DebugVRNodesOrigin();

                //DebugVRNode(QuickVRNode.Type.TrackingReference, Color.yellow);

                DebugVRNode(QuickVRNode.Type.Head, Color.grey);

                DebugVRNode(QuickVRNode.Type.LeftUpperArm, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.LeftHand, Color.blue);

                DebugVRNode(QuickVRNode.Type.RightUpperArm, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.RightHand, Color.red);

                DebugVRNode(QuickVRNode.Type.Hips, Color.black, true);

                DebugVRNode(QuickVRNode.Type.LeftUpperLeg, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.LeftFoot, Color.cyan);

                DebugVRNode(QuickVRNode.Type.RightUpperLeg, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.RightFoot, Color.magenta);

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.LeftUpperArm, true);
                DebugVRNodeConnection(QuickVRNode.Type.LeftUpperArm, QuickVRNode.Type.LeftHand, GetQuickVRNode(QuickVRNode.Type.LeftHand).IsTracked());

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.RightUpperArm, true);
                DebugVRNodeConnection(QuickVRNode.Type.RightUpperArm, QuickVRNode.Type.RightHand, GetQuickVRNode(QuickVRNode.Type.RightHand).IsTracked());

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.Hips, true);

                DebugVRNodeConnection(QuickVRNode.Type.Hips, QuickVRNode.Type.LeftUpperLeg, true);
                DebugVRNodeConnection(QuickVRNode.Type.LeftUpperLeg, QuickVRNode.Type.LeftFoot, GetQuickVRNode(QuickVRNode.Type.LeftFoot).IsTracked());

                DebugVRNodeConnection(QuickVRNode.Type.Hips, QuickVRNode.Type.RightUpperLeg, true);
                DebugVRNodeConnection(QuickVRNode.Type.RightUpperLeg, QuickVRNode.Type.RightFoot, GetQuickVRNode(QuickVRNode.Type.RightFoot).IsTracked());

                //Gizmos.color = Color.white;
                //QuickVRNode.Type tNode = GetQuickVRNode(QuickVRNode.Type.Hips).IsTracked() ? QuickVRNode.Type.Hips : QuickVRNode.Type.Head;
                //Gizmos.DrawLine(GetQuickVRNode(tNode).GetTrackedObject().transform.position, _vrNodesOrigin.position);
            }
        }

        protected virtual void DebugVRNodesOrigin()
        {
            DebugExtension.DrawCoordinatesSystem(_vrNodesOrigin.position, _vrNodesOrigin.right, _vrNodesOrigin.up, _vrNodesOrigin.forward, 0.1f);
        }

        protected virtual void DebugVRNode(QuickVRNode.Type nType, Color color, bool forceDraw = false)
        {
            float scale = 0.05f;
            QuickVRNode qNode = GetQuickVRNode(nType);
            if (qNode.IsTracked() || forceDraw)
            {
                Transform tNode = qNode.transform;
                Transform tTracked = qNode.GetTrackedObject().transform;

                Gizmos.color = color;
                Gizmos.DrawCube(tNode.position, new Vector3(scale, scale, scale));
                Gizmos.DrawSphere(tTracked.position, scale * 0.5f);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(tNode.position, tTracked.position);
            }
        }

        protected virtual void DebugVRNodeConnection(QuickVRNode.Type n1Type, QuickVRNode.Type n2Type, bool forceDraw = false)
        {
            QuickVRNode n1 = GetQuickVRNode(n1Type);
            QuickVRNode n2 = GetQuickVRNode(n2Type);
            if ((n1.IsTracked() && n2.IsTracked()) || forceDraw)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(n1.GetTrackedObject().transform.position, n2.GetTrackedObject().transform.position);
            }
        }

        #endregion

    }

}
