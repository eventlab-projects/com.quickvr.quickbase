using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;

namespace QuickVR {

	[System.Serializable]
    
    public enum IKLimbBones
    {
        Hips,
        Head,
        LeftHand,
        RightHand,
        LeftFoot,
        RightFoot,
    };

    public enum IKLimbBonesHand
    {
        Thumb, 
        Index, 
        Middle, 
        Ring,
        Little,
    };

	public abstract class QuickIKManager : QuickBaseTrackingManager 
    {

        #region PUBLIC PARAMETERS

        public bool _ikActive = true;

        [BitMask(typeof(IKLimbBones))]
        public int _ikMaskBody = -1;

        [BitMask(typeof(IKLimbBonesHand))]
        public int _ikMaskLeftHand = -1;

        [BitMask(typeof(IKLimbBonesHand))]
        public int _ikMaskRightHand = -1;

        [BitMask(typeof(IKLimbBones))]
        public int _ikHintMaskUpdate = -1;

        #endregion

        #region PROTECTED PARAMETERS

        protected Transform _ikTargetsRoot
        {
            get
            {
                if (!m_ikTargetsRoot) m_ikTargetsRoot = transform.CreateChild("__IKTargets__");
                return m_ikTargetsRoot;
            }
        }

        protected Transform _ikTargetsLeftHand
        {
            get
            {
                if (!m_ikTargetsLeftHand) m_ikTargetsLeftHand = transform.CreateChild("__IKTargetsLeftHand__");
                return m_ikTargetsLeftHand;
            }
        }

        protected Transform _ikTargetsRightHand
        {
            get
            {
                if (!m_ikTargetsRightHand) m_ikTargetsRightHand = transform.CreateChild("__IKTargetsRightHand__");
                return m_ikTargetsRightHand;
            }
        }

        protected Transform _ikSolversBody
        {
            get
            {
                if (!m_ikSolversBody) m_ikSolversBody = transform.CreateChild("__IKSolversBody__");
                return m_ikSolversBody;
            }
        }

        protected Transform _ikSolversLeftHand
        {
            get
            {
                if (!m_ikSolversLeftHand) m_ikSolversLeftHand = transform.CreateChild("__IKSolversLeftHand__");
                return m_ikSolversLeftHand;
            }
        }

        protected Transform _ikSolversRightHand
        {
            get
            {
                if (!m_ikSolversRightHand) m_ikSolversRightHand = transform.CreateChild("__IKSolversRightHand__");
                return m_ikSolversRightHand;
            }
        }

        protected Transform m_ikTargetsRoot = null;
        protected Transform m_ikTargetsLeftHand = null;
        protected Transform m_ikTargetsRightHand = null;

        protected Transform m_ikSolversBody = null;
        protected Transform m_ikSolversLeftHand = null;
        protected Transform m_ikSolversRightHand = null;

        protected static List<IKLimbBones> _ikLimbBones = null;
        protected static List<IKLimbBonesHand> _ikLimbBonesHand = null;

        protected Dictionary<HumanBodyBones, QuickIKData> _initialIKPose = new Dictionary<HumanBodyBones, QuickIKData>();

        #endregion

        #region CONSTANTS

        protected static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            base.Reset();

            CreateIKSolversBody();
            CreateIKSolversHand(HumanBodyBones.LeftHand);
            CreateIKSolversHand(HumanBodyBones.RightHand);
        }

        protected abstract void CreateIKSolversBody();

        protected virtual void CreateIKSolversHand(HumanBodyBones boneHandID)
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

        protected virtual T CreateIKSolver<T>(HumanBodyBones boneLimb) where T : MonoBehaviour, IQuickIKSolver
        {
            T ikSolver = GetIKSolver<T>(boneLimb);
            if (!ikSolver)
            {
                ikSolver = CreateIKSolverTransform(GetIKSolversRoot(boneLimb), boneLimb.ToString()).GetOrCreateComponent<T>();

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
                //ikSolver._boneID = boneLimb;
            }

            foreach (IQuickIKSolver s in ikSolver.GetComponents<IQuickIKSolver>())
            {
                s._boneID = boneLimb;
            }

            return ikSolver;
        }

        protected override void Awake()
        {
			base.Awake();

            Reset();

            foreach (IQuickIKSolver ikSolver in GetIKSolvers())
            {
                QuickIKData ikData = new QuickIKData();
                ikData._targetLimbLocalPosition = ikSolver._targetLimb.localPosition;
                ikData._targetLimbLocalRotation = ikSolver._targetLimb.localRotation;
                if (ikSolver._targetHint) ikData._targetHintLocalPosition = ikSolver._targetHint.localPosition;
                _initialIKPose[ikSolver._boneID] = ikData;
            }
        }

        protected virtual Transform CreateIKTarget(HumanBodyBones? boneID) {
			if (!boneID.HasValue) return null;

            Transform ikTarget = GetIKTarget(boneID.Value);

            //Create the ikTarget if necessary
            if (!ikTarget)
            {
                ikTarget = GetIKTargetParent(boneID.Value).CreateChild(GetIKTargetName(boneID.Value));
                Transform bone = _animator.GetBoneTransform(boneID.Value);
                string boneName = boneID.Value.ToString();

                //Set the position of the IKTarget
                ikTarget.position = bone.position;
                if (boneName.Contains("LowerArm") || boneID.Value.ToString().Contains("Spine"))
                {
                    ikTarget.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneName.Contains("LowerLeg"))
                {
                    ikTarget.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneName.Contains("Intermediate"))
                {
                    ikTarget.position += boneName.Contains("Thumb") ? transform.forward * 0.1f : transform.up * 0.1f;
                }

                //Set the rotation of the IKTarget
                if (boneName.Contains("Distal"))
                {
                    ikTarget.rotation = bone.rotation;
                }
                else
                {
                    ikTarget.rotation = transform.rotation;
                    if (boneID.Value == HumanBodyBones.LeftHand)
                    {
                        ikTarget.LookAt(bone.position - transform.right, transform.up);
                    }
                    else if (boneID.Value == HumanBodyBones.RightHand)
                    {
                        ikTarget.LookAt(bone.position + transform.right, transform.up);
                    }
                }
            }

			return ikTarget;
		}

        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikTargetsLeftHand;
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikTargetsRightHand;
            return _ikTargetsRoot;
        }

        #endregion

        #region GET AND SET

        public virtual QuickIKData GetInitialIKData(HumanBodyBones boneID)
        {
            return _initialIKPose.ContainsKey(boneID)? _initialIKPose[boneID] : null;
        }

        public virtual QuickIKData GetInitialIKData(IKLimbBones boneID)
        {
            return GetInitialIKData(ToUnity(boneID));
        }

        public virtual Vector3 GetInitialIKDataLocalPos(HumanBodyBones boneID)
        {
            QuickIKData ikData = GetInitialIKData(boneID);
            if (ikData == null) return Vector3.zero;

            if (IsBoneLimb(boneID))
            {
                return ikData._targetLimbLocalPosition;
            }
            if (IsBoneMid(boneID))
            {
                return ikData._targetHintLocalPosition;
            }

            return Vector3.zero;
        }

        public virtual Quaternion GetInitialIKDataLocalRot(HumanBodyBones boneID)
        {
            QuickIKData ikData = GetInitialIKData(boneID);
            return ((ikData != null) && (IsBoneLimb(boneID))) ? ikData._targetLimbLocalRotation : Quaternion.identity;
        }

        protected virtual Transform GetBoneUpper(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Hips);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone(boneLimbID)));
        }

        protected virtual Transform GetBoneMid(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Hips);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(boneLimbID));
        }

        public override void Calibrate()
        {
            //QuickIKSolver ikSolver = null;
            foreach (IQuickIKSolver ikSolver in GetIKSolvers())
            {
                ResetIKSolver(ikSolver._boneID);
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

        public virtual void ResetIKSolver(HumanBodyBones boneID)
        {
            IQuickIKSolver ikSolver = GetIKSolver(boneID);
            QuickIKData initialIKData = _initialIKPose[ikSolver._boneID];
            ikSolver._targetLimb.localPosition = initialIKData._targetLimbLocalPosition;
            ikSolver._targetLimb.localRotation = initialIKData._targetLimbLocalRotation;
            if (ikSolver._targetHint) ikSolver._targetHint.localPosition = initialIKData._targetHintLocalPosition;
        }

        protected virtual Transform GetIKSolversRoot(HumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikSolversLeftHand;
            else if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikSolversRightHand;
            return _ikSolversBody;
        }

        protected virtual Transform CreateIKSolverTransform(Transform ikSolversRoot, string name)
        {
            string tName = "_IKSolver_" + name;
            return ikSolversRoot.CreateChild(tName);
        }

        public virtual Transform GetIKTarget(HumanBodyBones boneID)
        {
            return GetIKTargetParent(boneID).Find(GetIKTargetName(boneID));
        }

        protected virtual string GetIKTargetName(HumanBodyBones boneID)
        {
            return "_IKTarget_" + boneID.ToString();
        }

        public static List<IKLimbBones> GetIKLimbBones()
        {
            if (_ikLimbBones == null)
            {
                _ikLimbBones = QuickUtils.GetEnumValues<IKLimbBones>();
            }

            return _ikLimbBones;
        }

        public static List<IKLimbBonesHand> GetIKLimbBonesHand()
        {
            if (_ikLimbBonesHand == null) _ikLimbBonesHand = QuickUtils.GetEnumValues<IKLimbBonesHand>();

            return _ikLimbBonesHand;
        }

        protected override int GetDefaultPriority()
        {
            return QuickBodyTracking.DEFAULT_PRIORITY_TRACKING_BODY - 1;
        }

        public static HumanBodyBones ToUnity(IKLimbBones boneID)
        {
            return QuickUtils.ParseEnum<HumanBodyBones>(boneID.ToString());
        }

        public static HumanBodyBones ToUnity(IKLimbBonesHand boneID, bool isLeft)
        {
            return QuickUtils.ParseEnum<HumanBodyBones>((isLeft ? "Left" : "Right") + boneID.ToString() + "Distal");
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

        public virtual T GetIKSolver<T>(string boneName) where T : IQuickIKSolver
        {
            if (QuickUtils.IsEnumValue<HumanBodyBones>(boneName)) return GetIKSolver<T>(QuickUtils.ParseEnum<HumanBodyBones>(boneName));
            return default(T);
        }

        public virtual IQuickIKSolver GetIKSolver(string boneName)
        {
            return GetIKSolver<IQuickIKSolver>(boneName);
        }

        public virtual T GetIKSolver<T>(IKLimbBones boneID) where T : IQuickIKSolver
        {
            return GetIKSolver<T>(ToUnity(boneID));
        }
        
        public virtual IQuickIKSolver GetIKSolver(IKLimbBones boneID)
        {
            return GetIKSolver<IQuickIKSolver>(boneID);
        }

        public virtual T GetIKSolver<T>(HumanBodyBones boneID) where T : IQuickIKSolver
        {
            Transform t = GetIKSolversRoot(boneID).Find("_IKSolver_" + boneID.ToString());
            if (!t) return default(T);

            return t.GetComponent<T>();
		}

        public virtual IQuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            return GetIKSolver<IQuickIKSolver>(boneID);
        }

        public virtual List<T> GetIKSolvers<T>() where T : IQuickIKSolver
        {
            List<T> result = new List<T>();
            result.AddRange(GetIKSolversBody<T>());
            result.AddRange(GetIKSolversLeftHand<T>());
            result.AddRange(GetIKSolversRightHand<T>());

            return result;
        }

        public virtual List<IQuickIKSolver> GetIKSolvers()
        {
            return GetIKSolvers<IQuickIKSolver>();
        }

        public virtual List<T> GetIKSolversBody<T>() where T : IQuickIKSolver
        {
            return new List<T>(_ikSolversBody.GetComponentsInChildren<T>());
        }

        public virtual List<IQuickIKSolver> GetIKSolversBody()
        {
            return GetIKSolversBody<IQuickIKSolver>();
        }

        public virtual List<T> GetIKSolversHand<T>(HumanBodyBones handBoneID) where T : IQuickIKSolver
        {
            Transform ikSolversRoot = handBoneID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;

            return new List<T>(ikSolversRoot.GetComponentsInChildren<T>());
        }

        public virtual List<IQuickIKSolver> GetIKSolversHand(HumanBodyBones handBoneID)
        {
            return GetIKSolversHand<IQuickIKSolver>(handBoneID);
        }

        public virtual List<T> GetIKSolversLeftHand<T>() where T : IQuickIKSolver
        {
            return GetIKSolversHand<T>(HumanBodyBones.LeftHand);
        }

        public virtual List<IQuickIKSolver> GetIKSolversLeftHand()
        {
            return GetIKSolversLeftHand<IQuickIKSolver>();
        }

        public virtual List<T> GetIKSolversRightHand<T>() where T : IQuickIKSolver
        {
            return GetIKSolversHand<T>(HumanBodyBones.RightHand);
        }

        public virtual List<IQuickIKSolver> GetIKSolversRightHand()
        {
            return GetIKSolversRightHand<IQuickIKSolver>();
        }

        public static HumanBodyBones? GetIKTargetMidBoneID(HumanBodyBones limbBoneID) {
			if (limbBoneID == HumanBodyBones.Head) return HumanBodyBones.Spine;

			if (limbBoneID == HumanBodyBones.LeftHand) return HumanBodyBones.LeftLowerArm;
			if (limbBoneID == HumanBodyBones.LeftFoot) return HumanBodyBones.LeftLowerLeg;

			if (limbBoneID == HumanBodyBones.RightHand) return HumanBodyBones.RightLowerArm;
			if (limbBoneID == HumanBodyBones.RightFoot) return HumanBodyBones.RightLowerLeg;

			return null;
		}

        public static bool IsBoneLimb(HumanBodyBones boneID)
        {
            List<string> limbBones = QuickUtils.GetEnumValuesToString<IKLimbBones>();
            return boneID.ToString().Contains("Distal") || limbBones.Contains(boneID.ToString());
        }

        public static bool IsBoneMid(HumanBodyBones boneID)
        {
            foreach (IKLimbBones b in GetIKLimbBones())
            {
                if (boneID == (HumanBodyBones)HumanTrait.GetParentBone((int)ToUnity(b))) return true;
            }

            return false;
        }

        [ButtonMethod]
        public virtual void CopyLeftHandPoseToRightHand()
        {
            CopyHandPose(HumanBodyBones.LeftHand, HumanBodyBones.RightHand);
        }

        [ButtonMethod]
        public virtual void CopyRightHandPoseToLeftHand()
        {
            CopyHandPose(HumanBodyBones.RightHand, HumanBodyBones.LeftHand);
        }

        protected virtual void CopyHandPose(HumanBodyBones srcHandBoneID, HumanBodyBones dstHandBoneID)
        {
            //Copy the hand pose
            MirrorPose(GetIKSolver(srcHandBoneID), GetIKSolver(dstHandBoneID));
            
            //Copy the fingers pose
            List<IQuickIKSolver> srcHandIKSolvers = GetIKSolversHand(srcHandBoneID);
            List<IQuickIKSolver> dstHandIKSolvers = GetIKSolversHand(dstHandBoneID);
            for (int i = 0; i < srcHandIKSolvers.Count; i++)
            {
                MirrorPose(srcHandIKSolvers[i], dstHandIKSolvers[i]);
            }
        }

        protected virtual void MirrorPose(IQuickIKSolver srcIKSolver, IQuickIKSolver dstIKSolver)
        {
            Vector3 srcPos = srcIKSolver._targetLimb.localPosition;
            Quaternion srcRot = srcIKSolver._targetLimb.localRotation;
            dstIKSolver._targetLimb.localPosition = Vector3.Scale(new Vector3(-1, 1, 1), srcPos);
            dstIKSolver._targetLimb.localRotation = new Quaternion(srcRot.x, -srcRot.y, -srcRot.z, srcRot.w);
            if (dstIKSolver._targetHint && srcIKSolver._targetHint)
            {
                dstIKSolver._targetHint.localPosition = Vector3.Scale(new Vector3(-1, 1, 1), srcIKSolver._targetHint.localPosition);
            }
        }

        #endregion

        #region UPDATE

        public virtual void Update()
        {

        }

        public override void UpdateTracking()
        {

            //Update the IK for the fingers
            Transform leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.position = leftHand.position;
            _ikTargetsLeftHand.rotation = leftHand.rotation;

            Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.position = rightHand.position;
            _ikTargetsRightHand.rotation = rightHand.rotation;

            foreach (IKLimbBonesHand boneID in GetIKLimbBonesHand())
            {
                QuickIKSolver ikSolver = GetIKSolver<QuickIKSolver>(ToUnity(boneID, true));
                if ((_ikMaskLeftHand & (1 << (int)boneID)) != 0) ikSolver.UpdateIK();
            }

            foreach (IKLimbBonesHand boneID in GetIKLimbBonesHand())
            {
                QuickIKSolver ikSolver = GetIKSolver<QuickIKSolver>(ToUnity(boneID, false));
                if ((_ikMaskRightHand & (1 << (int)boneID)) != 0) ikSolver.UpdateIK();
            }

            //Update the position of the hint targets
            if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.LeftHand)) > 0)
            {
                UpdateHintTargetElbow(HumanBodyBones.LeftHand);
            }
            if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.RightHand)) > 0)
            {
                UpdateHintTargetElbow(HumanBodyBones.RightHand);
            }
            if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.LeftFoot)) > 0)
            {
                UpdateHintTargetKnee(HumanBodyBones.LeftFoot);
            }
            if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.RightFoot)) > 0)
            {
                UpdateHintTargetKnee(HumanBodyBones.RightFoot);
            }
        }

        protected virtual void UpdateHintTargetElbow(HumanBodyBones boneLimbID)
        {
            IQuickIKSolver ikSolver = GetIKSolver(boneLimbID);
            Vector3 u = (ikSolver._boneMid.position - ikSolver._boneUpper.position).normalized;
            Vector3 v = (ikSolver._boneMid.position - ikSolver._boneLimb.position).normalized;
            if (Vector3.Angle(u, v) < 170.0f)
            {
                Vector3 n = Vector3.ProjectOnPlane((u + v) * 0.5f, transform.up).normalized;
                Vector3 w = (ikSolver._boneMid.position + n) - ikSolver._boneUpper.position;
                Vector3 t = ikSolver._targetLimb.position - ikSolver._boneUpper.position;
                float d = Vector3.Dot(Vector3.Cross(w, t), transform.up);
                if ((boneLimbID == HumanBodyBones.LeftHand && d < 0) || (boneLimbID == HumanBodyBones.RightHand && d > 0)) n *= -1.0f;

                ikSolver._targetHint.position = ikSolver._boneMid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
            }
        }

        protected virtual void UpdateHintTargetKnee(HumanBodyBones boneLimbID)
        {
            IQuickIKSolver ikSolver = GetIKSolver(boneLimbID);
            Vector3 n = Vector3.ProjectOnPlane(ikSolver._targetLimb.forward, transform.up).normalized;
            ikSolver._targetHint.position = ikSolver._boneMid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
        }

        #endregion

    }

}