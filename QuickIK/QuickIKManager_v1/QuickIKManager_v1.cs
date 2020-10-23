using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;

namespace QuickVR {

	public class QuickIKManager_v1 : QuickIKManager 
    {

        #region PROTECTED PARAMETERS

        protected Transform _boneRotator 
        {
            get
            {
                return transform.CreateChild("__BoneRotator__");
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void CreateIKSolversBody()
        {
            CreateIKSolver<QuickIKSolverHips_v1>(HumanBodyBones.Hips);
            CreateIKSolver<QuickIKSolver>(HumanBodyBones.Head);
            CreateIKSolver<QuickIKSolverHand_v1>(HumanBodyBones.LeftHand);
            CreateIKSolver<QuickIKSolverHand_v1>(HumanBodyBones.RightHand);
            CreateIKSolver<QuickIKSolver>(HumanBodyBones.LeftFoot);
            CreateIKSolver<QuickIKSolver>(HumanBodyBones.RightFoot);
        }

        //protected override void CreateIKSolversHand(HumanBodyBones boneHandID)
        //{
        //    Transform ikSolversRoot = boneHandID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
        //    Transform tBone = _animator.GetBoneTransform(boneHandID);

        //    Transform ikTargetsRoot = boneHandID == HumanBodyBones.LeftHand ? _ikTargetsLeftHand : _ikTargetsRightHand;
        //    ikTargetsRoot.position = tBone.position;
        //    ikTargetsRoot.rotation = tBone.rotation;

        //    string prefix = boneHandID.ToString().Contains("Left") ? "Left" : "Right";
        //    foreach (IKLimbBonesHand b in GetIKLimbBonesHand())
        //    {
        //        HumanBodyBones boneLimb = QuickUtils.ParseEnum<HumanBodyBones>(prefix + b.ToString() + "Distal");
        //        IQuickIKSolver ikSolver = CreateIKSolver<QuickIKSolver>(boneLimb);
        //        ikSolver._targetHint = CreateIKTarget(QuickHumanTrait.GetParentBone(boneLimb));
        //    }
        //}

        protected override Transform CreateIKTarget(HumanBodyBones? boneID)
        {
            Transform ikTarget = base.CreateIKTarget(boneID);
            string bName = boneID.Value.ToString();
            if (IsBoneLimb(boneID.Value))
            {
                //Create a child that will contain the real rotation of the bone
                ikTarget.CreateChild("__BoneRotation__").rotation = _animator.GetBoneTransform(boneID.Value).rotation;
            }
            
            return ikTarget;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            QuickUtils.Destroy(_boneRotator);
        }

        #endregion

        #region GET AND SET

        protected virtual bool IsIKLimbBoneActive(IKLimbBones boneID)
        {
            return ((_ikMaskBody & (1 << (int)boneID)) != 0);
        }

        public override void ResetIKSolver(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = GetIKSolver<QuickIKSolver>(boneID);
            if (ikSolver == null)
            {
                Debug.Log(boneID);
            }
            if ((_ikMaskBody & (1 << (int)boneID)) != 0)
            {
                ikSolver.ResetIKChain();
            }
            base.ResetIKSolver(boneID);
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

        public override void UpdateTrackingLate() 
        {
            if (IsTrackedIKLimbBone(IKLimbBones.Hips))
            {
                QuickIKSolver ikSolverHips = GetIKSolver<QuickIKSolver>(HumanBodyBones.Hips);
                ikSolverHips.UpdateIK();
            }

            if (IsTrackedIKLimbBone(IKLimbBones.Head))
            {
                QuickIKSolver ikSolverHead = GetIKSolver<QuickIKSolver>(HumanBodyBones.Head);
                ikSolverHead.UpdateIK();
            }

            if (IsTrackedIKLimbBone(IKLimbBones.Hips))
            {
                QuickIKSolver ikSolverHips = GetIKSolver<QuickIKSolver>(HumanBodyBones.Hips);
                ikSolverHips._targetLimb.position += GetIKTargetHipsOffset();
                ikSolverHips.UpdateIK();
            }

            List<HumanBodyBones> ikLimbBones = GetIKLimbBones();
            for (int i = (int)IKLimbBones.LeftHand; i <= (int)IKLimbBones.RightFoot; i++)
            {
                QuickIKSolver ikSolver = GetIKSolver<QuickIKSolver>(ikLimbBones[i]);
                if (ikSolver && ((_ikMaskBody & (1 << i)) != 0))
                {
                    //ikSolver.ResetIKChain();
                    //Correct the rotations of the limb bones by accounting for human body constraints
                    ikSolver.UpdateIK();
                    if (i == (int)IKLimbBones.LeftHand || i == (int)IKLimbBones.RightHand)
                    {
                        Vector3 localEuler = ikSolver._targetLimb.localEulerAngles;
                        float rotAngle = localEuler.z;
                        Vector3 rotAxis = (ikSolver._boneLimb.position - ikSolver._boneMid.position).normalized;

                        float boneMidWeight = 0.5f;
                        Quaternion limbRot = ikSolver._boneLimb.rotation;
                        CorrectRotation(ikSolver._boneMid, rotAxis, rotAngle * boneMidWeight);
                        ikSolver._boneLimb.rotation = limbRot;
                    }
                }
            }

            base.UpdateTrackingLate();
        }

        protected virtual void CorrectRotation(Transform tBone, Vector3 rotAxis, float rotAngle)
        {
            _boneRotator.forward = rotAxis;
            Vector3 upBefore = _boneRotator.up;
            _boneRotator.Rotate(rotAxis, rotAngle, Space.World);
            if (Vector3.Dot(upBefore, _boneRotator.up) < 0)
            {
                rotAngle += 180.0f;
            }
            tBone.Rotate(rotAxis, rotAngle, Space.World);
        }

        #endregion

    }

}