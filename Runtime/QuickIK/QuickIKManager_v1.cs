using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

	public class QuickIKManager_v1 : QuickIKManager 
    {

        #region CREATION AND DESTRUCTION

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

        #endregion

        #region GET AND SET

        //protected virtual bool IsIKLimbBoneActive(IKLimbBones boneID)
        //{
        //    return ((_ikMaskBody & (1 << (int)boneID)) != 0);
        //}

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

    }

}