using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace QuickVR {

    public class QuickCopyPoseDirect : QuickCopyPoseBase 
    {

        #region UPDATE

        protected override void CopyPoseImp()
        {
            Quaternion tmpRot = _source.transform.rotation;
            _source.transform.rotation = _dest.transform.rotation;

            CopyPose(QuickHumanBodyBones.Hips);
            CopyPose(QuickHumanBodyBones.Spine);
            CopyPoseChainLimb(QuickHumanBodyBones.Head);

            CopyPose(QuickHumanBodyBones.LeftUpperArm);
            CopyPose(QuickHumanBodyBones.LeftLowerArm);
            CopyPoseChainLimb(QuickHumanBodyBones.LeftHand);

            CopyPose(QuickHumanBodyBones.RightUpperArm);
            CopyPose(QuickHumanBodyBones.RightLowerArm);
            CopyPoseChainLimb(QuickHumanBodyBones.RightHand);

            CopyPose(QuickHumanBodyBones.LeftUpperLeg);
            CopyPose(QuickHumanBodyBones.LeftLowerLeg);
            CopyPoseChainLimb(QuickHumanBodyBones.LeftFoot);

            CopyPose(QuickHumanBodyBones.RightUpperLeg);
            CopyPose(QuickHumanBodyBones.RightLowerLeg);
            CopyPoseChainLimb(QuickHumanBodyBones.RightFoot);

            for (QuickHumanBodyBones fingerBoneID = QuickHumanBodyBones.LeftThumbProximal; fingerBoneID <= QuickHumanBodyBones.RightLittleDistal; fingerBoneID++)
            {
                CopyPose(fingerBoneID);
            }

            _source.transform.rotation = tmpRot;
        }

        protected virtual void CopyPose(QuickHumanBodyBones boneID)
        {
            QuickHumanBodyBones nextBoneID = QuickHumanTrait.GetNextBoneInChain(boneID);
            Transform tBoneSrc = _source.GetBoneTransform(boneID);
            Transform tBoneSrcNext = _source.GetBoneTransform(nextBoneID);
            Transform tBoneDst = _dest.GetBoneTransform(boneID);
            Transform tBoneDstNext = _dest.GetBoneTransform(nextBoneID);

            Vector3 currentDir = tBoneDstNext.position - tBoneDst.position;
            Vector3 targetDir = tBoneSrcNext.position - tBoneSrc.position;

            Vector3 rotAxis = Vector3.Cross(currentDir, targetDir);
            float rotAngle = Vector3.Angle(currentDir, targetDir);

            tBoneDst.Rotate(rotAxis, rotAngle, Space.World);
        }

        protected virtual void CopyPoseChainLimb(QuickHumanBodyBones boneID)
        {
            Transform tRotSrc = _source.GetRotationReference(boneID);
            Transform tRotDst = _dest.GetRotationReference(boneID);

            Transform tBoneDst = _dest.GetBoneTransform(boneID);
            tBoneDst.Rotate(Vector3.Cross(tRotDst.forward, tRotSrc.forward), Vector3.Angle(tRotDst.forward, tRotSrc.forward), Space.World);
            tBoneDst.Rotate(Vector3.Cross(tRotDst.up, tRotSrc.up), Vector3.Angle(tRotDst.up, tRotSrc.up), Space.World);
        }

        #endregion

    }

}
