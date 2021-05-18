﻿using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

using System.Collections.Generic;

namespace QuickVR {

    public class QuickUnityVR : QuickIKManager
    {

        #region CONSTANTS

        protected static float HUMAN_HEADS_TALL = 7.5f;
        protected static float HUMAN_HEADS_TALL_EYES = HUMAN_HEADS_TALL - 0.5f;
        protected static float HUMAN_HEADS_TALL_HEAD = HUMAN_HEADS_TALL - 1.0f;

        public static Vector3 HAND_CONTROLLER_POSITION_OFFSET = new Vector3(0, 0, -0.1f);

        #endregion

        #region PUBLIC ATTRIBUTES

        public bool _useFootprints = true;

        public static bool _handsSwaped = false;

        public enum HandTrackingMode
        {
            Controllers,
            Hands,
        }
        public HandTrackingMode _handTrackingMode = HandTrackingMode.Hands;

        public enum ControlType
        {
            Tracking,
            Animation,
            IK,
        }

        public bool _applyHeadRotation
        {
            get
            {
                return m_ApplyHeadRotation;
            }
            set
            {
                if (value != m_ApplyHeadRotation)
                {
                    GetIKSolver(IKBone.Head)._weightIKRot = value ? 1 : 0;

                    m_ApplyHeadRotation = value;
                }
            }
        }
        [SerializeField, HideInInspector]
        protected bool m_ApplyHeadRotation = true;

        public bool _applyHeadPosition
        {
            get
            {
                return m_ApplyHeadPosition;
            }
            set
            {
                if (value != m_ApplyHeadPosition)
                {
                    GetIKSolver(IKBone.Head)._weightIKPos = value ? 1 : 0;

                    m_ApplyHeadPosition = value;
                }
            }
        }
        [SerializeField, HideInInspector]
        protected bool m_ApplyHeadPosition = true;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickVRPlayArea _vrPlayArea = null;

        protected Vector3 _headOffset = Vector3.zero;

        protected PositionConstraint _footprints = null;

        protected QuickVRHand _vrHandLeft = null;
        protected QuickVRHand _vrHandRight = null;

        protected List<ControlType> _ikControls
        {
            get
            {
                if (m_IKControls == null || m_IKControls.Count == 0)
                {
                    m_IKControls = new List<ControlType>();
                    for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
                    {
                        m_IKControls.Add(ControlType.Tracking);
                    }
                }

                return m_IKControls;
            }
        }
        [SerializeField, HideInInspector]
        protected List<ControlType> m_IKControls;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Application.isPlaying)
            {
                QuickVRNode.OnCalibrateVRNodeHead += OnCalibrateVRNodeHead;
                QuickVRNode.OnCalibrateVRNodeHips += OnCalibrateVRNodeHips;
                QuickVRNode.OnCalibrateVRNodeLeftHand += OnCalibrateVRNodeLeftHand;
                QuickVRNode.OnCalibrateVRNodeRightHand += OnCalibrateVRNodeRightHand;
                QuickVRNode.OnCalibrateVRNodeLeftFoot += OnCalibrateVRNodeFoot;
                QuickVRNode.OnCalibrateVRNodeRightFoot += OnCalibrateVRNodeFoot;
            }
        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                QuickVRNode.OnCalibrateVRNodeHead -= OnCalibrateVRNodeHead;
                QuickVRNode.OnCalibrateVRNodeHips -= OnCalibrateVRNodeHips;
                QuickVRNode.OnCalibrateVRNodeLeftHand -= OnCalibrateVRNodeLeftHand;
                QuickVRNode.OnCalibrateVRNodeRightHand -= OnCalibrateVRNodeRightHand;
                QuickVRNode.OnCalibrateVRNodeLeftFoot -= OnCalibrateVRNodeFoot;
                QuickVRNode.OnCalibrateVRNodeRightFoot -= OnCalibrateVRNodeFoot;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (Application.isPlaying)
            {
                _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
                _vrPlayArea.transform.parent = transform;

                CreateVRHands();
                CreateVRCursors();
                CreateFootPrints();
            }
        }

        protected virtual void Start()
        {
            _headOffset = Quaternion.Inverse(transform.rotation) * (_animator.GetBoneTransform(HumanBodyBones.Head).position - _animator.GetEyeCenterPosition());

            if (Application.isPlaying)
            {
                CheckHandtrackingMode();

                _vrManager.AddUnityVRTrackingSystem(this);
            }
        }

        protected virtual void CreateFootPrints()
        {
            _footprints = Instantiate<GameObject>(Resources.Load<GameObject>("Footprints/Footprints")).GetOrCreateComponent<PositionConstraint>();
            _footprints.transform.ResetTransformation();
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = transform;
            source.weight = 1.0f;
            _footprints.AddSource(source);
            _footprints.constraintActive = true;
        }

        protected virtual void CreateVRCursors()
        {
            CreateVRCursorHand(QuickUICursor.Role.LeftHand, _vrHandLeft._handBone, _vrHandLeft._handBoneIndexDistal);
            CreateVRCursorHand(QuickUICursor.Role.RightHand, _vrHandRight._handBone, _vrHandRight._handBoneIndexDistal);
        }

        protected virtual void CreateVRCursorHand(QuickUICursor.Role cType, Transform tHand, Transform tDistal)
        {
            Transform tIntermediate = tDistal.parent;
            Transform tProximal = tIntermediate.parent;
            float l1 = Vector3.Distance(tDistal.position, tIntermediate.position);
            float l2 = Vector3.Distance(tIntermediate.position, tProximal.position);
            Transform cursorOrigin = tHand.CreateChild("__CursorOrigin__");
            cursorOrigin.forward = (tIntermediate.position - tProximal.position).normalized;
            cursorOrigin.position = tProximal.position + cursorOrigin.forward * (l1 + l2 + (l2 - l1));

            QuickUICursor.CreateVRCursor(cType, cursorOrigin);
        }

        protected virtual void CreateVRHands()
        {
            _vrHandLeft = gameObject.AddComponent<QuickVRHand>();
            _vrHandLeft._handBone = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _vrHandLeft._handBoneIndexDistal = _animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

            _vrHandRight = gameObject.AddComponent<QuickVRHand>();
            _vrHandRight._handBone = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _vrHandRight._handBoneIndexDistal = _animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

            if (_vrHandLeft._axisAnim == "") _vrHandLeft._axisAnim = "LeftTrigger";
            if (_vrHandRight._axisAnim == "") _vrHandRight._axisAnim = "RightTrigger";
        }

        #endregion

        #region GET AND SET

        public virtual ControlType GetIKControl(IKBone ikBone)
        {
            return _ikControls[(int)ikBone];
        }

        public virtual void SetIKControl(IKBone ikBone, ControlType cType)
        {
            _ikControls[(int)ikBone] = cType;
        }

        public virtual void CheckHandtrackingMode()
        {
            if (_handTrackingMode == HandTrackingMode.Hands && !QuickVRManager.IsHandTrackingSupported())
            {
                _handTrackingMode = HandTrackingMode.Controllers;
            }
        }

        public virtual int GetNumExtraTrackers()
        {
            return 0;
        }

        public virtual Vector3 GetDisplacement()
        {
            //if (_isStanding)
            //{
            //    QuickVRNode hipsNode = _vrPlayArea.GetVRNode(QuickVRNode.Type.Hips);
            //    if (_vrPlayArea.IsTrackedNode(hipsNode)) return hipsNode.GetTrackedObject().GetDisplacement();
            //    else if (_displaceWithCamera) return _vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject().GetDisplacement();
            //}

            QuickVRNode n = _vrPlayArea.GetVRNodeMain();
            Vector3 offset = n.GetTrackedObject().transform.position - n.GetCalibrationPose().position;

            return Vector3.Scale(offset, Vector3.right + Vector3.forward);
        }

        protected virtual float GetRotationOffset()
        {
            Vector3 userForward = _vrPlayArea.GetUserForward();
            return Vector3.SignedAngle(transform.forward, userForward, transform.up);
        }

        public override void Calibrate()
        {
            base.Calibrate();

            transform.localScale = Vector3.one;

            _footprints.translationOffset = Vector3.zero;
            _footprints.transform.rotation = transform.rotation;

            _vrPlayArea.Calibrate();

            float rotAngle = Vector3.SignedAngle(_vrPlayArea.GetUserForward(), transform.forward, transform.up);
            _vrPlayArea.transform.Rotate(transform.up, rotAngle, Space.World);

            //Set the offset of the TrackedObject of the head
            QuickVRNode node = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
            Vector3 offset = GetIKSolver(IKBone.Head)._targetLimb.position - node.GetTrackedObject().transform.position;
            _vrPlayArea.transform.position += offset;

            //Update eyeCenter
            Transform tEyeCenter = _animator.GetEyeCenter();
            tEyeCenter.rotation = transform.rotation;
            tEyeCenter.position = Vector3.Lerp(_animator.GetEye(true).position, _animator.GetEye(false).position, 0.5f);
        }

        protected virtual void OnCalibrateVRNodeHead(QuickVRNode node)
        {
            node.GetTrackedObject().transform.localPosition = _headOffset;
        }

        protected virtual void OnCalibrateVRNodeHips(QuickVRNode node)
        {
            QuickTrackedObject tObjectHead = _vrPlayArea.GetVRNode(HumanBodyBones.Head).GetTrackedObject();
            QuickTrackedObject tObjectHips = node.GetTrackedObject();
            tObjectHips.transform.position = new Vector3(tObjectHead.transform.position.x, tObjectHips.transform.position.y, tObjectHead.transform.position.z);
        }

        protected virtual void OnCalibrateVRNodeLeftHand(QuickVRNode node)
        {
            QuickTrackedObject tObject = node.GetTrackedObject();
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

        protected virtual void OnCalibrateVRNodeRightHand(QuickVRNode node)
        {
            QuickTrackedObject tObject = node.GetTrackedObject();
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

        protected virtual void OnCalibrateVRNodeFoot(QuickVRNode node)
        {
            Transform ikTarget = GetIKSolver((HumanBodyBones)node.GetRole())._targetLimb;
            QuickTrackedObject tObject = node.GetTrackedObject();
            tObject.transform.rotation = ikTarget.rotation;
        }

        public virtual QuickVRHand GetVRHand(QuickVRNode.Type nType)
        {
            if (nType == QuickVRNode.Type.LeftHand) return _vrHandLeft;
            if (nType == QuickVRNode.Type.RightHand) return _vrHandRight;

            return null;
        }

        protected override Vector3 GetIKTargetHipsOffset()
        {
            if (Application.isPlaying && _applyHeadPosition)
            {
                return _vrPlayArea.GetVRNode(HumanBodyBones.Head).transform.position - _animator.GetEyeCenterPosition();
            }

            return Vector3.zero;
        }

        #endregion

        #region UPDATE

        public override void UpdateTracking()
        {
            if (Application.isPlaying)
            {
                //1) Update all the IKTargets taking into consideration its ControlType. 
                for (IKBone ikBone = IKBone.Hips; ikBone < IKBone.LastBone; ikBone++)
                {
                    ControlType cType = GetIKControl(ikBone);
                    HumanBodyBones boneID = ToHumanBodyBones(ikBone);
                    GetIKSolver(ikBone)._enableIK = cType != ControlType.Animation;

                    if (cType == ControlType.Tracking)
                    {
                        QuickVRNode node = _vrPlayArea.GetVRNode(boneID);
                        if (node.IsTracked())
                        {
                            //Update the QuickVRNode's position
                            if (node._updateModePos == QuickVRNode.UpdateMode.FromUser) UpdateIKTargetPosFromUser(node, boneID);
                            else UpdateIKTargetPosFromCalibrationPose(node, boneID);

                            //Update the QuickVRNode's rotation
                            if (node._updateModeRot == QuickVRNode.UpdateMode.FromUser) UpdateIKTargetRotFromUser(node, boneID);
                            else UpdateIKTargetRotFromCalibrationPose(node, boneID);

                            if (boneID == HumanBodyBones.Head)
                            {
                                QuickIKSolver ikSolverHead = GetIKSolver(IKBone.Head);
                                if (!_applyHeadPosition)
                                {
                                    ikSolverHead._weightIKPos = 0;
                                }
                                if (!_applyHeadRotation)
                                {
                                    ikSolverHead._weightIKRot = 0;
                                }
                            }
                            else if (boneID == HumanBodyBones.LeftEye || boneID == HumanBodyBones.RightEye)
                            {
                                InputDevice iDevice = node._inputDevice;
                                bool isLeftEye = boneID == HumanBodyBones.LeftEye;
                                if (iDevice.isValid && iDevice.TryGetFeatureValue(isLeftEye ? QuickVRUsages.leftEyeOpenness : QuickVRUsages.rightEyeOpenness, out float eOpen))
                                {
                                    QuickIKSolverEye ikSolver = (QuickIKSolverEye)_animator.GetComponent<QuickIKManager>().GetIKSolver(boneID);
                                    ikSolver._weightBlink = 1.0f - eOpen;
                                }
                            }
                        }
                    }
                }

                //2) Special case: There is no tracker on the hips. So the hips position is estimated by the movement of the head
                QuickVRNode nodeHips = _vrPlayArea.GetVRNode(HumanBodyBones.Hips);
                if (!nodeHips.IsTracked() && _applyHeadPosition)
                {
                    QuickVRNode vrNode = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
                    UpdateIKTargetPosFromCalibrationPose(vrNode, HumanBodyBones.Hips, Vector3.up);
                    float maxY = GetIKSolver(IKBone.Hips).GetInitialLocalPosTargetLimb().y;
                    Transform targetHips = GetIKSolver(IKBone.Hips)._targetLimb;
                    targetHips.localPosition = new Vector3(targetHips.localPosition.x, Mathf.Min(targetHips.localPosition.y, maxY), targetHips.localPosition.z);
                }

                UpdateVRCursors();
                _footprints.gameObject.SetActive(_useFootprints);

                foreach (bool isLeft in new bool[]{ true, false}) 
                {
                    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                    {
                        float fLength = _vrPlayArea.GetFingerLength(f, isLeft);
                        if (fLength > 0)
                        {
                            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
                            QuickVRNode n0 = _vrPlayArea.GetVRNode(fingerBones[0]);
                            QuickVRNode n2 = _vrPlayArea.GetVRNode(fingerBones[2]);

                            QuickIKSolver ikSolver = GetIKSolver((HumanBodyBones)fingerBones[2]);

                            if (n0.IsTracked() && n2.IsTracked())
                            {
                                float sf = ikSolver.GetChainLength() / fLength;
                                Vector3 v = sf * (n2.transform.position - n0.transform.position);

                                Transform ikTarget = ikSolver._targetLimb;
                                ikTarget.position = ikSolver._boneUpper.position + v;
                            }
                        }
                    }
                }
            }

            base.UpdateTracking();
        }

        protected virtual void UpdateIKTargetPosFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            GetIKSolver(boneID)._targetLimb.position = node.GetTrackedObject().transform.position;
        }

        protected virtual void UpdateIKTargetRotFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            GetIKSolver(boneID)._targetLimb.rotation = node.GetTrackedObject().transform.rotation;
        }

        protected virtual void UpdateIKTargetPosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        {
            UpdateIKTargetPosFromCalibrationPose(node, boneID, Vector3.one);
        }

        protected virtual void UpdateIKTargetPosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID, Vector3 offsetScale)
        {
            Transform t = GetIKSolver(boneID)._targetLimb;
            Transform calibrationPose = node.GetCalibrationPose();
            if (!t || !calibrationPose) return;

            t.localPosition = GetIKSolver(boneID).GetInitialLocalPosTargetLimb();
            Vector3 offset = Vector3.Scale(node.GetTrackedObject().transform.position - calibrationPose.position, offsetScale);
            t.position += offset;
        }

        protected virtual void UpdateIKTargetRotFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = GetIKSolver(boneID)._targetLimb;
            Transform calibrationPose = node.GetCalibrationPose();
            if (!t || !calibrationPose) return;

            t.localRotation = GetIKSolver(boneID).GetInitialLocalRotTargetLimb();
            Quaternion rotOffset = node.GetTrackedObject().transform.rotation * Quaternion.Inverse(calibrationPose.rotation);
            t.rotation = rotOffset * t.rotation;
        }

        protected virtual void UpdateVRCursors()
        {
            QuickUICursor.GetVRCursor(QuickUICursor.Role.LeftHand).transform.position = _vrHandLeft._handBoneIndexDistal.position;
            QuickUICursor.GetVRCursor(QuickUICursor.Role.RightHand).transform.position = _vrHandRight._handBoneIndexDistal.position;
        }

        #endregion

    }

}
