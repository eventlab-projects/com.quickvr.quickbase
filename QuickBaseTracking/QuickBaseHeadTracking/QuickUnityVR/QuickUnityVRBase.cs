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

        protected static Vector3 HAND_CONTROLLER_POSITION_OFFSET = new Vector3(0, 0, -0.1f);

        #endregion

        #region PUBLIC ATTRIBUTES

        [BitMask(typeof(IKLimbBones))]
        public int _trackedJoints = -1;

        public bool _useFootprints = true;
        public Transform _footprints = null;

        public static bool _handsSwaped = false;

        public enum HandTrackingMode
        {
            Controllers,
            Hands,
        }
        public HandTrackingMode _handTrackingMode = HandTrackingMode.Controllers;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRPlayArea _vrPlayArea = null;

        protected Vector3 _userDisplacement = Vector3.zero; //The accumulated real displacement of the user

        protected Vector3 _initialPosition = Vector3.zero;
        protected Quaternion _initialRotation = Quaternion.identity;

        protected QuickCharacterControllerManager _characterControllerManager = null;

        protected bool _autoUserForward = true; //If true, the forward of the user (the real person) is retrieved from the tracking data at every frame. Otherwise, the user forward is manually provided. 
        protected Vector3 _userForward = Vector3.zero;  //The provided user forward when _autoUserForward is set to false. 

        protected Vector3 _headOffset = Vector3.zero;

        #endregion

        #region EVENTS

        public delegate void CalibrateAction();
        public static event CalibrateAction OnCalibrate;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += OnPostUpdateTracking;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= OnPostUpdateTracking;
        }

        protected override void Awake()
        {
            _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _vrPlayArea.transform.parent = transform;

            base.Awake();

            _characterControllerManager = gameObject.GetOrCreateComponent<QuickCharacterControllerManager>();

            _footprints = Instantiate<GameObject>(Resources.Load<GameObject>("Footprints/Footprints")).transform;
            
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        protected override void Start()
        {
            base.Start();

            if (!QuickUtils.IsHandTrackingSupported())
            {
                _handTrackingMode = HandTrackingMode.Controllers;
            }
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

        public virtual void InitVRNodeFootPrints()
        {
            _userDisplacement = Vector3.zero;
        }

        #endregion

        #region GET AND SET

        public virtual Vector3 GetHeadOffset()
        {
            return _headOffset;
        }

        public virtual Vector3 GetUserForward()
        {
            return _userForward;
        }

        public virtual void SetUserForward(Vector3 fwd)
        {
            _autoUserForward = false;
            _userForward = fwd;
        }

        public virtual void ResetUserForward()
        {
            _autoUserForward = false;
            UpdateUserForward();
        }

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

        public virtual void SetInitialPosition(Vector3 initialPosition)
        {
            _initialPosition = initialPosition;
        }

        public virtual void SetInitialRotation(Quaternion initialRotation)
        {
            _initialRotation = initialRotation;
        }

        protected virtual bool IsExtraTracker(ulong id)
        {
            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (s.uniqueID == id)
                {
                    return s.nodeType == XRNode.HardwareTracker;
                }
            }

            return false;
        }

        protected virtual List<QuickExtraTracker> GetExtraTrackers()
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

        //protected bool IsVRNodesSwaped(QuickVRNode.Type typeNodeLeft, QuickVRNode.Type typeNodeRight, bool doSwaping = true)
        //{
        //    return IsVRNodesSwaped(GetQuickVRNode(typeNodeLeft), GetQuickVRNode(typeNodeRight), doSwaping);
        //}

        //protected bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        //{
        //    QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);

        //    float dLeft = nodeLeft.IsTracked() ? Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;
        //    float dRight = nodeRight.IsTracked() ? Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right) : 0.0f;

        //    bool swaped = dLeft > dRight;
        //    if (swaped && doSwaping)
        //    {
        //        SwapQuickVRNode(nodeLeft, nodeRight);
        //    }

        //    return swaped;
        //}

        protected virtual void ConfigureVRExtraTrackersHMD(List<QuickExtraTracker> extraTrackers)
        {
            if (extraTrackers.Count() == 1)
            {
                //This is the HipsTracker
            }
            //XRNodeState sHead = new XRNodeState();
            //XRNodeState sLeftHand = new XRNodeState();
            //XRNodeState sRightHand = new XRNodeState();
            //sHead.uniqueID = sLeftHand.uniqueID = sRightHand.uniqueID = 0;

            //foreach (XRNodeState s in GetXRNodeStates())
            //{
            //    if (!s.tracked) continue;

            //    if (s.nodeType == XRNode.Head) sHead = s;
            //    else if (s.nodeType == XRNode.LeftHand) sLeftHand = s;
            //    else if (s.nodeType == XRNode.RightHand) sRightHand = s;
            //}

            //if (sLeftHand.uniqueID != 0 && sRightHand.uniqueID != 0)
            //{
            //    GetQuickVRNode(QuickVRNode.Type.Head).SetID(sHead.uniqueID);
            //    GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(sLeftHand.uniqueID);
            //    GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(sRightHand.uniqueID);

            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 1 => Hips
            //    int hipsIndex = GetHipsIndex(extraTrackers);
            //    GetQuickVRNode(QuickVRNode.Type.Hips).SetID(extraTrackers[hipsIndex].Key.uniqueID);
            //    extraTrackers.RemoveAt(hipsIndex);
            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 2 => ... + LeftFoot
            //    GetQuickVRNode(QuickVRNode.Type.LeftFoot).SetID(extraTrackers[0].Key.uniqueID);
            //    extraTrackers.RemoveAt(0);
            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 3 => ... + RightFoot
            //    GetQuickVRNode(QuickVRNode.Type.RightFoot).SetID(extraTrackers[0].Key.uniqueID);
            //    extraTrackers.RemoveAt(0);
            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 4 => ... + LeftLowerArm
            //    GetQuickVRNode(QuickVRNode.Type.LeftLowerArm).SetID(extraTrackers[0].Key.uniqueID);
            //    extraTrackers.RemoveAt(0);
            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 5 => ... + RightLowerArm
            //    GetQuickVRNode(QuickVRNode.Type.RightLowerArm).SetID(extraTrackers[0].Key.uniqueID);
            //    extraTrackers.RemoveAt(0);
            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 6 => ... + LeftLowerLeg
            //    GetQuickVRNode(QuickVRNode.Type.LeftLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
            //    SwapQuickVRNode(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.LeftLowerLeg);
            //    extraTrackers.RemoveAt(0);
            //    if (extraTrackers.Count() == 0) return;

            //    //numTrackers == 7 => ... + RightLowerLeg
            //    GetQuickVRNode(QuickVRNode.Type.RightLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
            //    SwapQuickVRNode(QuickVRNode.Type.RightLowerArm, QuickVRNode.Type.RightLowerLeg);
            //    extraTrackers.RemoveAt(0);
            //    if (extraTrackers.Count() == 0) return;
            //}
            //else
            //{
            //    //Add the HMD Node to the end of the list and make the auto-assignation
            //    //as in the case where NoHMD is detected.
            //    Vector3 pos;
            //    sHead.TryGetPosition(out pos);
            //    extraTrackers.Add(new QuickExtraTracker(sHead, pos));

            //    ConfigureVRExtraTrackersNoHMD(extraTrackers);
            //}
        }

        //protected virtual void ConfigureVRExtraTrackersNoHMD(List<QuickExtraTracker> extraTrackers)
        //{
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 1 => Head
        //    //The head is always considered to be the tracker on the top. 
        //    GetQuickVRNode(QuickVRNode.Type.Head).SetID(extraTrackers[extraTrackers.Count() - 1].Key.uniqueID);
        //    extraTrackers.RemoveAt(extraTrackers.Count() - 1);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 2 => ... + Hips
        //    int HipsIndex = GetHipsIndex(extraTrackers);
        //    GetQuickVRNode(QuickVRNode.Type.Hips).SetID(extraTrackers[HipsIndex].Key.uniqueID);
        //    extraTrackers.RemoveAt(HipsIndex);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 3 => ... + LeftHand
        //    GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(extraTrackers[0].Key.uniqueID);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 4 => ... + RightHand
        //    GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(extraTrackers[0].Key.uniqueID);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 5 => ... + LeftFoot
        //    GetQuickVRNode(QuickVRNode.Type.LeftFoot).SetID(extraTrackers[0].Key.uniqueID);
        //    SwapQuickVRNode(QuickVRNode.Type.LeftHand, QuickVRNode.Type.LeftFoot);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 6 => ... + RightFoot
        //    GetQuickVRNode(QuickVRNode.Type.RightFoot).SetID(extraTrackers[0].Key.uniqueID);
        //    SwapQuickVRNode(QuickVRNode.Type.RightHand, QuickVRNode.Type.RightFoot);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 7 => ... + LeftLowerArm
        //    GetQuickVRNode(QuickVRNode.Type.LeftLowerArm).SetID(extraTrackers[0].Key.uniqueID);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 8 => ... + RightLowerArm
        //    GetQuickVRNode(QuickVRNode.Type.RightLowerArm).SetID(extraTrackers[0].Key.uniqueID);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 9 => ... + LeftLowerLeg
        //    GetQuickVRNode(QuickVRNode.Type.LeftLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
        //    SwapQuickVRNode(QuickVRNode.Type.LeftHand, QuickVRNode.Type.LeftLowerArm);
        //    SwapQuickVRNode(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.LeftLowerLeg);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;

        //    //numTrackers == 10 => ... + RightLowerLeg
        //    GetQuickVRNode(QuickVRNode.Type.RightLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
        //    SwapQuickVRNode(QuickVRNode.Type.RightHand, QuickVRNode.Type.RightLowerArm);
        //    SwapQuickVRNode(QuickVRNode.Type.RightLowerArm, QuickVRNode.Type.RightLowerLeg);
        //    extraTrackers.RemoveAt(0);
        //    if (extraTrackers.Count() == 0) return;
        //}

        //protected int GetHipsIndex(List<QuickExtraTracker> extraTrackers)
        //{
        //    QuickVRNode nodeHead = GetQuickVRNode(QuickVRNode.Type.Head);
        //    Vector3 n = Vector3.ProjectOnPlane(nodeHead.transform.forward, transform.up);
        //    Vector3 p = nodeHead.transform.position;

        //    int HipsIndex = 0;
        //    float dMin = Mathf.Infinity;
        //    for (int i = 0; i < extraTrackers.Count; i++)
        //    {
        //        Vector3 v = Math3d.ProjectPointOnPlane(Vector3.up, p, Math3d.ProjectPointOnPlane(n, p, extraTrackers[i].Value));
        //        float d = Vector3.Distance(v, p);
        //        if (d < dMin)
        //        {
        //            dMin = d;
        //            HipsIndex = i;
        //        }
        //    }

        //    return HipsIndex;
        //}

        //protected int GetHipsIndex(List<QuickExtraTracker> extraTrackers)
        //{
        //    QuickVRNode nodeHead = GetQuickVRNode(QuickVRNode.Type.Head);
        //    Vector3 n = Vector3.ProjectOnPlane(nodeHead.transform.forward, Vector3.up).normalized;
        //    Vector3 p = nodeHead.transform.position;

        //    int HipsIndex = 0;
        //    float aMin = Mathf.Infinity;
        //    Debug.Log("==============================================");
        //    for (int i = 0; i < extraTrackers.Count; i++)
        //    {
        //        Vector3 v = Vector3.ProjectOnPlane((extraTrackers[i].Value - p).normalized, Vector3.up);
        //        float a = Vector3.Angle(n, v);
        //        Debug.Log("i = " + i);
        //        Debug.Log("a = " + a.ToString("f3"));
        //        Debug.Log("pos = " + extraTrackers[i].Value.ToString("f3"));
        //        if (a < aMin)
        //        {
        //            aMin = a;
        //            HipsIndex = i;
        //        }
        //    }

        //    return HipsIndex;
        //}

        //protected int GetHipsIndex(List<QuickExtraTracker> extraTrackers)
        //{
        //    //QuickVRNode nodeHead = GetQuickVRNode(QuickVRNode.Type.Head);
        //    //Vector3 p = nodeHead.transform.position;

        //    //int HipsIndex = 0;
        //    //float aMin = Mathf.Infinity;
        //    //for (int i = 0; i < extraTrackers.Count; i++)
        //    //{
        //    //    Vector3 v = p - extraTrackers[i].Value;
        //    //    float a = Vector3.Angle(Vector3.up, v);
        //    //    if (a < aMin)
        //    //    {
        //    //        aMin = a;
        //    //        HipsIndex = i;
        //    //    }
        //    //}

        //    QuickVRNode nodeHead = GetQuickVRNode(QuickVRNode.Type.Head);
        //    Vector3 p = nodeHead.transform.position;

        //    int HipsIndex = 0;
        //    float dMin = Mathf.Infinity;
        //    for (int i = 0; i < extraTrackers.Count; i++)
        //    {
        //        float d = Vector3.Distance(extraTrackers[i].Value, p);
        //        if (d < dMin)
        //        {
        //            HipsIndex = i;
        //            dMin = d;
        //        }
        //    }

        //    return HipsIndex;
        //}

        //protected virtual void SwapQuickVRNode(QuickVRNode.Type typeA, QuickVRNode.Type typeB)
        //{
        //    SwapQuickVRNode(GetQuickVRNode(typeA), GetQuickVRNode(typeB));
        //}

        //protected virtual void SwapQuickVRNode(QuickVRNode vrNodeA, QuickVRNode vrNodeB)
        //{
        //    ulong tmp = vrNodeA.GetID();
        //    vrNodeA.SetID(vrNodeB.GetID());
        //    vrNodeB.SetID(tmp);
        //}

        public override void Calibrate()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;

            CalibrateVRNodes();
            CalibrateVRPlayArea();

            base.Calibrate();

            if (OnCalibrate != null) OnCalibrate();
        }

        protected virtual void CalibrateVRNodes()
        {
            //Get all the tracked extratrackers and sort them by Y, from lowest to higher. 
            List<QuickExtraTracker> extraTrackers = GetExtraTrackers();
            extraTrackers = extraTrackers.OrderBy(o => o.Value.y).ToList();

            Debug.Log("NUM EXTRA TRACKERS = " + extraTrackers.Count());

            if (IsHMDPresent()) ConfigureVRExtraTrackersHMD(extraTrackers);
            //else ConfigureVRExtraTrackersNoHMD(extraTrackers);

            ////Check if left/right nodes are set in the correct side, and swap them if necessary. 
            //_handsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
            //IsVRNodesSwaped(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.RightLowerArm);

            //IsVRNodesSwaped(QuickVRNode.Type.LeftFoot, QuickVRNode.Type.RightFoot);
            //IsVRNodesSwaped(QuickVRNode.Type.LeftLowerLeg, QuickVRNode.Type.RightLowerLeg);

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                QuickVRNode node = _vrPlayArea.GetVRNode(t);
                if (node) CalibrateVRNode(node);
            }
        }

        protected virtual void CalibrateVRNode(QuickVRNode node)
        {
            QuickTrackedObject tObject = node.GetTrackedObject();
            tObject.transform.ResetTransformation();

            QuickVRNode.Type role = node.GetRole();
            if (role == QuickVRNode.Type.Head)
            {
                transform.localScale = Vector3.one;
                //if (_applyUserScale)
                //{
                //    Vector3 offset = _camera.transform.TransformVector(_headOffset);
                //    Vector3 userHeadPos = _camera.transform.position + offset;
                //    float userHeadHeight = _camera.transform.localPosition.y - (_camera.transform.position.y - userHeadPos.y);

                //    Debug.Log("userHeadHeight = " + userHeadHeight.ToString("f3"));
                //    transform.localScale *= (userHeadHeight / _ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb.localPosition.y);
                //}

                //Set the offset of the TrackedObject of the head
                node.GetTrackedObject().transform.localPosition = _headOffset * transform.lossyScale.x;
            }
            else if (role == QuickVRNode.Type.Hips)
            {
                QuickTrackedObject tObjectHead = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject();
                if (_vrPlayArea.IsTrackedNode(node))
                {
                    tObject.transform.position = new Vector3(tObjectHead.transform.position.x, tObject.transform.position.y, tObjectHead.transform.position.z);
                }
                else
                {
                    node.transform.position = new Vector3(tObjectHead.transform.position.x, tObjectHead.transform.position.y - 0.8f, tObjectHead.transform.position.z);
                }
            }
            else if (role == QuickVRNode.Type.LeftHand)
            {
                //if (IsExtraTracker(node.GetID()))
                //{
                //    //tObject.transform.Rotate(tObject.transform.right, 90.0f, Space.World);
                //    //tObject.transform.rotation = _vrNodesOrigin.rotation;
                //    //float d = Vector3.Dot(node.transform.forward, _vrNodesOrigin.up);
                //    //if (d < 0.5f)
                //    //{
                //    //    tObject.transform.Rotate(_vrNodesOrigin.right, 90.0f, Space.World);
                //    //    tObject.transform.Rotate(_vrNodesOrigin.up, nodeType == QuickVRNode.Type.LeftHand? -90.0f : 90.0f, Space.World);
                //    //}
                //}
                //else
                {
                    //This is a controller
                    //float sign = role == QuickVRNode.Type.LeftHand ? 1.0f : -1.0f;
                    //tObject.transform.Rotate(tObject.transform.forward, sign * 90.0f, Space.World);
                    //tObject.transform.localPosition = HAND_CONTROLLER_POSITION_OFFSET;

                    //tObject.transform.LookAt(tObject.transform.position + node.transform.right, -node.transform.up);
                }

                if (_handTrackingMode == HandTrackingMode.Controllers)
                {
                    tObject.transform.Rotate(tObject.transform.forward, 90.0f, Space.World);
                    tObject.transform.localPosition = HAND_CONTROLLER_POSITION_OFFSET;
                }
                else
                {
                    tObject.transform.LookAt(tObject.transform.position + node.transform.right, -node.transform.up);
                }
            }
            else if (role == QuickVRNode.Type.RightHand)
            {
                if (_handTrackingMode == HandTrackingMode.Controllers)
                {
                    tObject.transform.Rotate(tObject.transform.forward, -90.0f, Space.World);
                    tObject.transform.localPosition = HAND_CONTROLLER_POSITION_OFFSET;
                }
                else
                {
                    tObject.transform.LookAt(tObject.transform.position - node.transform.right, node.transform.up);
                }
            }
            else if (role == QuickVRNode.Type.LeftFoot || role == QuickVRNode.Type.RightFoot)
            {
                //tObject.transform.rotation = _vrNodesOrigin.rotation;
                //tObject.transform.position += -_vrNodesOrigin.forward * 0.075f;
            }
            else if (role == QuickVRNode.Type.LeftLowerArm || role == QuickVRNode.Type.RightLowerArm)
            {
                tObject.transform.position += (-node.transform.forward * 0.1f);// + (-_vrNodesOrigin.up * 0.1f);
            }
        }

        protected virtual void CalibrateVRPlayArea()
        {
            _vrPlayArea.Calibrate();

            float rotAngle = Vector3.SignedAngle(_userForward, transform.forward, transform.up);
            _vrPlayArea.transform.Rotate(transform.up, rotAngle, Space.World);
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

            UpdateTransformRoot();
            UpdateTransformNodes();

            UpdateFootPrints();

            UpdateVRCursors();

            UpdateUserForward();
        }

        protected virtual void OnPostUpdateTracking()
        {
            UpdateCameraPosition();
        }

        protected virtual void UpdateFootPrints()
        {
            _footprints.position = transform.position - _userDisplacement;
            _footprints.gameObject.SetActive(_useFootprints);
        }

        protected virtual void UpdateTransformRoot()
        {
            ////Update the rotation
            //float rotOffset = GetRotationOffset();
            //transform.Rotate(transform.up, rotOffset, Space.World);
            //_vrNodesOrigin.Rotate(_vrNodesOrigin.up, rotOffset, Space.World);

            ////Update the position
            //Vector3 disp = Vector3.Scale(GetDisplacement(), Vector3.right + Vector3.forward);
            //_vrNodesOrigin.Translate(_vrNodesOrigin.InverseTransformVector(disp), Space.Self);

            //Vector3 userDisp = ToAvatarSpace(disp);
            //transform.Translate(userDisp, Space.World);
            //_characterControllerManager.SetStepVelocity(userDisp / Time.deltaTime);
            //_userDisplacement += userDisp;
        }

        protected virtual void UpdateTransformNodes()
        {
            //QuickTrackedObject tHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            //GetQuickVRNode(QuickVRNode.Type.LeftUpperArm).transform.position = tHead.transform.position - _vrNodesOrigin.right * 0.15f;
            //GetQuickVRNode(QuickVRNode.Type.RightUpperArm).transform.position = tHead.transform.position + _vrNodesOrigin.right * 0.15f;

            //QuickTrackedObject tHips = GetQuickVRNode(QuickVRNode.Type.Hips).GetTrackedObject();
            //GetQuickVRNode(QuickVRNode.Type.LeftUpperLeg).transform.position = tHips.transform.position - _vrNodesOrigin.right * 0.10f;
            //GetQuickVRNode(QuickVRNode.Type.RightUpperLeg).transform.position = tHips.transform.position + _vrNodesOrigin.right * 0.10f;
        }

        protected virtual void UpdateCameraPosition()
        {
            //Apply the correct rotation to the cameracontrollerroot:
            QuickVRNode nodeHead = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head);
            if (!nodeHead) return;

            Vector3 fwdCam = Vector3.ProjectOnPlane(_camera.transform.forward, transform.up).normalized;
            Vector3 fwdHead = Vector3.ProjectOnPlane(nodeHead.GetTrackedObject().transform.forward, transform.up).normalized;
            float rotOffset = Vector3.SignedAngle(fwdCam, fwdHead, transform.up);
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

        protected virtual void UpdateUserForward()
        {
            QuickVRNode node = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head);
            if (_autoUserForward && node)
            {
                _userForward = Vector3.ProjectOnPlane(node.transform.forward, transform.up);
            }
        }

        //protected virtual void OnHMDConnected(XRNodeState state)
        //{
        //    GetQuickVRNode(QuickVRNode.Type.Head).SetID(state.uniqueID);
        //    Calibrate();
        //    InitVRNodeFootPrints();
        //}

        //protected virtual void OnLeftHandConnected(XRNodeState state)
        //{
        //    GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(state.uniqueID);
        //    _handsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
        //    CalibrateVRNode(_handsSwaped? QuickVRNode.Type.RightHand : QuickVRNode.Type.LeftHand);
        //}

        //protected virtual void OnRightHandConnected(XRNodeState state)
        //{
        //    GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(state.uniqueID);
        //    _handsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
        //    CalibrateVRNode(_handsSwaped? QuickVRNode.Type.LeftHand : QuickVRNode.Type.RightHand);
        //}

        #endregion

    }

}
