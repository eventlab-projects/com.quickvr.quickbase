using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Animations;

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

        public static Vector3 HAND_CONTROLLER_POSITION_OFFSET = new Vector3(0, 0, -0.1f);

        #endregion

        #region PUBLIC ATTRIBUTES

        [BitMask(typeof(IKLimbBones))]
        public int _trackedJoints = -1;

        public bool _useFootprints = true;
        
        public static bool _handsSwaped = false;

        public enum HandTrackingMode
        {
            Controllers,
            Hands,
        }
        public HandTrackingMode _handTrackingMode = HandTrackingMode.Controllers;

        public bool _updatePosition = false;
        public bool _updateRotation = false;

        public bool _isStanding = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRPlayArea _vrPlayArea = null;

        protected Vector3 _userDisplacement = Vector3.zero; //The accumulated real displacement of the user

        protected Transform _calibrationPose = null;

        protected QuickCharacterControllerManager _characterControllerManager = null;

        protected bool _autoUserForward = true; //If true, the forward of the user (the real person) is retrieved from the tracking data at every frame. Otherwise, the user forward is manually provided. 
        protected Vector3 _customUserForward = Vector3.zero;  //The provided user forward when _autoUserForward is set to false. 

        protected Vector3 _headOffset = Vector3.zero;

        protected PositionConstraint _footprints = null;

        #endregion

        #region EVENTS

        public delegate void CalibrateAction();
        public static event CalibrateAction OnCalibrate;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += OnPostUpdateTracking;

            QuickVRNode.OnCalibrateVRNodeHead += OnCalibrateVRNodeHead;
            QuickVRNode.OnCalibrateVRNodeHips += OnCalibrateVRNodeHips;
            QuickVRNode.OnCalibrateVRNodeLeftHand += OnCalibrateVRNodeLeftHand;
            QuickVRNode.OnCalibrateVRNodeRightHand += OnCalibrateVRNodeRightHand;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= OnPostUpdateTracking;

            QuickVRNode.OnCalibrateVRNodeHead -= OnCalibrateVRNodeHead;
            QuickVRNode.OnCalibrateVRNodeHips -= OnCalibrateVRNodeHips;
            QuickVRNode.OnCalibrateVRNodeLeftHand -= OnCalibrateVRNodeLeftHand;
            QuickVRNode.OnCalibrateVRNodeRightHand -= OnCalibrateVRNodeRightHand;
        }

        protected override void Awake()
        {
            _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _vrPlayArea.transform.parent = transform;

            base.Awake();

            CreateFootPrints();

            _characterControllerManager = gameObject.GetOrCreateComponent<QuickCharacterControllerManager>();

            _calibrationPose = new GameObject("__CalibrationPose__").transform;
            _calibrationPose.position = transform.position;
            _calibrationPose.rotation = transform.rotation;
        }

        protected override void Start()
        {
            base.Start();

            if (!QuickUtils.IsHandTrackingSupported())
            {
                _handTrackingMode = HandTrackingMode.Controllers;
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

        #endregion

        #region GET AND SET

        public virtual Vector3 GetUserForward()
        {
            if (_autoUserForward)
            {
                return Vector3.ProjectOnPlane(_vrPlayArea.GetVRNodeMain().transform.forward, transform.up);
            }
            return _customUserForward;
        }

        public virtual void SetUserForward(Vector3 fwd)
        {
            _autoUserForward = false;
            _customUserForward = fwd;
        }

        public virtual void ResetUserForward()
        {
            _autoUserForward = false;
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
            Vector3 userForward = GetUserForward();
            return Vector3.SignedAngle(transform.forward, userForward, transform.up);
        }

        public override void Calibrate()
        {
            transform.position = _calibrationPose.position;
            transform.rotation = _calibrationPose.rotation;
            _footprints.translationOffset = Vector3.zero;
            _footprints.transform.rotation = transform.rotation;

            _vrPlayArea.Calibrate();

            if (OnCalibrate != null) OnCalibrate();
        }

        protected virtual void OnCalibrateVRNodeHead(QuickVRNode node)
        {
            node.GetTrackedObject().transform.localPosition = _headOffset;

            float rotAngle = Vector3.SignedAngle(GetUserForward(), transform.forward, transform.up);
            _vrPlayArea.transform.Rotate(transform.up, rotAngle, Space.World);
        }

        protected virtual void OnCalibrateVRNodeHips(QuickVRNode node)
        {
            QuickTrackedObject tObjectHead = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            QuickTrackedObject tObjectHips = node.GetTrackedObject();
            tObjectHips.transform.position = new Vector3(tObjectHead.transform.position.x, tObjectHips.transform.position.y, tObjectHead.transform.position.z);

            float rotAngle = Vector3.SignedAngle(GetUserForward(), transform.forward, transform.up);
            _vrPlayArea.transform.Rotate(transform.up, rotAngle, Space.World);
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
                tObject.transform.LookAt(tObject.transform.position + transform.right, -transform.up);
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
                tObject.transform.LookAt(tObject.transform.position - transform.right, transform.up);
            }
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

            UpdateVRCursors();
        }

        protected virtual void OnPostUpdateTracking()
        {
            UpdateCameraPosition();
        }

        protected virtual void UpdateTransformRoot()
        {
            if (!_isStanding) return;

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

        #endregion

    }

}
