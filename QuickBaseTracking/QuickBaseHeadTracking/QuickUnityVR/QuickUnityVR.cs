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

        protected override void CalibrateVRNode(QuickVRNode.Type nodeType)
        {
            base.CalibrateVRNode(nodeType);

            QuickTrackedObject tObject = GetQuickVRNode(nodeType).GetTrackedObject();
            Transform cPose = GetCalibrationPose(nodeType);
            cPose.position = tObject.transform.position;
            cPose.rotation = tObject.transform.rotation;
            tObject.Reset();
        }

        protected override void CalibrateVRNodeHead(QuickVRNode node)
        {
            base.CalibrateVRNodeHead(node);

            //Set the offset of the TrackedObject of the head
            QuickTrackedObject tObject = node.GetTrackedObject();
            IQuickIKSolver ikSolverHead = _ikManager.GetIKSolver(IKLimbBones.Head);
            tObject.transform.localPosition = Quaternion.Inverse(transform.rotation) * (ikSolverHead._boneLimb.position - GetEyeCenterPosition());

            //Set the position of the vrNodesOrigin
            Vector3 offset1 = ikSolverHead._boneUpper.position - ikSolverHead._targetLimb.position;
            Vector3 offset2 = transform.position - ikSolverHead._boneUpper.position;
            _vrNodesOrigin.position = tObject.transform.position + ToTrackingSpace(offset1 + offset2);
        }

        public override Vector3 GetDisplacement()
        {
            if (_isStanding)
            {
                QuickVRNode HipsNode = GetQuickVRNode(QuickVRNode.Type.Hips);
                if (HipsNode.IsTracked()) return HipsNode.GetTrackedObject().GetDisplacement();
                else if (_displaceWithCamera) return GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().GetDisplacement();
            }
            
            return Vector3.zero;
        }

        protected override float GetRotationOffset()
        {
            if (_isStanding)
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

        protected override void UpdateTransformRoot()
        {
            base.UpdateTransformRoot();

            Transform t = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().transform;
            float h = t.position.y - _vrNodesOrigin.position.y;
            _vrNodesOrigin.position = new Vector3(_vrNodesOrigin.position.x, t.position.y - Mathf.Min(h, GetHeadHeight()), _vrNodesOrigin.position.z);

            if (_isStanding)
            {
                float hipsOffsetY = Mathf.Min(0, _verticalReference.transform.position.y - _initialVerticalReferencePosY);
                Transform tHips = _animator.GetBoneTransform(HumanBodyBones.Hips);
                tHips.localPosition = _initialHipsLocalPosition;
                tHips.Translate(Vector3.up * hipsOffsetY, Space.World);
            }
            
            if (_rotateWithCamera) CalibrateCameraForward();
        }

        protected override void UpdateTransformNodes()
        {
            base.UpdateTransformNodes();

            foreach (QuickVRNode.Type t in QuickVRNode.GetTypeList())
            {
                UpdateTransformNode(t);
            }

            UpdateTrackingIK();
        }

        protected virtual void UpdateTransformNode(QuickVRNode.Type nType)
        {
            if (!IsNodeTracked(nType)) return;

            QuickVRNode node = GetQuickVRNode(nType);
            HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(nType.ToString());
            QuickTrackedObject tObject = node.GetTrackedObject();

            IQuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
            if (ikSolver == null) return;

            QuickIKData initialIKData = _ikManager.GetInitialIKData(boneID);
            Transform t = null;
            Vector3 initialLocalPos = Vector3.zero;
            Quaternion initialLocalRot = Quaternion.identity;
            if (QuickIKManager.IsBoneLimb(boneID))
            {
                t = ikSolver._targetLimb;
                initialLocalPos = initialIKData._targetLimbLocalPosition;
                initialLocalRot = initialIKData._targetLimbLocalRotation;
            }
            else if (QuickIKManager.IsBoneMid(boneID))
            {
                t = ikSolver._targetHint;
                initialLocalPos = initialIKData._targetHintLocalPosition;
            }

            if (!t) return;

            Transform calibrationPose = GetCalibrationPose(nType);
            if (!calibrationPose) return;

            Quaternion tmp = transform.rotation;
            transform.rotation = _vrNodesOrigin.rotation;

            t.localPosition = initialLocalPos;
            t.localRotation = initialLocalRot;

            Vector3 offset = tObject.transform.position - calibrationPose.position;
            t.position += offset;

            Quaternion rotOffset = tObject.transform.rotation * Quaternion.Inverse(calibrationPose.rotation);
            t.rotation = rotOffset * t.rotation;
            transform.rotation = tmp;
        }

        //protected virtual void UpdateTransformNode(QuickVRNode.Type nType)
        //{
        //    if (!IsNodeTracked(nType)) return;

        //    QuickVRNode node = GetQuickVRNode(nType);
        //    HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(nType.ToString());
        //    QuickTrackedObject tObject = node.GetTrackedObject();
        //    Vector3 posOffset = tObject.transform.position - _vrNodesOrigin.position;

        //    IQuickIKSolver ikSolver = _ikManager.GetIKSolver(boneID);
        //    if (ikSolver == null) return;

        //    Transform t = null;
        //    if (QuickIKManager.IsBoneLimb(boneID))
        //    {
        //        t = ikSolver._targetLimb;
        //    }
        //    else if (QuickIKManager.IsBoneMid(boneID))
        //    {
        //        t = ikSolver._targetHint;
        //        posOffset += node.transform.forward * 0.25f;
        //    }

        //    if (!t) return;

        //    t.position = transform.position + ToAvatarSpace(posOffset);
        //    t.rotation = ToAvatarSpace(tObject.transform.rotation);
        //}

        protected virtual void UpdateTrackingIK()
        {
            //_ikManager._ikMask = _trackedJoints;
            //Check if the IKHint targets are actually tracked. 
            foreach (IKLimbBones b in QuickIKManager.GetIKLimbBones())
            {
                HumanBodyBones boneLimbID = QuickIKManager.ToUnity(b);
                HumanBodyBones boneMidID = QuickHumanTrait.GetParentBone(boneLimbID);
                IQuickIKSolver ikSolver = _ikManager.GetIKSolver(boneLimbID);
                QuickVRNode nodeMid = GetQuickVRNode(boneMidID);
                if (nodeMid && nodeMid.IsTracked())
                {
                    _ikManager._ikHintMaskUpdate &= ~(1 << (int)boneLimbID);
                    //ikSolver.ResetIKChain();

                    //Compute the rotation of the Bone Upper
                    HumanBodyBones boneUpperID = QuickHumanTrait.GetParentBone(boneMidID);
                    Vector3 userBoneMidPos = GetQuickVRNode(boneMidID).GetTrackedObject().transform.position;
                    Vector3 userBoneUpperPos = GetQuickVRNode(boneUpperID).GetTrackedObject().transform.position;

                    Vector3 currentBoneDir = ikSolver._boneMid.position - ikSolver._boneUpper.position;
                    Vector3 targetBoneDir = ToAvatarSpace(userBoneMidPos - userBoneUpperPos);

                    Vector3 rotAxis = Vector3.Cross(currentBoneDir, targetBoneDir).normalized;
                    float rotAngle = Vector3.Angle(currentBoneDir, targetBoneDir);
                    ikSolver._boneUpper.Rotate(rotAxis, rotAngle, Space.World);

                    //Compute the rotation of the Bone Mid
                    Vector3 userBoneLimbPos = GetQuickVRNode(boneLimbID).GetTrackedObject().transform.position;

                    currentBoneDir = ikSolver._boneLimb.position - ikSolver._boneMid.position;
                    targetBoneDir = ToAvatarSpace(userBoneLimbPos - userBoneMidPos);

                    rotAxis = Vector3.Cross(currentBoneDir, targetBoneDir).normalized;
                    rotAngle = Vector3.Angle(currentBoneDir, targetBoneDir);
                    ikSolver._boneMid.Rotate(rotAxis, rotAngle, Space.World);

                    ikSolver._weightIKPos = 0.0f;
                }
                else ikSolver._weightIKPos = 1.0f;
            }

            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKPos = _applyHeadPosition ? 1.0f : 0.0f;
            _ikManager.GetIKSolver(HumanBodyBones.Head)._weightIKRot = _applyHeadRotation ? 1.0f : 0.0f;

            _ikManager.UpdateTracking();
        }

        #endregion

        #region DEBUG

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Application.isPlaying)
            {
                DebugVRNodeCalibrationPose(QuickVRNode.Type.Head, Color.grey);
                DebugVRNodeCalibrationPose(QuickVRNode.Type.LeftHand, Color.blue);
                DebugVRNodeCalibrationPose(QuickVRNode.Type.LeftFoot, Color.cyan);
                DebugVRNodeCalibrationPose(QuickVRNode.Type.RightHand, Color.red);
                DebugVRNodeCalibrationPose(QuickVRNode.Type.RightFoot, Color.magenta);
                DebugVRNodeCalibrationPose(QuickVRNode.Type.Hips, Color.black);
            }
        }

        protected virtual void DebugVRNodeCalibrationPose(QuickVRNode.Type nType, Color color, float scale = 0.05f)
        {
            Transform t = GetCalibrationPose(nType);
            QuickVRNode qNode = GetQuickVRNode(nType);

            Gizmos.color = color;
            Gizmos.DrawWireSphere(t.position, scale * 0.5f);
        }

        #endregion

    }

}
