﻿using UnityEngine;
using UnityEngine.Animations;

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

#if UNITY_EDITOR

        [SerializeField, HideInInspector]
        public bool _showControlsBody = false;

        [SerializeField, HideInInspector]
        public bool _showControlsFingersLeftHand = false;

        [SerializeField, HideInInspector]
        public bool _showControlsFingersRightHand = false;

#endif

#endregion

        #region PROTECTED PARAMETERS

        protected QuickVRPlayArea _vrPlayArea = null;

        protected Vector3 _headOffset = Vector3.zero;

        protected PositionConstraint _footprints = null;

        protected QuickVRHand _vrHandLeft = null;
        protected QuickVRHand _vrHandRight = null;

        protected List<ControlType> _controlsBody
        {
            get
            {
                if (m_ControlsBody == null || m_ControlsBody.Count == 0)
                {
                    m_ControlsBody = new List<ControlType>();
                    foreach (HumanBodyBones boneID in GetIKLimbBones())
                    {
                        m_ControlsBody.Add(ControlType.Tracking);
                    }
                }

                return m_ControlsBody;
            }
        }

        [SerializeField, HideInInspector]
        protected List<ControlType> m_ControlsBody = null;

        protected List<ControlType> _controlsFingersLeftHand
        {
            get
            {
                if (m_ControlsFingersLeftHand == null || m_ControlsFingersLeftHand.Count != 5)
                {
                    m_ControlsFingersLeftHand = new List<ControlType>();
                    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                    {
                        m_ControlsFingersLeftHand.Add(ControlType.Tracking);
                    }
                }

                return m_ControlsFingersLeftHand;
            }
        }

        [SerializeField, HideInInspector]
        protected List<ControlType> m_ControlsFingersLeftHand = null;

        protected List<ControlType> _controlsFingersRightHand
        {
            get
            {
                if (m_ControlsFingersRightHand == null || m_ControlsFingersRightHand.Count != 5)
                {
                    m_ControlsFingersRightHand = new List<ControlType>();
                    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                    {
                        m_ControlsFingersRightHand.Add(ControlType.Tracking);
                    }
                }

                return m_ControlsFingersRightHand;
            }
        }

        [SerializeField, HideInInspector]
        protected List<ControlType> m_ControlsFingersRightHand = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            if (Application.isPlaying)
            {
                base.OnEnable();

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

        public virtual ControlType GetControlBody(HumanBodyBones boneID)
        {
            ControlType cType = ControlType.Tracking;
            int i = GetIKLimbBones().IndexOf(boneID);
            if (i != -1)
            {
                cType = _controlsBody[i];
            }

            return cType;
        }

        public virtual void SetControlBody(HumanBodyBones boneID, ControlType cType)
        {
            int i = GetIKLimbBones().IndexOf(boneID);
            if (i != -1)
            {
                _controlsBody[i] = cType;
            }
        }

        public virtual ControlType GetControlFinger(QuickHumanFingers f, bool isLeft)
        {
            int i = (int)f;
            return isLeft ? _controlsFingersLeftHand[i] : _controlsFingersRightHand[i];
        }

        public virtual void SetControlFinger(QuickHumanFingers f, bool isLeft, ControlType cType)
        {
            int i = (int)f;
            if (isLeft)
            {
                _controlsFingersLeftHand[i] = cType;
            }
            else
            {
                _controlsFingersRightHand[i] = cType;
            }
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
            Vector3 offset = GetIKSolver(HumanBodyBones.Head)._targetLimb.position - node.GetTrackedObject().transform.position;
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
            if (Application.isPlaying)
            {
                return _vrPlayArea.GetVRNode(HumanBodyBones.Head).transform.position - _animator.GetEyeCenterPosition();
            }

            return Vector3.zero;
        }

        #endregion

        #region UPDATE

        protected override void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                base.UpdateTracking();
            }
        }

        public override void UpdateTracking()
        {
            //1) Update all the IKTargets taking into consideration its ControlType. 
            _ikMaskBody = -1;
            List<HumanBodyBones> ikLimbBones = GetIKLimbBones();
            for (int i = 0; i < ikLimbBones.Count; i++)
            {
                HumanBodyBones boneID = ikLimbBones[i];
                ControlType cType = GetControlBody(boneID);
                if (cType == ControlType.Animation)
                {
                    //This body limb is controlled by the Animation. So disable the IK
                    _ikMaskBody &= ~(1 << i);
                }
                else if (cType == ControlType.Tracking)
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
                    }
                }
            }

            //2) Special case: There is no tracker on the hips. So the hips position is estimated by the movement of the head
            QuickVRNode nodeHips = _vrPlayArea.GetVRNode(HumanBodyBones.Hips);
            if (!nodeHips.IsTracked())
            {
                QuickVRNode vrNode = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
                UpdateIKTargetPosFromCalibrationPose(vrNode, HumanBodyBones.Hips, Vector3.up);
                float maxY = GetIKSolver(HumanBodyBones.Hips).GetInitialLocalPosTargetLimb().y;
                Transform targetHips = GetIKSolver(HumanBodyBones.Hips)._targetLimb;
                targetHips.localPosition = new Vector3(targetHips.localPosition.x, Mathf.Min(targetHips.localPosition.y, maxY), targetHips.localPosition.z);
            }

            UpdateVRCursors();
            _footprints.gameObject.SetActive(_useFootprints);

            UpdateTrackingFingers(true);
            UpdateTrackingFingers(false);

            base.UpdateTracking();
        }

        protected virtual void UpdateTrackingFingers(bool isLeft)
        {
            //Apply the rotation to the bones
            QuickHumanFingers[] fingers = QuickHumanTrait.GetHumanFingers();
            for (int i = 0; i < fingers.Length; i++)
            {
                QuickHumanFingers f = fingers[i];
                ControlType cType = GetControlFinger(f, isLeft);
                int m = (1 << i);
                if (cType == ControlType.IK)
                {
                    //If this finger is driven by the IK, activate the corresponding position in the mask
                    if (isLeft)
                    {
                        _ikMaskLeftHand |= m;
                    }
                    else
                    {
                        _ikMaskRightHand |= m;
                    }
                }
                else
                {
                    //Otherwise, deactivate the corresponding position in the mask
                    if (isLeft)
                    {
                        _ikMaskLeftHand &= ~m;
                    }
                    else
                    {
                        _ikMaskRightHand &= ~m;
                    }

                    if (cType == ControlType.Tracking)
                    {
                        //Apply the tracking data to this finger
                        List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
                        for (int j = 0; j < fingerBones.Count - 1; j++)
                        {
                            QuickVRNode node = _vrPlayArea.GetVRNode(fingerBones[j]);
                            if (node.IsTracked())
                            {
                                UpdateTrackingFingerPhalange(fingerBones[j], fingerBones[j + 1]);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void UpdateTrackingFingerPhalange(QuickHumanBodyBones boneStartID, QuickHumanBodyBones boneEndID)
        {
            Transform bone0 = _animator.GetBoneTransform(boneStartID);
            Transform bone1 = _animator.GetBoneTransform(boneEndID);
            Transform node0 = _vrPlayArea.GetVRNode(boneStartID).transform;
            Transform node1 = _vrPlayArea.GetVRNode(boneEndID).transform;

            if (bone0 && bone1 && node0 && node1)
            {
                Vector3 currentDir = bone1.position - bone0.position;
                Vector3 targetDir = node1.position - node0.position;
                float rotAngle = Vector3.Angle(currentDir, targetDir);
                Vector3 rotAxis = Vector3.Cross(currentDir, targetDir).normalized;

                bone0.Rotate(rotAxis, rotAngle, Space.World);
            }
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
