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
            CreateIKSolver<QuickIKSolverHips_v1>(HumanBodyBones.Hips);
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
            }
        }

        protected override Transform CreateIKTarget(HumanBodyBones? boneID)
        {
            Transform ikTarget = base.CreateIKTarget(boneID);
            string bName = boneID.Value.ToString();
            if (IsBoneLimb(boneID.Value))
            {
                //Create a child that will contain the real rotation of the bone
                if (!ikTarget.Find("__BoneRotation__"))
                {
                    ikTarget.CreateChild("__BoneRotation__").rotation = _animator.GetBoneTransform(boneID.Value).rotation;
                }
            }
            
            return ikTarget;
        }

        #endregion

        #region GET AND SET

        protected virtual bool IsIKLimbBoneActive(IKLimbBones boneID)
        {
            return ((_ikMaskBody & (1 << (int)boneID)) != 0);
        }

        public override void ResetIKSolver(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = (QuickIKSolver)GetIKSolver(boneID);
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