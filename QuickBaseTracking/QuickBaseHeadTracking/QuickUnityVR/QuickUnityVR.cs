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

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected QuickIKManager _ikManager = null;

        protected Vector3 _initialHipsLocalPosition = Vector3.zero;
        protected QuickTrackedObject _verticalReference = null;
        protected float _initialVerticalReferencePosY = 0.0f;

        protected float _unscaledHeadHeight = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _initialHipsLocalPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
            _unscaledHeadHeight = (_animator.GetBoneTransform(HumanBodyBones.Head).position.y - transform.position.y);
            
            //Create the IKManager
            _ikManager = gameObject.GetOrCreateComponent<QuickIKManager>();
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

        protected virtual float GetHeadHeight()
        {
            return _unscaledHeadHeight * (1.0f / transform.lossyScale.y);
        }

        public override void Calibrate()
        {
            _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = _initialHipsLocalPosition;

            _ikManager.Calibrate();

            base.Calibrate();

            QuickVRNode nodeHips = GetQuickVRNode(QuickVRNode.Type.Hips);
            _verticalReference = nodeHips.IsTracked() ? nodeHips.GetTrackedObject() : GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            _initialVerticalReferencePosY = _verticalReference.transform.position.y;
        }

        protected override void CalibrateVRNodeHead(QuickVRNode node)
        {
            QuickTrackedObject tObject = node.GetTrackedObject();

            Transform tHead = _animator.GetBoneTransform(HumanBodyBones.Head);
            tObject.transform.localPosition = Quaternion.Inverse(transform.rotation) * (tHead.position - GetEyeCenterPosition());
            _vrNodesOrigin.position = tObject.transform.position - transform.up * GetHeadHeight();

            base.CalibrateVRNodeHead(node);
        }

        public override Vector3 GetDisplacement()
        {
            QuickVRNode HipsNode = GetQuickVRNode(QuickVRNode.Type.Hips);
            if (HipsNode.IsTracked()) return HipsNode.GetTrackedObject().GetDisplacement();
            else if (_displaceWithCamera) return GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().GetDisplacement();

            return Vector3.zero;
        }

        protected override float GetRotationOffset()
        {
            QuickVRNode HipsNode = GetQuickVRNode(QuickVRNode.Type.Hips);
            QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);
            QuickVRNode node = null;

            if (HipsNode.IsTracked()) node = HipsNode;
            else if (_rotateWithCamera) node = hmdNode;

            if (!node) return 0.0f;

            Vector3 currentForward = Vector3.ProjectOnPlane(_vrNodesOrigin.forward, _vrNodesOrigin.up);
            Vector3 targetForward = Vector3.ProjectOnPlane(node.transform.forward, _vrNodesOrigin.up);

            return Vector3.SignedAngle(currentForward, targetForward, _vrNodesOrigin.up);
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

        protected override void UpdateTransformRoot()
        {
            base.UpdateTransformRoot();

            Transform t = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().transform;
            float h = t.position.y - _vrNodesOrigin.position.y;
            _vrNodesOrigin.position = new Vector3(_vrNodesOrigin.position.x, t.position.y - Mathf.Min(h, GetHeadHeight()), _vrNodesOrigin.position.z);

            float hipsOffsetY = Mathf.Min(0, _verticalReference.transform.position.y - _initialVerticalReferencePosY);
            Transform tHips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            tHips.localPosition = _initialHipsLocalPosition;
            tHips.Translate(Vector3.up * hipsOffsetY, Space.World);

            if (_rotateWithCamera) CalibrateCameraForward();
        }

        protected override void UpdateTransformNodes()
        {
            base.UpdateTransformNodes();

            QuickIKSolver ikSolverHead = _ikManager.GetIKSolver(HumanBodyBones.Head);
            QuickTrackedObject tObjectHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            //_vrNodesOrigin.position = tObjectHead.transform.position + (_vrNodesOrigin.position - tObjectHead.transform.position).normalized * ikSolverHead.GetChainLength();

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                UpdateTransformNodeReferenceUserPose(t);
            }
            UpdateTrackingIK();
        }

        protected virtual void UpdateTransformNodeReferenceUserPose(QuickVRNode.Type nType)
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

        protected virtual void UpdateTrackingIK()
        {
            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKPos = _applyHeadPosition ? 1.0f : 0.0f;
            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKRot = _applyHeadRotation ? 1.0f : 0.0f;

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

    }

}
