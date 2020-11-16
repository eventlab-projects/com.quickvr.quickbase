using UnityEngine;
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

        public bool _updatePosition = false;
        public bool _updateRotation = false;

        public bool _applyUserScale = false;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickVRPlayArea _vrPlayArea = null;

        protected Transform _calibrationPose = null;

        protected Vector3 _headOffset = Vector3.zero;

        protected PositionConstraint _footprints = null;

        protected Dictionary<VRCursorType, QuickUICursor> _vrCursors = new Dictionary<VRCursorType, QuickUICursor>();

        protected QuickVRHand _vrHandLeft = null;
        protected QuickVRHand _vrHandRight = null;

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

        protected override void Reset()
        {
            base.Reset();

            _animator.CreateEyes();
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

                _calibrationPose = new GameObject("__CalibrationPose__").transform;
                _calibrationPose.position = transform.position;
                _calibrationPose.rotation = transform.rotation;

                _headOffset = Quaternion.Inverse(transform.rotation) * (_animator.GetBoneTransform(HumanBodyBones.Head).position - _animator.GetEyeCenterPosition());
            }
        }

        protected override void RegisterTrackingManager()
        {
            _vrManager.AddUnityVRTrackingSystem(this);
        }

        protected virtual void Start()
        {
            if (Application.isPlaying)
            {
                CheckHandtrackingMode();
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
            //CreateVRCursor(VRCursorType.HEAD, Camera.main.transform);
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

        protected virtual void CreateVRCursor(VRCursorType cType, Transform cTransform)
        {
            QuickUICursor vrCursor = cTransform.gameObject.AddComponent<QuickUICursor>();
            vrCursor._TriggerVirtualKey = InputManager.DEFAULT_BUTTON_CONTINUE;
            vrCursor._drawRay = (cType == VRCursorType.LEFT || cType == VRCursorType.RIGHT);

            _vrCursors[cType] = vrCursor;
            SetVRCursorActive(cType, false);
        }

        #endregion

        #region GET AND SET

        public virtual void CheckHandtrackingMode()
        {
            if (_handTrackingMode == HandTrackingMode.Hands && !QuickUtils.IsHandTrackingSupported())
            {
                _handTrackingMode = HandTrackingMode.Controllers;
            }
        }

        public virtual QuickUICursor GetVRCursor(VRCursorType cType)
        {
            if (!_vrCursors.ContainsKey(cType)) return null;

            return _vrCursors[cType];
        }

        public virtual bool IsVRCursorActive(VRCursorType cType)
        {
            QuickUICursor cursor = GetVRCursor(cType);
            return cursor ? cursor.enabled : false;
        }

        public virtual void SetVRCursorActive(VRCursorType cType, bool active)
        {
            if (!_vrCursors.ContainsKey(cType)) return;

            _vrCursors[cType].enabled = active;
        }

        public virtual int GetNumExtraTrackers()
        {
            return 0;
        }

        public virtual void SetInitialPosition(Vector3 initialPosition)
        {
            _calibrationPose.position = initialPosition;
        }

        public virtual void SetInitialRotation(Quaternion initialRotation)
        {
            _calibrationPose.rotation = initialRotation;
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

            transform.position = _calibrationPose.position;
            transform.rotation = _calibrationPose.rotation;
            _footprints.translationOffset = Vector3.zero;
            _footprints.transform.rotation = transform.rotation;

            _vrPlayArea.Calibrate();

            float rotAngle = Vector3.SignedAngle(_vrPlayArea.GetUserForward(), transform.forward, transform.up);
            _vrPlayArea.transform.Rotate(transform.up, rotAngle, Space.World);

            //Set the offset of the TrackedObject of the head
            QuickVRNode node = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
            Vector3 offset = _animator.GetBoneTransform(HumanBodyBones.Head).position - node.GetTrackedObject().transform.position;
            _vrPlayArea.transform.position += offset;
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
            Transform ikTarget = GetIKTarget(node.GetRole());
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

        public override void UpdateTrackingEarly()
        {
            base.UpdateTrackingEarly();

            if (Application.isPlaying)
            {
                UpdateTransformRoot();
                UpdateTransformNodes();

                UpdateVRCursors();

                _footprints.gameObject.SetActive(_useFootprints);
            }
        }

        protected virtual void UpdateTransformRoot()
        {
            _vrPlayArea.transform.parent = null;

            if (_updateRotation)
            {
                //Update the rotation
                float rotOffset = GetRotationOffset();
                transform.Rotate(transform.up, rotOffset, Space.World);
            }

            if (_updatePosition)
            {
                //Update the position
                Vector3 disp = GetDisplacement();
                transform.Translate(disp, Space.World);
                _vrPlayArea.GetCalibrationPoseRoot().Translate(disp, Space.World);

                _footprints.translationOffset -= disp;
            }

            _vrPlayArea.transform.parent = transform;
        }

        protected virtual void UpdateTransformNodes()
        {
            //1) Update all the nodes but the hips, which has to be treated differently. 
            foreach (HumanBodyBones boneID in QuickVRNode.GetTypeList())
            {
                QuickVRNode node = _vrPlayArea.GetVRNode(boneID);
                if (!node.IsTracked()) continue;
                
                QuickTrackedObject tObject = node.GetTrackedObject();

                //Update the QuickVRNode's position
                if (node._updateModePos == QuickVRNode.UpdateMode.FromUser) UpdateTransformNodePosFromUser(node, boneID);
                else UpdateTransformNodePosFromCalibrationPose(node, boneID);

                //Update the QuickVRNode's rotation
                if (node._updateModeRot == QuickVRNode.UpdateMode.FromUser) UpdateTransformNodeRotFromUser(node, boneID);
                else UpdateTransformNodeRotFromCalibrationPose(node, boneID);
            }

            //2) Special case: There is no tracker on the hips. So the hips position is estimated by the movement of the head
            QuickVRNode nodeHips = _vrPlayArea.GetVRNode(HumanBodyBones.Hips);
            if (!nodeHips.IsTracked())
            {
                QuickVRNode vrNode = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
                UpdateTransformNodePosFromCalibrationPose(vrNode, HumanBodyBones.Hips, Vector3.up);
                float maxY = GetIKSolver(HumanBodyBones.Hips).GetInitialLocalPosTargetLimb().y; 
                Transform targetHips = GetIKTarget(HumanBodyBones.Hips);
                targetHips.localPosition = new Vector3(targetHips.localPosition.x, Mathf.Min(targetHips.localPosition.y, maxY), targetHips.localPosition.z);
            }
        }

        protected virtual void UpdateTransformNodePosFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = GetIKTarget(boneID);
            if (!t) return;

            t.position = node.GetTrackedObject().transform.position;
        }

        protected virtual void UpdateTransformNodeRotFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = GetIKTarget(boneID);
            if (!t) return;

            t.rotation = node.GetTrackedObject().transform.rotation;
        }

        protected virtual void UpdateTransformNodePosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        {
            UpdateTransformNodePosFromCalibrationPose(node, boneID, Vector3.one);
        }

        protected virtual void UpdateTransformNodePosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID, Vector3 offsetScale)
        {
            Transform t = GetIKTarget(boneID);
            Transform calibrationPose = node.GetCalibrationPose();
            if (!t || !calibrationPose) return;

            t.localPosition = GetIKSolver(boneID).GetInitialLocalPosTargetLimb();
            Vector3 offset = Vector3.Scale(node.GetTrackedObject().transform.position - calibrationPose.position, offsetScale);
            t.position += offset;
        }

        protected virtual void UpdateTransformNodeRotFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = GetIKTarget(boneID);
            Transform calibrationPose = node.GetCalibrationPose();
            if (!t || !calibrationPose) return;

            t.localRotation = GetIKSolver(boneID).GetInitialLocalRotTargetLimb();
            Quaternion rotOffset = node.GetTrackedObject().transform.rotation * Quaternion.Inverse(calibrationPose.rotation);
            t.rotation = rotOffset * t.rotation;
        }

        protected virtual void UpdateVRCursors()
        {
            GetVRCursor(VRCursorType.LEFT).transform.position = _vrHandLeft._handBoneIndexDistal.position;
            GetVRCursor(VRCursorType.RIGHT).transform.position = _vrHandRight._handBoneIndexDistal.position;
        }

        #endregion

    }

}
