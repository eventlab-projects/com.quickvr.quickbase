using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

    public class QuickUnityVR : QuickUnityVRBase
    {

        #region PUBLIC ATTRIBUTES

        public bool _displaceWithCamera = false;
        public bool _rotateWithCamera = false;

        public bool _isStanding = true;
        public bool _applyUserScale = false;

        public enum UpdateMode
        {
            FromUser,
            FromCalibrationPose,
        }
        public UpdateMode _updateMode = UpdateMode.FromUser;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected QuickIKManager _ikManager = null;

        protected QuickTrackedObject _verticalReference = null;
        protected float _initialVerticalReferencePosY = 0.0f;

        protected float _unscaledHeadHeight = 0.0f;
        protected Vector3 _headOffset = Vector3.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _headOffset = Quaternion.Inverse(transform.rotation) * (_animator.GetBoneTransform(HumanBodyBones.Head).position - GetEyeCenterPosition());

            //Create the IKManager
            if (!gameObject.GetComponent<QuickIKManager>())
            {
#if UNITY_2019_1_OR_NEWER 
                gameObject.AddComponent<QuickIKManager_v2>();
#else
                gameObject.AddComponent<QuickIKManager_v1>();
#endif
            }
            _ikManager = gameObject.GetComponent<QuickIKManager>();
            _ikManager.enabled = false; //We control when to update the IK
        }

        protected override void CreateVRHands()
        {
            _vrHandLeft = gameObject.AddComponent<QuickVRHand>();
            _vrHandLeft._handBone = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _vrHandLeft._handBoneIndexDistal = _animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

            _vrHandRight = gameObject.AddComponent<QuickVRHand>();
            _vrHandRight._handBone = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _vrHandRight._handBoneIndexDistal = _animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

            base.CreateVRHands();
        }

        #endregion

        #region GET AND SET

        protected Transform GetCalibrationPose(QuickVRNode.Type tNode)
        {
            return _vrPlayArea.transform.Find("_CalibrationPose_" + tNode.ToString());
        }

        protected virtual float GetHeadHeight()
        {
            return _unscaledHeadHeight * (1.0f / transform.lossyScale.y);
        }

        public override void Calibrate()
        {
            _ikManager.Calibrate();

            transform.localScale = Vector3.one;
            _unscaledHeadHeight = _ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb.position.y - transform.position.y;

            base.Calibrate();

            //QuickVRNode nodeHips = _vrPlayArea.GetVRNode(QuickVRNode.Type.Hips);
            //_verticalReference = nodeHips.IsTracked() ? nodeHips.GetTrackedObject() : _vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            //_initialVerticalReferencePosY = _verticalReference.transform.position.y;
        }

        protected override void CalibrateVRNode(QuickVRNode.Type nodeType)
        {
            base.CalibrateVRNode(nodeType);

            //QuickTrackedObject tObject = _vrPlayArea.GetVRNode(nodeType).GetTrackedObject();
            //Transform cPose = GetCalibrationPose(nodeType);
            //cPose.position = tObject.transform.position;
            //cPose.rotation = tObject.transform.rotation;
            //tObject.Reset();
        }

        protected override void CalibrateVRNodeHead(QuickVRNode node)
        {
            base.CalibrateVRNodeHead(node);

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
            QuickTrackedObject tObject = node.GetTrackedObject();
            tObject.transform.localPosition = _headOffset * transform.lossyScale.x;

            Vector3 offset = _animator.GetBoneTransform(HumanBodyBones.Head).position - tObject.transform.position;
            _vrPlayArea.transform.position += offset;

            //Set the position of the vrNodesOrigin
            //_vrNodesOrigin.position = tObject.transform.position - transform.up * GetHeadHeight() * transform.lossyScale.y;
        }

        public override Vector3 GetDisplacement()
        {
            if (_isStanding)
            {
                QuickVRNode hipsNode = _vrPlayArea.GetVRNode(QuickVRNode.Type.Hips);
                if (_vrPlayArea.IsTrackedNode(hipsNode)) return hipsNode.GetTrackedObject().GetDisplacement();
                else if (_displaceWithCamera) return _vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject().GetDisplacement();
            }
            
            return Vector3.zero;
        }

        protected override float GetRotationOffset()
        {
            //if (_isStanding)
            //{
            //    QuickVRNode HipsNode = _vrPlayArea.GetVRNode(QuickVRNode.Type.Hips);
            //    QuickVRNode hmdNode = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head);
            //    QuickVRNode node = null;

            //    if (HipsNode.IsTracked()) node = HipsNode;
            //    else if (_rotateWithCamera) node = hmdNode;

            //    if (!node) return 0.0f;

            //    Vector3 currentForward = Vector3.ProjectOnPlane(_vrNodesOrigin.forward, _vrNodesOrigin.up);
            //    Vector3 targetForward = Vector3.ProjectOnPlane(node.transform.forward, _vrNodesOrigin.up);

            //    return Vector3.SignedAngle(currentForward, targetForward, _vrNodesOrigin.up);
            //}

            return 0.0f;
        }

        public override Vector3 GetEyeCenterPosition()
        {
            Transform lEye = _animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Transform rEye = _animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (lEye && rEye) return Vector3.Lerp(lEye.position, rEye.position, 0.5f);
            if (lEye) return lEye.position;
            if (rEye) return rEye.position;

            return GetAvatarHead().position;
        }

        #endregion

        #region UPDATE

        public virtual void Update()
        {
            _ikManager.Update();
        }

        //protected override void UpdateTransformRoot()
        //{
        //    base.UpdateTransformRoot();

        //    Transform t = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head).GetTrackedObject().transform;
        //    float h = t.position.y - _vrNodesOrigin.position.y;
        //    _vrNodesOrigin.position = new Vector3(_vrNodesOrigin.position.x, t.position.y - Mathf.Min(h, GetHeadHeight()), _vrNodesOrigin.position.z);

        //    if (_isStanding)
        //    {
        //        float hipsOffsetY = Mathf.Min(0, _verticalReference.transform.position.y - _initialVerticalReferencePosY);
        //        _ikManager.ResetIKSolver(HumanBodyBones.Hips);
        //        IQuickIKSolver ikSolver = _ikManager.GetIKSolver(HumanBodyBones.Hips); 
        //        ikSolver._targetLimb.Translate(Vector3.up * hipsOffsetY, Space.World);
        //    }
            
        //    if (_rotateWithCamera) CalibrateCameraForward();
        //}

        protected override void UpdateTransformNodes()
        {
            base.UpdateTransformNodes();

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                if (!_vrPlayArea.IsTrackedNode(t)) continue;
                QuickVRNode node = _vrPlayArea.GetVRNode(t);
                if (!node) continue;

                HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(t.ToString());
                QuickTrackedObject tObject = node.GetTrackedObject();

                IQuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
                if (ikSolver == null) continue;

                if (_updateMode == UpdateMode.FromUser)
                {
                    UpdateTransformNodeFromUser(node, ikSolver, boneID, true, true);
                }
                else
                {
                    if (boneID == HumanBodyBones.Head)
                    {
                        UpdateTransformNodeFromCalibrationPose(node, ikSolver, boneID, true, false);
                        UpdateTransformNodeFromUser(node, ikSolver, boneID, false, true);
                    }
                    else
                    {
                        UpdateTransformNodeFromCalibrationPose(node, ikSolver, boneID, true, true);
                    }
                }
            }

            UpdateTrackingIK();
        }

        protected virtual void UpdateTransformNodeFromUser(QuickVRNode node, IQuickIKSolver ikSolver, HumanBodyBones boneID, bool updatePos, bool updateRot)
        {
            Transform t = _ikManager.GetIKTarget(boneID);
            if (!t) return;

            if (updatePos)
            {
                t.position = node.GetTrackedObject().transform.position;
            }

            if (updateRot)
            {
                t.rotation = node.GetTrackedObject().transform.rotation;
            }
        }

        protected virtual void UpdateTransformNodeFromCalibrationPose(QuickVRNode node, IQuickIKSolver ikSolver, HumanBodyBones boneID, bool updatePos, bool updateRot)
        {
            //QuickIKData initialIKData = _ikManager.GetInitialIKData(boneID);
            //Transform t = _ikManager.GetIKTarget(boneID);
            //if (!t) return;

            //Vector3 initialLocalPos = _ikManager.GetInitialIKDataLocalPos(boneID);
            //Quaternion initialLocalRot = _ikManager.GetInitialIKDataLocalRot(boneID);
            
            //Transform calibrationPose = GetCalibrationPose(node.GetNodeType());
            //if (!calibrationPose) return;

            //Quaternion tmp = transform.rotation;
            //transform.rotation = _vrNodesOrigin.rotation;
            
            //if (updatePos)
            //{
            //    t.localPosition = initialLocalPos;
            //    Vector3 offset = node.GetTrackedObject().transform.position - calibrationPose.position;
            //    t.position += offset;
            //}
            
            //if (updateRot)
            //{
            //    t.localRotation = initialLocalRot;
            //    Quaternion rotOffset = node.GetTrackedObject().transform.rotation * Quaternion.Inverse(calibrationPose.rotation);
            //    t.rotation = rotOffset * t.rotation;
            //}

            //transform.rotation = tmp;
        }

        protected virtual void UpdateTrackingIK()
        {
            //_ikManager._ikMask = _trackedJoints;
            //Check if the IKHint targets are actually tracked. 
            //foreach (IKLimbBones b in QuickIKManager.GetIKLimbBones())
            //{
            //    HumanBodyBones boneLimbID = QuickIKManager.ToUnity(b);
            //    HumanBodyBones boneMidID = QuickHumanTrait.GetParentBone(boneLimbID);
            //    IQuickIKSolver ikSolver = _ikManager.GetIKSolver(boneLimbID);
            //    QuickVRNode nodeMid = _vrPlayArea.GetVRNode(boneMidID);
            //    if (nodeMid && nodeMid.IsTracked())
            //    {
            //        _ikManager._ikHintMaskUpdate &= ~(1 << (int)boneLimbID);
            //        //ikSolver.ResetIKChain();

            //        //Compute the rotation of the Bone Upper
            //        HumanBodyBones boneUpperID = QuickHumanTrait.GetParentBone(boneMidID);
            //        Vector3 userBoneMidPos = _vrPlayArea.GetVRNode(boneMidID).GetTrackedObject().transform.position;
            //        Vector3 userBoneUpperPos = _vrPlayArea.GetVRNode(boneUpperID).GetTrackedObject().transform.position;

            //        Vector3 currentBoneDir = ikSolver._boneMid.position - ikSolver._boneUpper.position;
            //        Vector3 targetBoneDir = ToAvatarSpace(userBoneMidPos - userBoneUpperPos);

            //        Vector3 rotAxis = Vector3.Cross(currentBoneDir, targetBoneDir).normalized;
            //        float rotAngle = Vector3.Angle(currentBoneDir, targetBoneDir);
            //        ikSolver._boneUpper.Rotate(rotAxis, rotAngle, Space.World);

            //        //Compute the rotation of the Bone Mid
            //        Vector3 userBoneLimbPos = _vrPlayArea.GetVRNode(boneLimbID).GetTrackedObject().transform.position;

            //        currentBoneDir = ikSolver._boneLimb.position - ikSolver._boneMid.position;
            //        targetBoneDir = ToAvatarSpace(userBoneLimbPos - userBoneMidPos);

            //        rotAxis = Vector3.Cross(currentBoneDir, targetBoneDir).normalized;
            //        rotAngle = Vector3.Angle(currentBoneDir, targetBoneDir);
            //        ikSolver._boneMid.Rotate(rotAxis, rotAngle, Space.World);

            //        ikSolver._weightIKPos = 0.0f;
            //    }
            //    else ikSolver._weightIKPos = 1.0f;
            //}

            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKPos = _applyHeadPosition ? 1.0f : 0.0f;
            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKRot = _applyHeadRotation ? 1.0f : 0.0f;

            _ikManager.UpdateTracking();
        }

        #endregion

    }

}
