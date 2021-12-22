using UnityEngine;
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

        //Rotation limits for CameraMono
        const float MAX_HORIZONTAL_ANGLE = 80;
        const float MAX_VERTICAL_ANGLE = 45;

        #endregion

        #region PUBLIC ATTRIBUTES

        public QuickHandGestureSettings _gestureSettingsLeftHand = null;
        public QuickHandGestureSettings _gestureSettingsRightHand = null;

        public bool _useFootprints = true;

        public static bool _handsSwaped = false;

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

        public bool _rotateCameraMono = true;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickVRPlayArea _vrPlayArea = null;

        protected Vector3 _headOffset = Vector3.zero;

        protected PositionConstraint _footprints = null;

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

        protected List<KeyValuePair<Transform, Transform>> _boneFingers = null;

        //Rotation attributes for CameraMono
        protected float _speedH = 2.0f;
        protected float _speedV = 2.0f;
        protected float _offsetH = 0;
        protected float _offsetV = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            _vrPlayArea.GetVRNode(HumanBodyBones.Head).OnCalibrateVRNode += OnCalibrateVRNodeHead;
            _vrPlayArea.GetVRNode(HumanBodyBones.Hips).OnCalibrateVRNode += OnCalibrateVRNodeHips;
            _vrPlayArea.GetVRNode(HumanBodyBones.LeftHand).OnCalibrateVRNode += OnCalibrateVRNodeLeftHand;
            _vrPlayArea.GetVRNode(HumanBodyBones.RightHand).OnCalibrateVRNode += OnCalibrateVRNodeRightHand;
            _vrPlayArea.GetVRNode(HumanBodyBones.LeftFoot).OnCalibrateVRNode += OnCalibrateVRNodeFoot;
            _vrPlayArea.GetVRNode(HumanBodyBones.RightFoot).OnCalibrateVRNode += OnCalibrateVRNodeFoot;

            if (!QuickVRManager.IsXREnabled())
            {
                QuickVRManager.OnPostUpdateIKTargets += UpdateHeadRotationMono;
            }
        }

        protected virtual void OnDisable()
        {
            _vrPlayArea.GetVRNode(HumanBodyBones.Head).OnCalibrateVRNode -= OnCalibrateVRNodeHead;
            _vrPlayArea.GetVRNode(HumanBodyBones.Hips).OnCalibrateVRNode -= OnCalibrateVRNodeHips;
            _vrPlayArea.GetVRNode(HumanBodyBones.LeftHand).OnCalibrateVRNode -= OnCalibrateVRNodeLeftHand;
            _vrPlayArea.GetVRNode(HumanBodyBones.RightHand).OnCalibrateVRNode -= OnCalibrateVRNodeRightHand;
            _vrPlayArea.GetVRNode(HumanBodyBones.LeftFoot).OnCalibrateVRNode -= OnCalibrateVRNodeFoot;
            _vrPlayArea.GetVRNode(HumanBodyBones.RightFoot).OnCalibrateVRNode -= OnCalibrateVRNodeFoot;

            if (!QuickVRManager.IsXREnabled())
            {
                QuickVRManager.OnPostUpdateIKTargets -= UpdateHeadRotationMono;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _vrPlayArea.transform.parent = transform;

            CreateVRCursors();
            CreateFootPrints();
        }

        protected virtual void Start()
        {
            _headOffset = Quaternion.Inverse(transform.rotation) * (_animator.GetBoneTransform(HumanBodyBones.Head).position - _animator.GetEyeCenterPosition());
            _vrManager.AddUnityVRTrackingSystem(this);

            if (_gestureSettingsLeftHand == null)
            {
                _gestureSettingsLeftHand = LoadHandGestureSettings(true);
            }
            if (_gestureSettingsRightHand == null)
            {
                _gestureSettingsRightHand = LoadHandGestureSettings(false);
            }
        }

        protected virtual QuickHandGestureSettings LoadHandGestureSettings(bool isLeft)
        {
            string s = "";
            const string infix = "_HandGestureSettings_";

            switch (QuickVRManager._hmdModel)
            {
                case QuickVRManager.HMDModel.OculusQuest:
                case QuickVRManager.HMDModel.OculusQuest2:
                    s = "Touch";
                    break;

                default:
                    s = "Default";
                    break;
            }

            return Resources.Load<QuickHandGestureSettings>("HandGestureSettings/" + s + infix + (isLeft ? "Left" : "Right"));
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
            CreateVRCursorHand(QuickUICursor.Role.LeftHand, _animator.GetBoneTransform(HumanBodyBones.LeftHand), _animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal));
            CreateVRCursorHand(QuickUICursor.Role.RightHand, _animator.GetBoneTransform(HumanBodyBones.RightHand), _animator.GetBoneTransform(HumanBodyBones.RightIndexDistal));
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

        #endregion

        #region GET AND SET

        protected virtual void InitBoneFingers()
        {
            _boneFingers = new List<KeyValuePair<Transform, Transform>>();
            foreach (bool b in new bool[] { true, false })
            {
                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, b);
                    for (int i = 0; i < fingerBones.Count; i++)
                    {
                        QuickHumanBodyBones fBoneID = fingerBones[i];
                        _boneFingers.Add(new KeyValuePair<Transform, Transform>(_animator.GetBoneTransform(fBoneID), _vrPlayArea.GetVRNode(fBoneID).GetTrackedObject().transform));
                    }
                }
            }
        }

        public virtual ControlType GetIKControl(IKBone ikBone)
        {
            return _ikControls[(int)ikBone];
        }

        public virtual void SetIKControl(IKBone ikBone, ControlType cType)
        {
            _ikControls[(int)ikBone] = cType;
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

            //QuickVRNode n = _vrPlayArea.GetVRNodeMain();
            //Vector3 offset = n.GetTrackedObject().transform.position - n.GetCalibrationPose().position;

            //return Vector3.Scale(offset, Vector3.right + Vector3.forward);

            return Vector3.zero;
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
            
        }

        protected virtual void OnCalibrateVRNodeRightHand(QuickVRNode node)
        {
            
        }

        protected virtual void OnCalibrateVRNodeFoot(QuickVRNode node)
        {
            Transform ikTarget = GetIKSolver((HumanBodyBones)node.GetRole())._targetLimb;
            QuickTrackedObject tObject = node.GetTrackedObject();
            tObject.transform.rotation = ikTarget.rotation;
        }

        #endregion

        #region UPDATE

        protected override void LateUpdate()
        {
            
        }

        public virtual void UpdateIKTargets()
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
                            UpdateIKTargetPosFromUser(node, boneID);
                            
                            //Update the QuickVRNode's rotation
                            UpdateIKTargetRotFromUser(node, boneID);
                            
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
                                QuickIKSolverEye ikSolver = (QuickIKSolverEye)_animator.GetComponent<QuickIKManager>().GetIKSolver(boneID);
                                ikSolver._weightBlink = ((QuickVRNodeEye)node).GetBlinkFactor();
                            }
                        }
                    }
                }

                //2) Special case. If the Hips is set to Tracking mode, we need to adjust the IKTarget position of the hips
                //in a way that the head will match the position of the camera provided by the HMD
                if (GetIKControl(IKBone.Hips) == ControlType.Tracking)
                {
                    QuickIKSolver ikSolverHips = GetIKSolver(IKBone.Hips);
                    QuickIKSolver ikSolverHead = GetIKSolver(IKBone.Head);
                    float chainLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.Hips).position, _animator.GetBoneTransform(HumanBodyBones.Head).position);
                    Vector3 v = (ikSolverHips._targetLimb.position - ikSolverHead._targetLimb.position).normalized;
                    ikSolverHips._targetLimb.position = ikSolverHead._targetLimb.position + v * chainLength;
                }

                UpdateVRCursors();
                _footprints.gameObject.SetActive(_useFootprints);
            }
        }

        protected virtual void UpdateHeadRotationMono()
        {
            if (_rotateCameraMono)
            {
                float x = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
                float y = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
                _offsetH += _speedH * x;
                _offsetV -= _speedV * y;

                _offsetH = Mathf.Clamp(_offsetH, -MAX_HORIZONTAL_ANGLE, MAX_HORIZONTAL_ANGLE);
                _offsetV = Mathf.Clamp(_offsetV, -MAX_VERTICAL_ANGLE, MAX_VERTICAL_ANGLE);

                Transform t = GetIKSolver(HumanBodyBones.Head)._targetLimb;
                t.localRotation = Quaternion.identity;
                t.Rotate(t.up, _offsetH, Space.World);
                t.Rotate(t.right, _offsetV, Space.World);

                _vrPlayArea.GetVRNode(HumanBodyBones.Head).transform.localRotation = t.localRotation;
            }
        }

        protected virtual void ApplyFingerRotation(KeyValuePair<Transform, Transform> fingerBone, KeyValuePair<Transform, Transform> fingerBoneNext)
        {
            Transform bone0 = fingerBone.Key;
            Transform bone1 = fingerBoneNext.Key;
            Transform ovrBone0 = fingerBone.Value; 
            Transform ovrBone1 = fingerBoneNext.Value;

            Vector3 currentDir = bone1.position - bone0.position;
            Vector3 targetDir = ovrBone1.position - ovrBone0.position;
            float rotAngle = Vector3.Angle(currentDir, targetDir);
            Vector3 rotAxis = Vector3.Cross(currentDir, targetDir).normalized;

            bone0.Rotate(rotAxis, rotAngle, Space.World);
        }

        protected override void UpdateIKFingers()
        {
            if (_vrPlayArea)
            {

                //if (_boneFingers == null)
                //{
                //    InitBoneFingers();
                //}

                //for (int j = 0; j < _boneFingers.Count; j+= 4)
                //{
                //    if (_boneFingers[j].Key != null)
                //    {
                //        for (int i = 0; i < 3; i++)
                //        {
                //            if ((i == 0 && (j == 0 || j == 20)) && QuickVRManager._handTrackingMode == QuickVRManager.HandTrackingMode.Controllers)
                //            {
                //                //HACK
                //                //Avoid applying the rotation to the thumb distal fingers as the results look weird. Look for a better method 
                //                //of transfering the bone rotations when using the controllers. 
                //                continue;
                //            }
                //            ApplyFingerRotation(_boneFingers[j + i], _boneFingers[j + i + 1]);
                //        }
                //    }
                //}


                //foreach (bool b in new bool[] { true, false })
                //{
                //    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                //    {
                //        List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, b);
                //        List<Quaternion> initialFingerBonesLocalRotations = new List<Quaternion>();

                //        for (int i = 0; i < fingerBones.Count - 1; i++)
                //        {
                //            QuickHumanBodyBones fBoneID = fingerBones[i];
                //            initialFingerBonesLocalRotations.Add(_animator.GetBoneTransform(fBoneID).localRotation);

                //            if (_animator.GetBoneTransform(fBoneID) && _vrPlayArea.GetVRNode(fBoneID).IsTracked())
                //            {
                //                ApplyFingerRotation(fBoneID, fingerBones[i + 1]);
                //            }
                //        }

                //        //At this point the finger is correctly aligned. Set the targets to match this. 
                //        //HumanBodyBones boneID = (HumanBodyBones)fingerBones[2];
                //        //QuickIKSolver ikSolver = GetIKSolver(boneID);
                //        //Transform tBone = _animator.GetBoneTransform(boneID);

                //        //ikSolver._targetLimb.position = tBone.position;
                //        //ikSolver._targetLimb.GetChild(0).rotation = tBone.rotation;
                //        //ikSolver._targetHint.position = ikSolver._boneMid.position + (ikSolver._boneMid.position - ikSolver._boneUpper.position) + (ikSolver._boneMid.position - ikSolver._boneLimb.position);

                //        ////Restore the rotation of the bone fingers
                //        //ikSolver._boneUpper.localRotation = initialFingerBonesLocalRotations[0];
                //        //ikSolver._boneMid.localRotation = initialFingerBonesLocalRotations[1];
                //        //ikSolver._boneLimb.localRotation = initialFingerBonesLocalRotations[2];
                //    }
                //}



                //foreach (bool b in new bool[] { true, false })
                //{
                //    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                //    {
                //        List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, b);
                //        List<Quaternion> initialFingerBonesLocalRotations = new List<Quaternion>();

                //        for (int i = 0; i < fingerBones.Count - 1; i++)
                //        {
                //            QuickHumanBodyBones fBoneID = fingerBones[i];
                //            initialFingerBonesLocalRotations.Add(_animator.GetBoneTransform(fBoneID).localRotation);

                //            if (_animator.GetBoneTransform(fBoneID) && _vrPlayArea.GetVRNode(fBoneID).IsTracked())
                //            {
                //                ApplyFingerRotation(fBoneID, fingerBones[i + 1]);
                //            }
                //        }

                //        //At this point the finger is correctly aligned. Set the targets to match this. 
                //        //HumanBodyBones boneID = (HumanBodyBones)fingerBones[2];
                //        //QuickIKSolver ikSolver = GetIKSolver(boneID);
                //        //Transform tBone = _animator.GetBoneTransform(boneID);

                //        //ikSolver._targetLimb.position = tBone.position;
                //        //ikSolver._targetLimb.GetChild(0).rotation = tBone.rotation;
                //        //ikSolver._targetHint.position = ikSolver._boneMid.position + (ikSolver._boneMid.position - ikSolver._boneUpper.position) + (ikSolver._boneMid.position - ikSolver._boneLimb.position);

                //        ////Restore the rotation of the bone fingers
                //        //ikSolver._boneUpper.localRotation = initialFingerBonesLocalRotations[0];
                //        //ikSolver._boneMid.localRotation = initialFingerBonesLocalRotations[1];
                //        //ikSolver._boneLimb.localRotation = initialFingerBonesLocalRotations[2];
                //    }
                //}

                //foreach (bool isLeft in new bool[] { true, false })
                //{
                //    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                //    {
                //        float fLength = _vrPlayArea.GetFingerLength(f, isLeft);
                //        if (fLength > 0)
                //        {
                //            List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
                //            QuickVRNode n0 = _vrPlayArea.GetVRNode(fingerBones[0]);
                //            QuickVRNode n1 = _vrPlayArea.GetVRNode(fingerBones[1]);
                //            QuickVRNode n2 = _vrPlayArea.GetVRNode(fingerBones[2]);

                //            QuickIKSolver ikSolver = GetIKSolver((HumanBodyBones)fingerBones[2]);

                //            if (n0.IsTracked() && n2.IsTracked())
                //            {
                //                float sf = ikSolver.GetChainLength() / fLength;
                //                Vector3 v = sf * (n2.transform.position - n0.transform.position);

                //                ikSolver._targetLimb.position = ikSolver._boneUpper.position + v;
                //                ikSolver._targetHint.position = ikSolver._boneMid.position + (n1.transform.position - n0.transform.position) + (n1.transform.position - n2.transform.position);
                //            }
                //        }
                //    }
                //}

                foreach (bool isLeft in new bool[] { true, false })
                {
                    foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                    {
                        List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
                        QuickVRNode n0 = _vrPlayArea.GetVRNode(fingerBones[0]);
                        QuickVRNode n1 = _vrPlayArea.GetVRNode(fingerBones[1]);
                        QuickVRNode n2 = _vrPlayArea.GetVRNode(fingerBones[2]);

                        if (n0.IsTracked() && n1.IsTracked() && n2.IsTracked())
                        {
                            QuickIKSolver ikSolver = GetIKSolver((HumanBodyBones)fingerBones[2]);

                            Vector3 v = (n1.transform.position - n0.transform.position).normalized;
                            Vector3 w = (n2.transform.position - n1.transform.position).normalized;

                            ikSolver._targetLimb.position = ikSolver._boneUpper.position + v * ikSolver.GetUpperLength() + w * ikSolver.GetMidLength();
                            ikSolver._targetLimb.rotation = n2.transform.rotation;
                            ikSolver._targetHint.position = ikSolver._boneMid.position + n1.transform.up * DEFAULT_TARGET_HINT_FINGER_DISTANCE;
                            //ikSolver._targetHint.position = ikSolver._boneMid.position + (n1.transform.position - n0.transform.position) + (n1.transform.position - n2.transform.position);
                        }
                    }
                }

            }

            base.UpdateIKFingers();
        }
        
        protected virtual void UpdateIKTargetPosFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            GetIKSolver(boneID)._targetLimb.position = node.GetTrackedObject().transform.position;
        }

        protected virtual void UpdateIKTargetRotFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            GetIKSolver(boneID)._targetLimb.rotation = node.GetTrackedObject().transform.rotation;
        }

        //protected virtual void UpdateIKTargetPosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        //{
        //    UpdateIKTargetPosFromCalibrationPose(node, boneID, Vector3.one);
        //}

        //protected virtual void UpdateIKTargetPosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID, Vector3 offsetScale)
        //{
        //    Transform t = GetIKSolver(boneID)._targetLimb;
        //    Transform calibrationPose = node.GetCalibrationPose();
        //    if (!t || !calibrationPose) return;

        //    t.localPosition = GetIKSolver(boneID).GetInitialLocalPosTargetLimb();
        //    Vector3 offset = Vector3.Scale(node.GetTrackedObject().transform.position - calibrationPose.position, offsetScale);
        //    t.position += offset;
        //}

        //protected virtual void UpdateIKTargetRotFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        //{
        //    Transform t = GetIKSolver(boneID)._targetLimb;
        //    Transform calibrationPose = node.GetCalibrationPose();
        //    if (!t || !calibrationPose) return;

        //    t.localRotation = GetIKSolver(boneID).GetInitialLocalRotTargetLimb();
        //    Quaternion rotOffset = node.GetTrackedObject().transform.rotation * Quaternion.Inverse(calibrationPose.rotation);
        //    t.rotation = rotOffset * t.rotation;
        //}

        protected virtual void UpdateVRCursors()
        {
            QuickUICursor.GetVRCursor(QuickUICursor.Role.LeftHand).transform.position = _animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal).position;
            QuickUICursor.GetVRCursor(QuickUICursor.Role.RightHand).transform.position = _animator.GetBoneTransform(HumanBodyBones.RightIndexDistal).position;
        }

        #endregion

    }

}
