﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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

        protected List<XRNodeState> GetExtraTrackers()
        {
            List<XRNodeState> extraTrackers = new List<XRNodeState>();
            foreach (XRNodeState s in GetXRNodeStates())
            {
                if (s.tracked && s.nodeType == XRNode.HardwareTracker) extraTrackers.Add(s);
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
            QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);
            QuickVRNode leftHandNode = GetQuickVRNode(QuickVRNode.Type.LeftHand);
            QuickVRNode rightHandNode = GetQuickVRNode(QuickVRNode.Type.RightHand);

            leftHandNode.SetID(0);
            rightHandNode.SetID(0);

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

            if (!leftHandNode.IsTracked() && !rightHandNode.IsTracked()) return;

            float dLeft = Vector3.Dot(leftHandNode.transform.position - hmdNode.transform.position, hmdNode.transform.right);
            float dRight = Vector3.Dot(rightHandNode.transform.position - hmdNode.transform.position, hmdNode.transform.right);

            if (leftHandNode.IsTracked() && rightHandNode.IsTracked())
            {
                if (dLeft > dRight)
                {
                    SwapQuickVRNode(leftHandNode, rightHandNode);
                }
            } 
            else if (leftHandNode.IsTracked())
            {
                if (dLeft > 0)
                {
                    rightHandNode.SetID(leftHandNode.GetID());
                    leftHandNode.SetID(0);
                }
            }
            else if (rightHandNode.IsTracked())
            {
                if (dRight < 0)
                {
                    leftHandNode.SetID(rightHandNode.GetID());
                    rightHandNode.SetID(0);
                }
            }
        }

        protected virtual void CheckVRExtraTrackers()
        {
            List<XRNodeState> extraTrackers = GetExtraTrackers();
            int numTrackers = extraTrackers.Count;
            QuickVRNode waistNode = GetQuickVRNode(QuickVRNode.Type.Waist);
            QuickVRNode leftFootNode = GetQuickVRNode(QuickVRNode.Type.LeftFoot);
            QuickVRNode rightFootNode = GetQuickVRNode(QuickVRNode.Type.RightFoot);

            waistNode.SetID(0);
            leftFootNode.SetID(0);
            rightFootNode.SetID(0);

            if (numTrackers == 1)
            {
                //We guess the extra tracker is the waist
                waistNode.SetID(extraTrackers[0].uniqueID);
            }
            else if (numTrackers == 2)
            {
                //We guess the extra trackers are the two feet
                leftFootNode.SetID(extraTrackers[0].uniqueID);
                rightFootNode.SetID(extraTrackers[1].uniqueID);
            }
            else if (numTrackers == 3)
            {
                //Set a random assignation
                waistNode.SetID(extraTrackers[0].uniqueID);
                leftFootNode.SetID(extraTrackers[1].uniqueID);
                rightFootNode.SetID(extraTrackers[2].uniqueID);

                //Let's determine which is the waist node
                if (waistNode.transform.position.y < leftFootNode.transform.position.y)
                {
                    SwapQuickVRNode(waistNode, leftFootNode);
                }
                if (waistNode.transform.position.y < rightFootNode.transform.position.y)
                {
                    SwapQuickVRNode(waistNode, rightFootNode);
                }
            }
        }

        protected virtual void CheckVRFeet()
        {
            //Check if left and right feet are swapped
            QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);
            QuickVRNode leftFootNode = GetQuickVRNode(QuickVRNode.Type.LeftFoot);
            QuickVRNode rightFootNode = GetQuickVRNode(QuickVRNode.Type.RightFoot);
            if (
                Vector3.Dot(leftFootNode.transform.position - hmdNode.transform.position, hmdNode.transform.right) >
                Vector3.Dot(rightFootNode.transform.position - hmdNode.transform.position, hmdNode.transform.right))
            {
                SwapQuickVRNode(leftFootNode, rightFootNode);
            }
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
            CheckVRExtraTrackers();
            CheckVRHands();
            CheckVRFeet();

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                CalibrateVRNode(t);
            }
        }

        protected virtual void CalibrateVRNode(QuickVRNode.Type nodeType)
        {
            QuickVRNode node = GetQuickVRNode(nodeType);
            if (!node.IsTracked()) return;

            QuickTrackedObject tObject = node.GetTrackedObject();

            if (nodeType == QuickVRNode.Type.Head)
            {
                CalibrateVRNodeHead(node);
            }

            if (nodeType == QuickVRNode.Type.LeftHand || nodeType == QuickVRNode.Type.RightHand)
            {
                tObject.transform.localPosition = _handControllerPositionOffset;
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

            UpdateTransformRoot();
            UpdateTransformNodes();

            UpdateCameraPosition();

            UpdateFootPrints();

            UpdateVRCursors();
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

        protected abstract void UpdateTransformNodes();

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
                DebugVRNode(QuickVRNode.Type.LeftHand, Color.blue);
                DebugVRNode(QuickVRNode.Type.LeftFoot, Color.cyan);
                DebugVRNode(QuickVRNode.Type.RightHand, Color.red);
                DebugVRNode(QuickVRNode.Type.RightFoot, Color.magenta);
                DebugVRNode(QuickVRNode.Type.Waist, Color.black);

                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.LeftHand);
                DebugVRNodeConnection(QuickVRNode.Type.Head, QuickVRNode.Type.RightHand);

                Gizmos.color = Color.white;
                QuickVRNode.Type tNode = GetQuickVRNode(QuickVRNode.Type.Waist).IsTracked() ? QuickVRNode.Type.Waist : QuickVRNode.Type.Head;
                Gizmos.DrawLine(GetQuickVRNode(tNode).GetTrackedObject().transform.position, _vrNodesOrigin.position);
            }
        }

        protected virtual void DebugVRNodesOrigin()
        {
            DebugExtension.DrawCoordinatesSystem(_vrNodesOrigin.position, _vrNodesOrigin.right, _vrNodesOrigin.up, _vrNodesOrigin.forward, 0.1f);
        }

        protected virtual void DebugVRNode(QuickVRNode.Type nType, Color color, float scale = 0.05f)
        {
            QuickVRNode qNode = GetQuickVRNode(nType);
            if (qNode.IsTracked())
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

        protected virtual void DebugVRNodeConnection(QuickVRNode.Type n1Type, QuickVRNode.Type n2Type)
        {
            QuickVRNode n1 = GetQuickVRNode(n1Type);
            QuickVRNode n2 = GetQuickVRNode(n2Type);
            if (n1.IsTracked() && n2.IsTracked())
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(n1.GetTrackedObject().transform.position, n2.GetTrackedObject().transform.position);
            }
        }

        #endregion

    }

}
