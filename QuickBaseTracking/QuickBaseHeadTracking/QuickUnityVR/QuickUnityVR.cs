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

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected QuickIKManager _ikManager = null;

        protected float _initialVerticalReferencePosY = 0.0f;

        protected float _unscaledHeadHeight = 0.0f;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _headOffset = Quaternion.Inverse(transform.rotation) * (_animator.GetBoneTransform(HumanBodyBones.Head).position - GetEyeCenterPosition());

            //Create the IKManager
            if (!gameObject.GetComponent<QuickIKManager>())
            {
                //#if UNITY_2019_1_OR_NEWER 
                //                gameObject.AddComponent<QuickIKManager_v2>();
                //#else
                //                gameObject.AddComponent<QuickIKManager_v1>();
                //#endif
                gameObject.AddComponent<QuickIKManager_v1>();
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
        }

        protected override void CalibrateVRPlayArea()
        {
            base.CalibrateVRPlayArea();

            //Set the offset of the TrackedObject of the head
            QuickVRNode node = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head);
            if (!node) return;

            Vector3 offset = _ikManager.GetIKTarget(HumanBodyBones.Head).position - node.GetTrackedObject().transform.position;
            _vrPlayArea.transform.position += offset;
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

        protected override void UpdateTransformRoot()
        {
            base.UpdateTransformRoot();

            //if (_rotateWithCamera) CalibrateCameraForward();
        }

        protected override void UpdateTransformNodes()
        {
            base.UpdateTransformNodes();

            //1) Update all the nodes but the hips, which has to be treated differently. 
            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                QuickVRNode node = _vrPlayArea.GetVRNode(t);
                if (t == QuickVRNode.Type.Hips || !node) continue;

                HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(t.ToString());
                QuickTrackedObject tObject = node.GetTrackedObject();

                //Update the QuickVRNode's position
                if (node._updateModePos == QuickVRNode.UpdateMode.FromUser) UpdateTransformNodePosFromUser(node, boneID);
                else UpdateTransformNodePosFromCalibrationPose(node, boneID);

                //Update the QuickVRNode's rotation
                if (node._updateModeRot == QuickVRNode.UpdateMode.FromUser) UpdateTransformNodeRotFromUser(node, boneID);
                else UpdateTransformNodeRotFromCalibrationPose(node, boneID);
            }

            //2) Update the hips target if necessary
            if (_isStanding)
            {
                QuickVRNode vrNode = _vrPlayArea.GetVRNode(_vrPlayArea.IsTrackedNode(QuickVRNode.Type.Hips) ? QuickVRNode.Type.Hips : QuickVRNode.Type.Head);
                UpdateTransformNodePosFromCalibrationPose(vrNode, HumanBodyBones.Hips, Vector3.up);
                float maxY = _ikManager.GetInitialIKDataLocalPos(HumanBodyBones.Hips).y;
                Transform targetHips = _ikManager.GetIKTarget(HumanBodyBones.Hips);
                targetHips.localPosition = new Vector3(targetHips.localPosition.x, Mathf.Min(targetHips.localPosition.y, maxY), targetHips.localPosition.z);
            }

            UpdateTrackingIK();
        }

        protected virtual void UpdateTransformNodePosFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = _ikManager.GetIKTarget(boneID);
            if (!t) return;

            t.position = node.GetTrackedObject().transform.position;
        }

        protected virtual void UpdateTransformNodeRotFromUser(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = _ikManager.GetIKTarget(boneID);
            if (!t) return;

            t.rotation = node.GetTrackedObject().transform.rotation;
        }

        protected virtual void UpdateTransformNodePosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        {
            UpdateTransformNodePosFromCalibrationPose(node, boneID, Vector3.one);
        }

        protected virtual void UpdateTransformNodePosFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID, Vector3 offsetScale)
        {
            Transform t = _ikManager.GetIKTarget(boneID);
            Transform calibrationPose = node.GetCalibrationPose();
            if (!t || !calibrationPose) return;

            t.localPosition = _ikManager.GetInitialIKDataLocalPos(boneID);
            Vector3 offset = Vector3.Scale(node.GetTrackedObject().transform.position - calibrationPose.position, offsetScale);
            t.position += offset;
        }

        protected virtual void UpdateTransformNodeRotFromCalibrationPose(QuickVRNode node, HumanBodyBones boneID)
        {
            Transform t = _ikManager.GetIKTarget(boneID);
            Transform calibrationPose = node.GetCalibrationPose();
            if (!t || !calibrationPose) return;

            t.localRotation = _ikManager.GetInitialIKDataLocalRot(boneID);
            Quaternion rotOffset = node.GetTrackedObject().transform.rotation * Quaternion.Inverse(calibrationPose.rotation);
            t.rotation = rotOffset * t.rotation;
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
