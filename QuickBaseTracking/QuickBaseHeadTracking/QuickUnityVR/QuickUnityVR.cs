using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

    public class QuickUnityVR : QuickUnityVRBase
    {

        #region PUBLIC ATTRIBUTES

        public bool _applyUserScale = false;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected QuickIKManager _ikManager = null;

        protected float _unscaledHeadHeight = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            QuickVRNode.OnCalibrateVRNodeLeftFoot += OnCalibrateVRNodeFoot;
            QuickVRNode.OnCalibrateVRNodeRightFoot += OnCalibrateVRNodeFoot;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            QuickVRNode.OnCalibrateVRNodeLeftFoot -= OnCalibrateVRNodeFoot;
            QuickVRNode.OnCalibrateVRNodeRightFoot -= OnCalibrateVRNodeFoot;
        }

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

            //Set the offset of the TrackedObject of the head
            QuickVRNode node = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head);
            Vector3 offset = _ikManager.GetIKTarget(HumanBodyBones.Head).position - node.GetTrackedObject().transform.position;
            _vrPlayArea.transform.position += offset;
        }

        protected virtual void OnCalibrateVRNodeFoot(QuickVRNode node)
        {
            HumanBodyBones boneID = QuickVRNode.ToHumanBodyBone(node.GetRole());
            Transform ikTarget = _ikManager.GetIKTarget(boneID);
            QuickTrackedObject tObject = node.GetTrackedObject();
            tObject.transform.rotation = ikTarget.rotation;
        }

        public override Vector3 GetEyeCenterPosition()
        {
            return _animator.GetEyeCenterPosition();
        }

        #endregion

        #region UPDATE

        protected override void UpdateTransformNodes()
        {
            base.UpdateTransformNodes();

            //1) Update all the nodes but the hips, which has to be treated differently. 
            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                QuickVRNode node = _vrPlayArea.GetVRNode(t);
                if (!node.IsTracked()) continue;
                
                HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(t.ToString());
                QuickTrackedObject tObject = node.GetTrackedObject();

                //Update the QuickVRNode's position
                if (node._updateModePos == QuickVRNode.UpdateMode.FromUser) UpdateTransformNodePosFromUser(node, boneID);
                else UpdateTransformNodePosFromCalibrationPose(node, boneID);

                //Update the QuickVRNode's rotation
                if (node._updateModeRot == QuickVRNode.UpdateMode.FromUser) UpdateTransformNodeRotFromUser(node, boneID);
                else UpdateTransformNodeRotFromCalibrationPose(node, boneID);
            }

            //2) Special case: The user is standing but there is no tracker on the hips. So the hips position is estimated by the movement of the head
            QuickVRNode nodeHips = _vrPlayArea.GetVRNode(QuickVRNode.Type.Hips);
            if (_isStanding && !nodeHips.IsTracked())
            {
                QuickVRNode vrNode = _vrPlayArea.GetVRNode(QuickVRNode.Type.Head);
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

            _ikManager.UpdateTracking();
        }

        #endregion

    }

}
