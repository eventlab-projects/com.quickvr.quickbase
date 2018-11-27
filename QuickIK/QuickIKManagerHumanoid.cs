using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;

namespace QuickVR {

	public class QuickIKManagerHumanoid : QuickBaseTrackingManager 
    {

        #region PUBLIC PARAMETERS

        public bool _ikActive = true;

        [BitMask(typeof(IKLimbBones))]
        public int _ikMask = -1;

        #endregion

        #region PROTECTED PARAMETERS

        protected static List<IKLimbBones> _ikLimbBones = null;

        protected Transform _ikTargetsRoot = null;
        protected Transform _ikSolversRoot = null;
        protected Transform _ikCalibrationTargetsRoot = null;

        protected HumanPoseHandler _srcPoseHandler = null;
        protected HumanPose _srcPose = new HumanPose();

        #endregion

        #region CONSTANTS

        private static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            base.Reset();

            _ikTargetsRoot = transform.CreateChild("__IKTargets__");
            _ikSolversRoot = transform.CreateChild("__IKSolvers__");

            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                HumanBodyBones uBone = ToUnity(boneID);
                QuickIKSolver ikSolver = CreateIKSolver(uBone);
                ikSolver.enabled = false;
            }

            _srcPoseHandler = new HumanPoseHandler(_animator.avatar, _animator.transform);
            _srcPoseHandler.GetHumanPose(ref _srcPose);
        }

        protected override void Awake() {
            base.Awake();

            Reset();

            CreateCalibrationTargets();
        }

        protected virtual void CreateCalibrationTargets()
        {
            _ikCalibrationTargetsRoot = transform.CreateChild("__IKCalibrationTargets__");
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                CreateCalibrationTarget(ToUnity(boneID));
            }
        }

        protected virtual void CreateCalibrationTarget(HumanBodyBones boneID)
        {
            Transform cTarget = _ikCalibrationTargetsRoot.CreateChild("_IKCalibrationTarget_" + boneID.ToString());
            QuickIKSolver ikSolver = GetIKSolver(boneID);
            if (boneID == HumanBodyBones.Head)
            {
                cTarget.position = GetBoneLimb(boneID).position;
            }
            else
            {
                cTarget.position = ikSolver._boneUpper.position - transform.up * ikSolver.GetChainLength();
            }
        }

        protected virtual QuickIKSolver CreateIKSolver(HumanBodyBones boneID) 
        {
            string tName = "_IKSolver_" + boneID.ToString();
            Transform t = _ikSolversRoot.CreateChild(tName);
            QuickIKSolver ikSolver = t.gameObject.GetOrCreateComponent<QuickIKSolver>();

            //And configure it according to the bone
            ikSolver._boneUpper = GetBoneUpper(boneID);
            ikSolver._boneMid = GetBoneMid(boneID);
            ikSolver._boneLimb = GetBoneLimb(boneID);

            ikSolver._targetLimb = CreateIKTarget(boneID);

            HumanBodyBones? midBoneID = GetIKTargetMidBoneID(boneID);
            if (midBoneID.HasValue)
            {
                ikSolver._targetHint = CreateIKTarget(midBoneID.Value);
            }

            return ikSolver;
        }

        protected virtual Transform CreateIKTarget(HumanBodyBones? boneID) {
			if (!boneID.HasValue) return null;

            Transform ikParent = GetIKTargetParent(boneID.Value);
            string ikTargetName = "_IKTarget_" + boneID.ToString();
            Transform ikTarget = ikParent.Find(ikTargetName);

            //Create the ikTarget if necessary
            if (!ikTarget)
            {
                ikTarget = ikParent.CreateChild(ikTargetName);
                Transform bone = GetBoneLimb(boneID.Value);

                //Set the position of the IKTarget
                ikTarget.position = bone.position;
                if (boneID.Value.ToString().Contains("LowerArm") || boneID.Value.ToString().Contains("Spine"))
                {
                    ikTarget.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                if (boneID.Value.ToString().Contains("LowerLeg"))
                {
                    ikTarget.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }

                //Set the rotation of the IKTarget
                ikTarget.rotation = transform.rotation;
                if (boneID.Value == HumanBodyBones.LeftHand)
                {
                    ikTarget.LookAt(bone.position - transform.right, transform.up);
                }
                else if (boneID.Value == HumanBodyBones.RightHand)
                {
                    ikTarget.LookAt(bone.position + transform.right, transform.up);
                }

                if (IsBoneLimb(boneID.Value))
                {
                    //Create a child that will contain the real rotation of the bone
                    ikTarget.CreateChild("__BoneRotation__").rotation = bone.rotation;
                }
            }

			return ikTarget;
		}

        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            return _ikTargetsRoot;
        }

        public virtual Transform GetIKTargetsRoot()
        {
            return _ikTargetsRoot;
        }

        #endregion

        #region GET AND SET

        public static List<IKLimbBones> GetIKLimbBones()
        {
            if (_ikLimbBones == null)
            {
                _ikLimbBones = QuickUtils.GetEnumValues<IKLimbBones>();
            }

            return _ikLimbBones;
        }

        public virtual bool IsIKLimbBoneTracked(IKLimbBones ikLimbBone)
        {
            return (_ikMask & (1 << (int)ikLimbBone)) != 0;
        }

        protected override int GetDefaultPriority()
        {
            return QuickBodyTracking.DEFAULT_PRIORITY_TRACKING_BODY - 1;
        }

        public static HumanBodyBones ToUnity(IKLimbBones boneID)
        {
            return QuickUtils.ParseEnum<HumanBodyBones>(boneID.ToString());
        }

        public override void Calibrate()
        {
            List<IKLimbBones> ikLimbBones = GetIKLimbBones();
            for (int i = 0; i < ikLimbBones.Count; i++)
            {
                IKLimbBones boneID = ikLimbBones[i];
                QuickIKSolver ikSolver = GetIKSolver(ToUnity(boneID));
                if (ikSolver && ((_ikMask & (1 << i)) != 0))
                {
                    ikSolver.ResetIKChain();

                    //if (boneID != IKLimbBones.Head)
                    //{
                    //    ikSolver._offsetTargetLimbPos = GetIKCalibrationTarget(ToUnity(boneID)).position - ikSolver._targetLimb.position;
                    //}
                }
            }

            base.Calibrate();
        }

        public virtual QuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            Transform t = _ikSolversRoot.Find("_IKSolver_" + boneID.ToString());
            if (!t) return null;

            return t.GetComponent<QuickIKSolver>();
		}

        public virtual QuickIKSolver[] GetIKSolvers()
        {
            return _ikSolversRoot.GetComponentsInChildren<QuickIKSolver>();
        }

        public virtual Transform GetIKTarget(HumanBodyBones boneID)
        {
            return _ikTargetsRoot.Find("_IKTarget_" + boneID.ToString());
        }

        public virtual Transform GetIKCalibrationTarget(HumanBodyBones boneID)
        {
            return _ikCalibrationTargetsRoot.Find("_IKCalibrationTarget_" + boneID.ToString());
        }

        public virtual HumanBodyBones? GetIKTargetMidBoneID(HumanBodyBones limbBoneID) {
			if (limbBoneID == HumanBodyBones.Head) return HumanBodyBones.Spine;

			if (limbBoneID == HumanBodyBones.LeftHand) return HumanBodyBones.LeftLowerArm;
			if (limbBoneID == HumanBodyBones.LeftFoot) return HumanBodyBones.LeftLowerLeg;

			if (limbBoneID == HumanBodyBones.RightHand) return HumanBodyBones.RightLowerArm;
			if (limbBoneID == HumanBodyBones.RightFoot) return HumanBodyBones.RightLowerLeg;

			return null;
		}

        protected virtual bool IsBoneLimb(HumanBodyBones boneID)
        {
            List<string> limbBones = QuickUtils.GetEnumValuesToString<IKLimbBones>();
            return limbBones.Contains(boneID.ToString());
        }

        protected virtual Transform GetBoneLimb(HumanBodyBones boneID) {
			return _animator.GetBoneTransform(boneID);
		}

        protected virtual HumanBodyBones GetBoneMidID(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Head) return HumanBodyBones.Hips;

            string midBoneName = HumanTrait.BoneName[HumanTrait.GetParentBone((int)boneLimbID)];
            return QuickUtils.ParseEnum<HumanBodyBones>(midBoneName);
        }

        protected virtual Transform GetBoneMid(HumanBodyBones boneID)
        {
            return _animator.GetBoneTransform(GetBoneMidID(boneID));
		}

        protected virtual HumanBodyBones GetBoneUpperID(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Head) return HumanBodyBones.Hips;

            string upperBoneName = HumanTrait.BoneName[HumanTrait.GetParentBone((int)GetBoneMidID(boneLimbID))];
            return QuickUtils.ParseEnum<HumanBodyBones>(upperBoneName);
        }

        protected virtual Transform GetBoneUpper(HumanBodyBones boneID)
        {
            return _animator.GetBoneTransform(GetBoneUpperID(boneID));
		}

        [ButtonMethod]
        public virtual void ResetIKTargets()
        {
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                QuickIKSolver ikSolver = GetIKSolver(ToUnity(boneID));
                
                ikSolver.ResetIKChain();
                Transform boneLimb = ikSolver._boneLimb;
                Transform boneMid = ikSolver._boneMid;
                Transform ikTargetMid = ikSolver._targetHint;
                Transform ikTargetLimb = ikSolver._targetLimb;
                    
                if (ikTargetLimb && boneLimb)
                {
                    ikTargetLimb.position = boneLimb.position;
                    ikTargetLimb.rotation = boneLimb.rotation;
                }
                if (ikTargetMid && boneMid)
                {
                    ikTargetMid.position = boneMid.position;
                    if (boneID.ToString().Contains("Hand"))
                    {
                        ikTargetMid.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                    }
                    if (boneID.ToString().Contains("Foot"))
                    {
                        ikTargetMid.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                    }
                }
                
            }
        }

        #endregion

		#region UPDATE

        protected virtual void OnAnimatorIK(int layerIndex)
        {
            //Apply the IK for the Spine, using the generic IKSolver
            HumanBodyBones boneLimbID = HumanBodyBones.Head;
            HumanBodyBones boneMidID = GetBoneMidID(boneLimbID);
            HumanBodyBones boneUpperID = GetBoneUpperID(boneLimbID);

            QuickIKSolver ikSolverHead = GetIKSolver(boneLimbID);
            ikSolverHead.UpdateIK();

            _animator.SetBoneLocalRotation(boneLimbID, _animator.GetBoneTransform(boneLimbID).localRotation);
            _animator.SetBoneLocalRotation(boneMidID, _animator.GetBoneTransform(boneMidID).rotation);
            _animator.SetBoneLocalRotation(boneUpperID, _animator.GetBoneTransform(boneUpperID).rotation);
            
            //Apply the IK for the rest of the joints applying Mecanim IK
            List<AvatarIKGoal> ikGoals = QuickUtils.GetEnumValues<AvatarIKGoal>();
            foreach (AvatarIKGoal g in ikGoals)
            {
                UpdateAnimatorIK(g);
            }
        }

        protected virtual void UpdateAnimatorIK(AvatarIKGoal ikGoal)
        {
            HumanBodyBones uBone = QuickUtils.ParseEnum<HumanBodyBones>(ikGoal.ToString());

            _animator.SetIKPosition(ikGoal, GetIKTarget(uBone).position);
            _animator.SetIKPositionWeight(ikGoal, 1.0f);

            _animator.SetIKRotation(ikGoal, GetIKTarget(uBone).rotation);
            _animator.SetIKRotationWeight(ikGoal, 1.0f);
        }

        public override void UpdateTracking()
        {
            
        }

        #endregion

    }

}