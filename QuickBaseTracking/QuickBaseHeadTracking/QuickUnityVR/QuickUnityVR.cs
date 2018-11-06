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

        public enum UpdateReference
        {
            UserPose,
            CalibrationPose,
        }
        public UpdateReference _updateReference = UpdateReference.UserPose;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected QuickIKManager _ikManager = null;

        protected Vector3 _initialHipsLocalPosition = Vector3.zero;
        protected QuickTrackedObject _verticalReference = null;
        protected float _initialVerticalReferencePosY = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _initialHipsLocalPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
            
            //Create the IKManager
            _ikManager = gameObject.GetOrCreateComponent<QuickIKManager>();
            _ikManager.enabled = false; //We control when to update the IK

            //TEMPORARY SOLUTION FOR FEET ROTATION
            _ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            _ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
            _ikManager.GetIKSolver(HumanBodyBones.LeftFoot)._weightIKRot = 0.0f;
            _ikManager.GetIKSolver(HumanBodyBones.RightFoot)._weightIKRot = 0.0f;
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

        protected override QuickVRNode CreateVRNode(QuickVRNode.Type n)
        {
            _vrNodesOrigin.CreateChild("_CalibrationPose_" + n.ToString());

            return base.CreateVRNode(n);
        }

        #endregion

        #region GET AND SET

        protected Transform GetCalibrationPose(QuickVRNode.Type tNode)
        {
            return _vrNodesOrigin.Find("_CalibrationPose_" + tNode.ToString());
        }

        public override void Calibrate()
        {
            _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = _initialHipsLocalPosition;

            _ikManager.Calibrate();

            base.Calibrate();

            QuickVRNode nodeWaist = GetQuickVRNode(QuickVRNode.Type.Waist);
            _verticalReference = nodeWaist.IsTracked() ? nodeWaist.GetTrackedObject() : GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            _initialVerticalReferencePosY = _verticalReference.transform.position.y;
        }

        protected override void CalibrateVRNode(QuickVRNode.Type nodeType)
        {
            base.CalibrateVRNode(nodeType);

            QuickVRNode node = GetQuickVRNode(nodeType);
            QuickTrackedObject tObject = node.GetTrackedObject();

            Transform ikTarget = _ikManager.GetIKSolver(QuickUtils.ParseEnum<IKLimbBones>(nodeType.ToString()))._targetLimb;
            if ((nodeType == QuickVRNode.Type.Head) || (_updateReference == UpdateReference.UserPose))
            {
                tObject.transform.rotation = node.transform.rotation;
                
                //Correct the hands rotation
                if (nodeType == QuickVRNode.Type.LeftHand) tObject.transform.Rotate(tObject.transform.forward, 90.0f, Space.World);
                else if (nodeType == QuickVRNode.Type.RightHand) tObject.transform.Rotate(tObject.transform.forward, -90.0f, Space.World);

                //Correct the feet rotation
                else if (nodeType == QuickVRNode.Type.LeftFoot)
                {
                    //float rotAngle = Vector3.Angle(tObject.transform.forward, transform.forward);
                    //Vector3 rotAxis = Vector3.Cross(tObject.transform.forward, transform.forward).normalized;
                    //tObject.transform.Rotate(rotAxis, rotAngle, Space.World);

                    //tObject.transform.rotation = transform.rotation;

                    tObject.transform.rotation = Quaternion.identity; //_ikManager.GetIKCalibrationTarget(HumanBodyBones.LeftFoot).rotation;
                }

                //Set the position of the hips
                if (nodeType == QuickVRNode.Type.Waist)
                {
                    QuickTrackedObject tObjectHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
                    tObject.transform.position = new Vector3(tObjectHead.transform.position.x, tObject.transform.position.y, tObjectHead.transform.position.z);
                }
            }
            else
            {
                tObject.transform.rotation = _vrNodesOrigin.rotation * Quaternion.Inverse(transform.rotation) * ikTarget.rotation;
            }

            //Initialize the calibration pose of the target with the current position and rotation of the tracked object. 
            Transform cPose = GetCalibrationPose(nodeType);
            cPose.position = tObject.transform.position;
            cPose.rotation = tObject.transform.rotation;
            tObject.Reset();
        }

        protected override void CalibrateVRNodeHead(QuickVRNode node)
        {
            QuickTrackedObject tObject = node.GetTrackedObject();

            tObject.transform.localPosition = Quaternion.Inverse(transform.rotation) * (_animator.GetBoneTransform(HumanBodyBones.Head).position - GetEyeCenterPosition());
            _vrNodesOrigin.position = tObject.transform.position - transform.up * _ikManager.GetIKSolver(HumanBodyBones.Head).GetChainLength();

            base.CalibrateVRNodeHead(node);
        }

        protected override Vector3 GetDisplacement()
        {
            QuickVRNode waistNode = GetQuickVRNode(QuickVRNode.Type.Waist);
            if (waistNode.IsTracked()) return waistNode.GetTrackedObject().GetDisplacement();
            else if (_displaceWithCamera) return GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().GetDisplacement();

            return Vector3.zero;
        }

        protected override float GetRotationOffset()
        {
            QuickVRNode waistNode = GetQuickVRNode(QuickVRNode.Type.Waist);
            QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);
            QuickVRNode node = null;

            if (waistNode.IsTracked()) node = waistNode;
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

            float hipsOffsetY = Mathf.Min(0, _verticalReference.transform.position.y - _initialVerticalReferencePosY);
            Transform tHips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            tHips.localPosition = _initialHipsLocalPosition;
            tHips.Translate(Vector3.up * hipsOffsetY, Space.World);

            if (_rotateWithCamera) CalibrateCameraForward();
        }

        protected override void UpdateTransformNodes()
        {
            QuickIKSolver ikSolverHead = _ikManager.GetIKSolver(HumanBodyBones.Head);
            QuickTrackedObject tObjectHead = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
            //_vrNodesOrigin.position = tObjectHead.transform.position + (_vrNodesOrigin.position - tObjectHead.transform.position).normalized * ikSolverHead.GetChainLength();

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                if (_updateReference == UpdateReference.UserPose) UpdateTransformNodeReferenceUserPose(t);
                else UpdateTransformNodeReferenceCalibrationPose(t);
            }
            UpdateTrackingIK();
        }

        protected virtual void UpdateTransformNodeReferenceCalibrationPose(QuickVRNode.Type nType)
        {
            QuickVRNode node = GetQuickVRNode(nType);
            if (node.IsTracked() && QuickUtils.IsEnumValue<HumanBodyBones>(nType.ToString()))
            {
                QuickTrackedObject tObject = node.GetTrackedObject();
                HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(nType.ToString());
                QuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
                _ikManager.ResetIKSolver(boneID);

                Vector3 posOffset = tObject.transform.position - GetCalibrationPose(nType).position;

                //tObject.transform.rotation = nodeType == QuickVRNode.Type.Head ? node.transform.rotation : _vrNodesOrigin.rotation * Quaternion.Inverse(transform.rotation) * ikTarget.rotation;
                ikSolver._targetLimb.position += transform.rotation * Quaternion.Inverse(_vrNodesOrigin.rotation) * posOffset;
                ikSolver._targetLimb.rotation = transform.rotation * Quaternion.Inverse(_vrNodesOrigin.rotation) * tObject.transform.rotation;
            }
        }

        protected virtual void UpdateTransformNodeReferenceUserPose(QuickVRNode.Type nType)
        {
            QuickVRNode node = GetQuickVRNode(nType);
            if (node.IsTracked() && QuickUtils.IsEnumValue<HumanBodyBones>(nType.ToString()))
            {
                Transform hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
                QuickIKSolver ikSolver = _ikManager.GetIKSolver(QuickUtils.ParseEnum<HumanBodyBones>(nType.ToString()));
                
                QuickTrackedObject tObject = node.GetTrackedObject();
                Vector3 posOffset = tObject.transform.position - _vrNodesOrigin.position;
                ikSolver._targetLimb.position = hips.position + transform.rotation * Quaternion.Inverse(_vrNodesOrigin.rotation) * posOffset;
                ikSolver._targetLimb.rotation = transform.rotation * Quaternion.Inverse(_vrNodesOrigin.rotation) * tObject.transform.rotation;
            }
        }

        protected virtual void UpdateTrackingIK()
        {
            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKPos = _applyHeadPosition ? 1.0f : 0.0f;
            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKRot = _applyHeadRotation ? 1.0f : 0.0f;

            _ikManager.UpdateTracking();
        }

        #endregion

    }

}
