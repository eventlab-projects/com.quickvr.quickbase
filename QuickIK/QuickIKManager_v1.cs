using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;

namespace QuickVR {

	public class QuickIKManager_v1 : QuickIKManager 
    {

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected Transform _boneRotator = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            _boneRotator = transform.CreateChild("__BoneRotator__");

            base.Reset();
        }

        protected override void CreateIKSolversBody()
        {
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                HumanBodyBones uBone = ToUnity(boneID);
                CreateIKSolver<QuickIKSolver>(uBone);
            }
        }

        protected override void CreateIKSolversHand(HumanBodyBones boneHandID)
        {
            Transform ikSolversRoot = boneHandID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
            Transform tBone = _animator.GetBoneTransform(boneHandID);

            Transform ikTargetsRoot = boneHandID == HumanBodyBones.LeftHand ? _ikTargetsLeftHand : _ikTargetsRightHand;
            ikTargetsRoot.position = tBone.position;
            ikTargetsRoot.rotation = tBone.rotation;

            string prefix = boneHandID.ToString().Contains("Left") ? "Left" : "Right";
            foreach (IKLimbBonesHand b in GetIKLimbBonesHand())
            {
                HumanBodyBones boneLimb = QuickUtils.ParseEnum<HumanBodyBones>(prefix + b.ToString() + "Distal");
                IQuickIKSolver ikSolver = CreateIKSolver<QuickIKSolver>(boneLimb);
                ikSolver._targetHint = CreateIKTarget(QuickHumanTrait.GetParentBone(boneLimb));
                ikSolver._targetHint.position += b.ToString().Contains("Thumb") ? transform.forward * 0.1f : transform.up * 0.1f;
            }
        }

        protected virtual T CreateIKSolver<T>(HumanBodyBones boneLimb) where T : MonoBehaviour, IQuickIKSolver 
        {
            T ikSolver = CreateIKSolverTransform(GetIKSolversRoot(boneLimb), boneLimb.ToString()).GetOrCreateComponent<T>();

            //And configure it according to the bone
            ikSolver._boneUpper = GetBoneUpper(boneLimb);
            ikSolver._boneMid = GetBoneMid(boneLimb);
            ikSolver._boneLimb = _animator.GetBoneTransform(boneLimb);

            ikSolver._targetLimb = CreateIKTarget(boneLimb);

            HumanBodyBones? midBoneID = GetIKTargetMidBoneID(boneLimb);
            if (midBoneID.HasValue)
            {
                ikSolver._targetHint = CreateIKTarget(midBoneID.Value);
            }
            ikSolver._boneID = boneLimb;

            return ikSolver;
        }

        protected override Transform CreateIKTarget(HumanBodyBones? boneID)
        {
            Transform ikTarget = base.CreateIKTarget(boneID);
            if (IsBoneLimb(boneID.Value))
            {
                //Create a child that will contain the real rotation of the bone
                ikTarget.CreateChild("__BoneRotation__").rotation = _animator.GetBoneTransform(boneID.Value).rotation;
            }

            return ikTarget;
        }

        #endregion

        #region GET AND SET

        protected virtual Transform GetBoneUpper(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Hips);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone(boneLimbID)));
        }

        protected virtual Transform GetBoneMid(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Hips);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(boneLimbID));
        }

        public override void Calibrate()
        {
            //QuickIKSolver ikSolver = null;
            foreach (IQuickIKSolver ikSolver in GetIKSolvers())
            {
                ResetIKSolver(ikSolver._boneID);
                QuickIKData initialIKData = _initialIKPose[ikSolver._boneID];
                ikSolver._targetLimb.localPosition = initialIKData._targetLimbLocalPosition;
                ikSolver._targetLimb.localRotation = initialIKData._targetLimbLocalRotation;
                ikSolver._targetHint.localPosition = initialIKData._targetHintLocalPosition;
            }

            //if (IsIKHintBoneActive(IKLimbBones.LeftHand))
            //{
            //    ikSolver = GetIKSolver(IKLimbBones.LeftHand);
            //    ikSolver._targetHint.position = ikSolver._boneMid.position - transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
            //}

            //if (IsIKHintBoneActive(IKLimbBones.RightHand))
            //{
            //    ikSolver = GetIKSolver(IKLimbBones.RightHand);
            //    ikSolver._targetHint.position = ikSolver._boneMid.position - transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
            //}

            base.Calibrate();
        }

        protected virtual bool IsIKLimbBoneActive(IKLimbBones boneID)
        {
            return ((_ikMaskBody & (1 << (int)boneID)) != 0);
        }

        public virtual void ResetIKSolver(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = (QuickIKSolver)GetIKSolver(boneID);
            if ((_ikMaskBody & (1 << (int)boneID)) != 0)
            {
                ikSolver.ResetIKChain();
            }
        }

        public virtual void ResetIKSolver(IKLimbBones boneID)
        {
            ResetIKSolver(ToUnity(boneID));
        }

        //public virtual void ResetIKChains()
        //{
        //    QuickIKSolver ikSolver = null;
        //    for (int i = 0; i < _ikLimbBones.Count; i++)
        //    {
        //        IKLimbBones boneID = _ikLimbBones[i];
        //        ikSolver = GetIKSolver(ToUnity(boneID));
        //        if (ikSolver && ((_ikMaskBody & (1 << i)) != 0))
        //        {
        //            ikSolver.ResetIKChain();
        //        }
        //    }
        //}

        #endregion

		#region UPDATE

        protected virtual void Update()
        {
            
        }

        public override void UpdateTracking() {
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                QuickIKSolver ikSolver = (QuickIKSolver)GetIKSolver(ToUnity(boneID));
                if (ikSolver && ((_ikMaskBody & (1 << (int)boneID)) != 0))
                {
                    //Correct the rotations of the limb bones by accounting for human body constraints
                    if (boneID == IKLimbBones.LeftHand || boneID == IKLimbBones.RightHand)
                    {
                        Transform tmpParent = ikSolver._targetLimb.parent;
                        ikSolver._targetLimb.parent = transform;
                        Vector3 localEuler = ikSolver._targetLimb.localEulerAngles;

                        float rotAngle = localEuler.z;
                        ikSolver._targetLimb.localRotation = Quaternion.Euler(localEuler.x, localEuler.y, rotAngle);

                        ikSolver.UpdateIK();

                        Vector3 rotAxis = (ikSolver._boneLimb.position - ikSolver._boneMid.position).normalized;

                        float boneMidWeight = 0.5f;
                        CorrectRotation(ikSolver._boneMid, rotAxis, rotAngle * boneMidWeight);
                        CorrectRotation(ikSolver._boneLimb, rotAxis, -rotAngle * (1.0f - boneMidWeight));

                        ikSolver._targetLimb.parent = tmpParent;
                        ikSolver._targetLimb.localScale = Vector3.one;
                    }
                    else
                    {
                        ikSolver.UpdateIK();
                    }
                }
            }

            Transform leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.position = leftHand.position;
            _ikTargetsLeftHand.rotation = leftHand.rotation;

            Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.position = rightHand.position;
            _ikTargetsRightHand.rotation = rightHand.rotation;

            foreach (IQuickIKSolver ikSolver in GetIKSolversLeftHand())
            {
                ((QuickIKSolver)ikSolver).UpdateIK();
            }

            foreach (IQuickIKSolver ikSolver in GetIKSolversRightHand())
            {
                ((QuickIKSolver)ikSolver).UpdateIK();
            }

            base.UpdateTracking();
        }

        protected virtual void CorrectRotation(Transform tBone, Vector3 rotAxis, float rotAngle)
        {
            _boneRotator.parent = tBone;
            _boneRotator.localPosition = Vector3.zero;
            _boneRotator.forward = rotAxis;
            Vector3 upBefore = _boneRotator.up;
            tBone.Rotate(rotAxis, rotAngle, Space.World);

            if (Vector3.Dot(upBefore, _boneRotator.up) < 0)
            {
                tBone.Rotate(rotAxis, 180.0f, Space.World);
            }
        }

        #endregion

    }

}