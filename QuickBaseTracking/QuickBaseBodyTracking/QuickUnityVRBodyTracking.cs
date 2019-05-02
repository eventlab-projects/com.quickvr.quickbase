using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

using QuickExtraTracker = System.Collections.Generic.KeyValuePair<UnityEngine.XR.XRNodeState, UnityEngine.Vector3>;
using System.Linq;

namespace QuickVR
{
    public class QuickUnityVRBodyTracking : QuickBodyTracking<QuickVRNode.Type>
    {

        #region PUBLIC ATTRIBUTES

        public Vector3 _handControllerPositionOffset = new Vector3(0, 0, -0.1f);

        public bool _useFootprints = true;
        public Transform _footprints = null;

        public static bool _handsSwaped = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickIKManager _ikManager = null;

        protected Transform _vrNodesRoot = null;
        protected Transform _vrNodesOrigin = null;

        protected Vector3 _userDisplacement = Vector3.zero; //The accumulated real displacement of the user

        protected float _unscaledHeadHeight = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            //Create the IKManager
            _ikManager = gameObject.GetOrCreateComponent<QuickIKManager>();
            _ikManager.enabled = false; //We control when to update the IK

            _footprints = Instantiate<GameObject>(Resources.Load<GameObject>("Footprints/Footprints")).transform;
            _footprints.gameObject.SetActive(_useFootprints);

            _unscaledHeadHeight = (_animator.GetBoneTransform(HumanBodyBones.Head).position.y - transform.position.y);

            CreateVRNodes();
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

            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (s.nodeType == state.nodeType)
                {
                    if (s.nodeType == XRNode.Head) OnHMDConnected(s);
                    else if (s.nodeType == XRNode.LeftHand) OnLeftHandConnected(s);
                    else if (s.nodeType == XRNode.RightHand) OnRightHandConnected(s);
                }
            }
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
            _handsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
            CalibrateVRNode(_handsSwaped ? QuickVRNode.Type.RightHand : QuickVRNode.Type.LeftHand);
        }

        protected virtual void OnRightHandConnected(XRNodeState state)
        {
            GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(state.uniqueID);
            _handsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
            CalibrateVRNode(_handsSwaped ? QuickVRNode.Type.LeftHand : QuickVRNode.Type.RightHand);
        }

        public virtual void InitVRNodeFootPrints()
        {
            _userDisplacement = Vector3.zero;
        }

        protected override void CreateIntermediateSkeletonMap()
        {
            //Spine Mapping
            CreateJointMapQuickVR(QuickJointType.SPINE_BASE, QuickVRNode.Type.Hips);
            CreateJointMapQuickVR(QuickJointType.HEAD, QuickVRNode.Type.Head);

            //Left Arm Mapping
            CreateJointMapQuickVR(QuickJointType.SHOULDER_LEFT, QuickVRNode.Type.LeftUpperArm);
            CreateJointMapQuickVR(QuickJointType.ELBOW_LEFT, QuickVRNode.Type.LeftLowerArm);
            CreateJointMapQuickVR(QuickJointType.HAND_LEFT, QuickVRNode.Type.LeftHand);

            //Right Arm Mapping
            CreateJointMapQuickVR(QuickJointType.SHOULDER_RIGHT, QuickVRNode.Type.RightUpperArm);
            CreateJointMapQuickVR(QuickJointType.ELBOW_RIGHT, QuickVRNode.Type.RightLowerArm);
            CreateJointMapQuickVR(QuickJointType.HAND_RIGHT, QuickVRNode.Type.RightHand);

            //Left Leg Mapping
            CreateJointMapQuickVR(QuickJointType.HIP_LEFT, QuickVRNode.Type.LeftUpperLeg);
            CreateJointMapQuickVR(QuickJointType.KNEE_LEFT, QuickVRNode.Type.LeftLowerLeg);
            CreateJointMapQuickVR(QuickJointType.ANKLE_LEFT, QuickVRNode.Type.LeftFoot);

            //Right Leg Mapping
            CreateJointMapQuickVR(QuickJointType.HIP_RIGHT, QuickVRNode.Type.RightUpperLeg);
            CreateJointMapQuickVR(QuickJointType.KNEE_RIGHT, QuickVRNode.Type.RightLowerLeg);
            CreateJointMapQuickVR(QuickJointType.ANKLE_RIGHT, QuickVRNode.Type.RightFoot);
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

        protected virtual void ConfigureVRExtraTrackersHMD(List<QuickExtraTracker> extraTrackers)
        {
            XRNodeState sHead = new XRNodeState();
            XRNodeState sLeftHand = new XRNodeState();
            XRNodeState sRightHand = new XRNodeState();
            sHead.uniqueID = sLeftHand.uniqueID = sRightHand.uniqueID = 0;

            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (!s.tracked) continue;

                if (s.nodeType == XRNode.Head) sHead = s;
                else if (s.nodeType == XRNode.LeftHand) sLeftHand = s;
                else if (s.nodeType == XRNode.RightHand) sRightHand = s;
            }

            if (sLeftHand.uniqueID != 0 && sRightHand.uniqueID != 0)
            {
                GetQuickVRNode(QuickVRNode.Type.Head).SetID(sHead.uniqueID);
                GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(sLeftHand.uniqueID);
                GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(sRightHand.uniqueID);

                if (extraTrackers.Count() == 0) return;

                //numTrackers == 1 => Hips
                int hipsIndex = GetHipsIndex(extraTrackers);
                GetQuickVRNode(QuickVRNode.Type.Hips).SetID(extraTrackers[hipsIndex].Key.uniqueID);
                extraTrackers.RemoveAt(hipsIndex);
                if (extraTrackers.Count() == 0) return;

                //numTrackers == 2 => ... + LeftFoot
                GetQuickVRNode(QuickVRNode.Type.LeftFoot).SetID(extraTrackers[0].Key.uniqueID);
                extraTrackers.RemoveAt(0);
                if (extraTrackers.Count() == 0) return;

                //numTrackers == 3 => ... + RightFoot
                GetQuickVRNode(QuickVRNode.Type.RightFoot).SetID(extraTrackers[0].Key.uniqueID);
                extraTrackers.RemoveAt(0);
                if (extraTrackers.Count() == 0) return;

                //numTrackers == 4 => ... + LeftLowerArm
                GetQuickVRNode(QuickVRNode.Type.LeftLowerArm).SetID(extraTrackers[0].Key.uniqueID);
                extraTrackers.RemoveAt(0);
                if (extraTrackers.Count() == 0) return;

                //numTrackers == 5 => ... + RightLowerArm
                GetQuickVRNode(QuickVRNode.Type.RightLowerArm).SetID(extraTrackers[0].Key.uniqueID);
                extraTrackers.RemoveAt(0);
                if (extraTrackers.Count() == 0) return;

                //numTrackers == 6 => ... + LeftLowerLeg
                GetQuickVRNode(QuickVRNode.Type.LeftLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
                SwapQuickVRNode(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.LeftLowerLeg);
                extraTrackers.RemoveAt(0);
                if (extraTrackers.Count() == 0) return;

                //numTrackers == 7 => ... + RightLowerLeg
                GetQuickVRNode(QuickVRNode.Type.RightLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
                SwapQuickVRNode(QuickVRNode.Type.RightLowerArm, QuickVRNode.Type.RightLowerLeg);
                extraTrackers.RemoveAt(0);
                if (extraTrackers.Count() == 0) return;
            }
            else
            {
                //Add the HMD Node to the end of the list and make the auto-assignation
                //as in the case where NoHMD is detected.
                Vector3 pos;
                sHead.TryGetPosition(out pos);
                extraTrackers.Add(new QuickExtraTracker(sHead, pos));

                ConfigureVRExtraTrackersNoHMD(extraTrackers);
            }
        }

        protected virtual void ConfigureVRExtraTrackersNoHMD(List<QuickExtraTracker> extraTrackers)
        {
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 1 => Head
            //The head is always considered to be the tracker on the top. 
            GetQuickVRNode(QuickVRNode.Type.Head).SetID(extraTrackers[extraTrackers.Count() - 1].Key.uniqueID);
            extraTrackers.RemoveAt(extraTrackers.Count() - 1);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 2 => ... + Hips
            int HipsIndex = GetHipsIndex(extraTrackers);
            GetQuickVRNode(QuickVRNode.Type.Hips).SetID(extraTrackers[HipsIndex].Key.uniqueID);
            extraTrackers.RemoveAt(HipsIndex);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 3 => ... + LeftHand
            GetQuickVRNode(QuickVRNode.Type.LeftHand).SetID(extraTrackers[0].Key.uniqueID);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 4 => ... + RightHand
            GetQuickVRNode(QuickVRNode.Type.RightHand).SetID(extraTrackers[0].Key.uniqueID);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 5 => ... + LeftFoot
            GetQuickVRNode(QuickVRNode.Type.LeftFoot).SetID(extraTrackers[0].Key.uniqueID);
            SwapQuickVRNode(QuickVRNode.Type.LeftHand, QuickVRNode.Type.LeftFoot);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 6 => ... + RightFoot
            GetQuickVRNode(QuickVRNode.Type.RightFoot).SetID(extraTrackers[0].Key.uniqueID);
            SwapQuickVRNode(QuickVRNode.Type.RightHand, QuickVRNode.Type.RightFoot);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 7 => ... + LeftLowerArm
            GetQuickVRNode(QuickVRNode.Type.LeftLowerArm).SetID(extraTrackers[0].Key.uniqueID);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 8 => ... + RightLowerArm
            GetQuickVRNode(QuickVRNode.Type.RightLowerArm).SetID(extraTrackers[0].Key.uniqueID);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 9 => ... + LeftLowerLeg
            GetQuickVRNode(QuickVRNode.Type.LeftLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
            SwapQuickVRNode(QuickVRNode.Type.LeftHand, QuickVRNode.Type.LeftLowerArm);
            SwapQuickVRNode(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.LeftLowerLeg);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;

            //numTrackers == 10 => ... + RightLowerLeg
            GetQuickVRNode(QuickVRNode.Type.RightLowerLeg).SetID(extraTrackers[0].Key.uniqueID);
            SwapQuickVRNode(QuickVRNode.Type.RightHand, QuickVRNode.Type.RightLowerArm);
            SwapQuickVRNode(QuickVRNode.Type.RightLowerArm, QuickVRNode.Type.RightLowerLeg);
            extraTrackers.RemoveAt(0);
            if (extraTrackers.Count() == 0) return;
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

        protected virtual void SwapQuickVRNode(QuickVRNode.Type typeA, QuickVRNode.Type typeB)
        {
            SwapQuickVRNode(GetQuickVRNode(typeA), GetQuickVRNode(typeB));
        }

        protected virtual void SwapQuickVRNode(QuickVRNode vrNodeA, QuickVRNode vrNodeB)
        {
            ulong tmp = vrNodeA.GetID();
            vrNodeA.SetID(vrNodeB.GetID());
            vrNodeB.SetID(tmp);
        }

        public override void Calibrate()
        {
            base.Calibrate();

            _ikManager.Calibrate();

            CalibrateVRNodes();
        }

        protected virtual void CalibrateVRNodes()
        {
            //Reset the IDs of the VRNodes
            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList()) GetQuickVRNode(t).SetID(0);

            //Get all the tracked extratrackers and sort them by Y, from lowest to higher. 
            List<QuickExtraTracker> extraTrackers = GetExtraTrackers();
            extraTrackers = extraTrackers.OrderBy(o => o.Value.y).ToList();

            Debug.Log("NUM EXTRA TRACKERS = " + extraTrackers.Count());

            if (IsHMDPresent()) ConfigureVRExtraTrackersHMD(extraTrackers);
            else ConfigureVRExtraTrackersNoHMD(extraTrackers);

            //Check if left/right nodes are set in the correct side, and swap them if necessary. 
            _handsSwaped = IsVRNodesSwaped(QuickVRNode.Type.LeftHand, QuickVRNode.Type.RightHand);
            IsVRNodesSwaped(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.RightLowerArm);

            IsVRNodesSwaped(QuickVRNode.Type.LeftFoot, QuickVRNode.Type.RightFoot);
            IsVRNodesSwaped(QuickVRNode.Type.LeftLowerLeg, QuickVRNode.Type.RightLowerLeg);

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
                float sign = nodeType == QuickVRNode.Type.LeftHand ? 1.0f : -1.0f;
                tObject.transform.Rotate(tObject.transform.forward, sign * 90.0f, Space.World);

                if (IsExtraTracker(node.GetID()))
                {
                    tObject.transform.Rotate(tObject.transform.right, 90.0f, Space.World);
                }
            }
            else if (nodeType == QuickVRNode.Type.LeftFoot || nodeType == QuickVRNode.Type.RightFoot)
            {
                tObject.transform.rotation = _vrNodesOrigin.rotation;
            }

        }

        protected virtual void CalibrateVRNodeHead(QuickVRNode node)
        {
            QuickTrackedObject tObject = node.GetTrackedObject();

            Transform tHead = _animator.GetBoneTransform(HumanBodyBones.Head);
            tObject.transform.localPosition = Quaternion.Inverse(transform.rotation) * (tHead.position - _animator.GetEyeCenterPosition());
            _vrNodesOrigin.position = tObject.transform.position - transform.up * GetHeadHeight();

            _vrNodesOrigin.forward = Vector3.ProjectOnPlane(node.transform.forward, transform.up);
            _footprints.rotation = ToAvatarSpace(_vrNodesOrigin.rotation);
        }

        protected virtual float GetHeadHeight()
        {
            return _unscaledHeadHeight * (1.0f / transform.lossyScale.y);
        }

        public virtual QuickVRNode GetQuickVRNode(QuickVRNode.Type node)
        {
            return _vrNodesRoot ? _vrNodesRoot.Find(node.ToString()).GetComponent<QuickVRNode>() : null;
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

        protected override bool IsTrackingDataWorldSpace()
        {
            return true;
        }

        protected override bool IsTrackingConnected()
        {
            return true;
        }

        protected override Vector3 GetTrackingJointPosition(QuickVRNode.Type jointID)
        {
            return GetQuickVRNode(jointID).GetTrackedObject().transform.position;
        }

        protected override Quaternion GetTrackingJointRotation(QuickVRNode.Type jointID)
        {
            return GetQuickVRNode(jointID).GetTrackedObject().transform.rotation;
        }

        protected override QuickJoint.State GetTrackingJointState(QuickVRNode.Type jointID)
        {
            QuickJoint.State state = base.GetTrackingJointState(jointID);
            if (state != QuickJoint.State.UNTRACKED)
            {
                if 
                (
                    jointID == QuickVRNode.Type.LeftUpperArm ||
                    jointID == QuickVRNode.Type.RightUpperArm ||
                    jointID == QuickVRNode.Type.LeftUpperLeg ||
                    jointID == QuickVRNode.Type.RightUpperLeg
                )
                {
                    state = QuickJoint.State.INFERRED;
                }
                else
                {
                    state = GetQuickVRNode(jointID).IsTracked() ? QuickJoint.State.TRACKED : QuickJoint.State.UNTRACKED;
                }
            }

            return state;            
        }

        #endregion

        #region UPDATE

        public override void UpdateTracking()
        {
            UpdateQuickVRNodeIDs();

            //UpdateTransformRoot();
            UpdateTransformNodes();

            base.UpdateTracking();

            UpdateTrackingIK();

            UpdateFootPrints();
        }

        protected virtual void UpdateQuickVRNodeIDs()
        {
            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (!s.tracked) continue;

                if (IsHMDPresent() && (s.nodeType == XRNode.Head)) GetQuickVRNode(QuickVRNode.Type.Head).SetID(s.uniqueID);
                else if (s.nodeType == XRNode.LeftHand) GetQuickVRNode(!_handsSwaped ? QuickVRNode.Type.LeftHand : QuickVRNode.Type.RightHand).SetID(s.uniqueID);
                else if (s.nodeType == XRNode.RightHand) GetQuickVRNode(!_handsSwaped ? QuickVRNode.Type.RightHand : QuickVRNode.Type.LeftHand).SetID(s.uniqueID);
            }
        }

        protected virtual void UpdateTransformNodes()
        {
            QuickTrackedObject tHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            GetQuickVRNode(QuickVRNode.Type.LeftUpperArm).transform.position = tHead.transform.position - _vrNodesOrigin.right * 0.15f;
            GetQuickVRNode(QuickVRNode.Type.RightUpperArm).transform.position = tHead.transform.position + _vrNodesOrigin.right * 0.15f;

            QuickTrackedObject tHips = GetQuickVRNode(QuickVRNode.Type.Hips).GetTrackedObject();
            GetQuickVRNode(QuickVRNode.Type.LeftUpperLeg).transform.position = tHips.transform.position - _vrNodesOrigin.right * 0.10f;
            GetQuickVRNode(QuickVRNode.Type.RightUpperLeg).transform.position = tHips.transform.position + _vrNodesOrigin.right * 0.10f;

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                UpdateTransformNode(t);
            }
        }

        protected virtual void UpdateTransformNode(QuickVRNode.Type nType)
        {
            QuickVRNode node = GetQuickVRNode(nType);
            if (!node.IsTracked()) return;

            HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(nType.ToString());
            QuickTrackedObject tObject = node.GetTrackedObject();
            Vector3 posOffset = tObject.transform.position - _vrNodesOrigin.position;

            QuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
            if (!ikSolver) return;

            Transform t = null;
            if (QuickIKManager.IsBoneLimb(boneID))
            {
                t = ikSolver._targetLimb;
            }
            else if (QuickIKManager.IsBoneMid(boneID))
            {
                t = ikSolver._targetHint;
            }

            if (!t) return;

            t.position = transform.position + ToAvatarSpace(posOffset);
            t.rotation = ToAvatarSpace(tObject.transform.rotation);
        }

        protected virtual void UpdateFootPrints()
        {
            _footprints.position = transform.position - _userDisplacement;
        }

        protected virtual void UpdateTrackingIK()
        {
            if (GetQuickVRNode(QuickVRNode.Type.LeftLowerArm).IsTracked())
            {
                _ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftHand);
            }
            if (GetQuickVRNode(QuickVRNode.Type.RightLowerArm).IsTracked())
            {
                _ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightHand);
            }
            if (GetQuickVRNode(QuickVRNode.Type.LeftLowerLeg).IsTracked())
            {
                _ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            }
            if (GetQuickVRNode(QuickVRNode.Type.RightLowerLeg).IsTracked())
            {
                _ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
            }

            _ikManager.UpdateTracking();
        }

        #endregion

        #region DEBUG

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Application.isPlaying)
            {
                DebugVRNodesOrigin();

                //DebugVRNode(QuickVRNode.Type.TrackingReference, Color.yellow);

                DebugVRNode(QuickVRNode.Type.Head, Color.grey);

                DebugVRNode(QuickVRNode.Type.LeftUpperArm, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.LeftLowerArm, new Color(0.0f, 0.5f, 0.5f));
                DebugVRNode(QuickVRNode.Type.LeftHand, Color.blue);

                DebugVRNode(QuickVRNode.Type.RightUpperArm, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.RightLowerArm, new Color(1.0f, 0.0f, 0.5f));
                DebugVRNode(QuickVRNode.Type.RightHand, Color.red);

                DebugVRNode(QuickVRNode.Type.Hips, Color.black, true);

                DebugVRNode(QuickVRNode.Type.LeftUpperLeg, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.LeftLowerLeg, new Color(0.0f, 1.0f, 0.5f));
                DebugVRNode(QuickVRNode.Type.LeftFoot, Color.cyan);

                DebugVRNode(QuickVRNode.Type.RightUpperLeg, Color.yellow, true);
                DebugVRNode(QuickVRNode.Type.RightLowerLeg, new Color(1.0f, 0.5f, 0.0f));
                DebugVRNode(QuickVRNode.Type.RightFoot, Color.magenta);

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.LeftUpperArm, true);
                if (GetQuickVRNode(QuickVRNode.Type.LeftLowerArm).IsTracked())
                {
                    DebugVRNodeConnection(QuickVRNode.Type.LeftUpperArm, QuickVRNode.Type.LeftLowerArm, true);
                    DebugVRNodeConnection(QuickVRNode.Type.LeftLowerArm, QuickVRNode.Type.LeftHand);
                }
                else DebugVRNodeConnection(QuickVRNode.Type.LeftUpperArm, QuickVRNode.Type.LeftHand, GetQuickVRNode(QuickVRNode.Type.LeftHand).IsTracked());

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.RightUpperArm, true);
                if (GetQuickVRNode(QuickVRNode.Type.RightLowerArm).IsTracked())
                {
                    DebugVRNodeConnection(QuickVRNode.Type.RightUpperArm, QuickVRNode.Type.RightLowerArm, true);
                    DebugVRNodeConnection(QuickVRNode.Type.RightLowerArm, QuickVRNode.Type.RightHand);
                }
                else DebugVRNodeConnection(QuickVRNode.Type.RightUpperArm, QuickVRNode.Type.RightHand, GetQuickVRNode(QuickVRNode.Type.RightHand).IsTracked());

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.Hips, true);

                DebugVRNodeConnection(QuickVRNode.Type.Hips, QuickVRNode.Type.LeftUpperLeg, true);
                if (GetQuickVRNode(QuickVRNode.Type.LeftLowerLeg).IsTracked())
                {
                    DebugVRNodeConnection(QuickVRNode.Type.LeftUpperLeg, QuickVRNode.Type.LeftLowerLeg, true);
                    DebugVRNodeConnection(QuickVRNode.Type.LeftLowerLeg, QuickVRNode.Type.LeftFoot);
                }
                else DebugVRNodeConnection(QuickVRNode.Type.LeftUpperLeg, QuickVRNode.Type.LeftFoot, GetQuickVRNode(QuickVRNode.Type.LeftFoot).IsTracked());

                DebugVRNodeConnection(QuickVRNode.Type.Hips, QuickVRNode.Type.RightUpperLeg, true);
                if (GetQuickVRNode(QuickVRNode.Type.RightLowerLeg).IsTracked())
                {
                    DebugVRNodeConnection(QuickVRNode.Type.RightUpperLeg, QuickVRNode.Type.RightLowerLeg, true);
                    DebugVRNodeConnection(QuickVRNode.Type.RightLowerLeg, QuickVRNode.Type.RightFoot);
                }
                else DebugVRNodeConnection(QuickVRNode.Type.RightUpperLeg, QuickVRNode.Type.RightFoot, GetQuickVRNode(QuickVRNode.Type.RightFoot).IsTracked());

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


