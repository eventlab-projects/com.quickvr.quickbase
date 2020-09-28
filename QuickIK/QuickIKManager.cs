using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

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

    [ExecuteInEditMode]
    public abstract class QuickIKManager : QuickBaseTrackingManager 
    {

        #region PUBLIC PARAMETERS

        [BitMask(typeof(IKLimbBones))]
        public int _ikMaskBody = -1;

        [BitMask(typeof(QuickHumanFingers))]
        public int _ikMaskLeftHand = -1;

        [BitMask(typeof(QuickHumanFingers))]
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

        protected Dictionary<HumanBodyBones, HumanBodyBones> _hintToLimbBone
        {
            get
            {
                if (m_HintToLimbBone == null)
                {
                    m_HintToLimbBone = new Dictionary<HumanBodyBones, HumanBodyBones>();
                    m_HintToLimbBone[HumanBodyBones.Spine] = HumanBodyBones.Head;
                    m_HintToLimbBone[HumanBodyBones.LeftLowerArm] = HumanBodyBones.LeftHand;
                    m_HintToLimbBone[HumanBodyBones.RightLowerArm] = HumanBodyBones.RightHand;
                    m_HintToLimbBone[HumanBodyBones.LeftLowerLeg] = HumanBodyBones.LeftFoot;
                    m_HintToLimbBone[HumanBodyBones.RightLowerLeg] = HumanBodyBones.RightFoot;

                    m_HintToLimbBone[HumanBodyBones.LeftThumbIntermediate] = HumanBodyBones.LeftThumbDistal;
                    m_HintToLimbBone[HumanBodyBones.LeftIndexIntermediate] = HumanBodyBones.LeftIndexDistal;
                    m_HintToLimbBone[HumanBodyBones.LeftMiddleIntermediate] = HumanBodyBones.LeftMiddleDistal;
                    m_HintToLimbBone[HumanBodyBones.LeftRingIntermediate] = HumanBodyBones.LeftRingDistal;
                    m_HintToLimbBone[HumanBodyBones.LeftLittleIntermediate] = HumanBodyBones.LeftLittleDistal;

                    m_HintToLimbBone[HumanBodyBones.RightThumbIntermediate] = HumanBodyBones.RightThumbDistal;
                    m_HintToLimbBone[HumanBodyBones.RightIndexIntermediate] = HumanBodyBones.RightIndexDistal;
                    m_HintToLimbBone[HumanBodyBones.RightMiddleIntermediate] = HumanBodyBones.RightMiddleDistal;
                    m_HintToLimbBone[HumanBodyBones.RightRingIntermediate] = HumanBodyBones.RightRingDistal;
                    m_HintToLimbBone[HumanBodyBones.RightLittleIntermediate] = HumanBodyBones.RightLittleDistal;
                }

                return m_HintToLimbBone;
            }
        } 

        protected Dictionary<HumanBodyBones, IQuickIKSolver> _ikSolvers = new Dictionary<HumanBodyBones, IQuickIKSolver>();
        protected static List<HumanBodyBones> _ikLimbBones = null;

        protected Dictionary<HumanBodyBones, QuickIKData> _initialIKPose = new Dictionary<HumanBodyBones, QuickIKData>();

        [SerializeField, HideInInspector]
        protected List<Quaternion> _initialLocalRotations = new List<Quaternion>();

        [SerializeField, HideInInspector]
        protected Vector3 _initialHipsLocalPosition = Vector3.zero;

        #endregion

        #region PRIVATE ATTRIBUTES

        [SerializeField, HideInInspector]
        private Transform m_ikTargetsRoot = null;

        [SerializeField, HideInInspector]
        private Transform m_ikTargetsLeftHand = null;

        [SerializeField, HideInInspector]
        private Transform m_ikTargetsRightHand = null;

        [SerializeField, HideInInspector]
        private Transform m_ikSolversBody = null;

        [SerializeField, HideInInspector]
        private Transform m_ikSolversLeftHand = null;

        [SerializeField, HideInInspector]
        private Transform m_ikSolversRightHand = null;

        private Dictionary<HumanBodyBones, HumanBodyBones> m_HintToLimbBone = null;

        #endregion

        #region CONSTANTS

        protected static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
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

        protected virtual void Reset()
        {
            _initialHipsLocalPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
            _initialLocalRotations.Clear();
            foreach (HumanBodyBones b in QuickHumanTrait.GetHumanBodyBones())
            {
                Transform tBone = _animator.GetBoneTransform(b);
                _initialLocalRotations.Add(tBone? tBone.localRotation : Quaternion.identity);
            }

            foreach (HumanBodyBones boneID in GetIKLimbBones())
            {
                CreateBoneHint(boneID);
            }

            RuntimeAnimatorController tmp = _animator.runtimeAnimatorController;
            //_animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("TPose");
            _animator.runtimeAnimatorController = null;
            _animator.Update(0);

            CreateIKSolversBody();
            CreateIKSolversHand(HumanBodyBones.LeftHand);
            CreateIKSolversHand(HumanBodyBones.RightHand);
            
            foreach (HumanBodyBones boneID in GetIKLimbBones())
            {
                CreateConstraintHint(boneID);
            }

            _animator.runtimeAnimatorController = tmp;

        }

        [ButtonMethod]
        public virtual void RemoveComponent()
        {
            _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = _initialHipsLocalPosition;
            foreach (HumanBodyBones b in QuickHumanTrait.GetHumanBodyBones())
            {
                _animator.GetBoneTransform(b).localRotation = _initialLocalRotations[(int)b];
            }

            foreach (HumanBodyBones boneID in GetIKLimbBones())
            {
                QuickUtils.Destroy(GetBoneHint(boneID));
            }

            QuickUtils.Destroy(m_ikTargetsRoot);
            QuickUtils.Destroy(m_ikTargetsLeftHand);
            QuickUtils.Destroy(m_ikTargetsRightHand);

            QuickUtils.Destroy(m_ikSolversBody);
            QuickUtils.Destroy(m_ikSolversLeftHand);
            QuickUtils.Destroy(m_ikSolversRightHand);

            DestroyImmediate(this);
        }

        protected virtual Transform CreateBoneHint(HumanBodyBones boneID)
        {
            Transform tBone = _animator.GetBoneTransform(boneID);
            Transform tRotationHint = tBone.CreateChild("__BoneHint__");

            tRotationHint.position = tBone.position;
            tRotationHint.rotation = transform.rotation;
            if (boneID == HumanBodyBones.LeftHand)
            {
                tRotationHint.LookAt(tBone.position - transform.right, transform.up);
            }
            else if (boneID == HumanBodyBones.RightHand)
            {
                tRotationHint.LookAt(tBone.position + transform.right, transform.up);
            }
            
            return tRotationHint;
        }

        protected override void RegisterTrackingManager()
        {
            if (Application.isPlaying)
            {
                _vrManager.AddIKManagerSystem(this);
            }
        }

        protected abstract void CreateIKSolversBody();

        protected virtual void CreateIKSolversHand(HumanBodyBones boneHandID)
        {
            Transform ikSolversRoot = boneHandID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
            Transform tBone = _animator.GetBoneTransform(boneHandID);

            Transform ikTargetsRoot = boneHandID == HumanBodyBones.LeftHand ? _ikTargetsLeftHand : _ikTargetsRightHand;
            ikTargetsRoot.position = tBone.position;
            ikTargetsRoot.rotation = tBone.rotation;

            bool isLeft = boneHandID.ToString().Contains("Left");
            foreach (QuickHumanFingers b in QuickHumanTrait.GetHumanFingers())
            {
                HumanBodyBones boneLimb = ToUnity(b, isLeft);
                IQuickIKSolver ikSolver = CreateIKSolver<QuickIKSolver>(boneLimb);
                ikSolver._targetHint = CreateIKTarget(QuickHumanTrait.GetParentBone(boneLimb));
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
            ikSolver._weightIKPos = 1.0f;
            ikSolver._weightIKRot = 1.0f;

            HumanBodyBones? midBoneID = GetIKTargetMidBoneID(boneLimb);
            if (midBoneID.HasValue)
            {
                ikSolver._targetHint = CreateIKTarget(midBoneID.Value);
            }
            //ikSolver._boneID = boneLimb;
            
            foreach (IQuickIKSolver s in ikSolver.GetComponents<IQuickIKSolver>())
            {
                s._boneID = boneLimb;
            }

            return ikSolver;
        }

        protected virtual void CreateConstraintHint(HumanBodyBones boneID)
        {
            //IQuickIKSolver ikSolver = GetIKSolver(boneID);
            //if (ikSolver._targetHint)
            //{
            //    ParentConstraint constraint = ikSolver._targetHint.GetComponent<ParentConstraint>();
            //    if (!constraint)
            //    {
            //        constraint = ikSolver._targetHint.gameObject.AddComponent<ParentConstraint>();
            //        ConstraintSource s = new ConstraintSource();
            //        s.sourceTransform = ikSolver._boneUpper;
            //        s.weight = 1;
            //        constraint.AddSource(s);
            //        constraint.SetTranslationOffset(0, s.sourceTransform.InverseTransformPoint(ikSolver._targetHint.position));
            //        constraint.SetRotationOffset(0, (Quaternion.Inverse(s.sourceTransform.rotation) * transform.rotation).eulerAngles);
            //    }

            //    constraint.constraintActive = true;
            //}

            IQuickIKSolver ikSolver = GetIKSolver(boneID);
            if (ikSolver._targetHint)
            {
                ikSolver._targetHint.parent = ikSolver._boneUpper;
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
                if (boneID.Value.ToString().Contains("Spine"))
                {
                    ikTarget.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if 
                    (
                    boneID.Value == HumanBodyBones.LeftLowerArm || boneID.Value == HumanBodyBones.RightLowerArm ||
                    boneID.Value == HumanBodyBones.LeftLowerLeg || boneID.Value == HumanBodyBones.RightLowerLeg
                    )
                {
                    HumanBodyBones boneParent = QuickHumanTrait.GetParentBone(boneID.Value);
                    HumanBodyBones boneChild = QuickHumanTrait.GetChildBones(boneID.Value)[0];
                    Vector3 vUpper = _animator.GetBoneTransform(boneID.Value).position - _animator.GetBoneTransform(boneParent).position;
                    Vector3 vLower = _animator.GetBoneTransform(boneID.Value).position - _animator.GetBoneTransform(boneChild).position;

                    if (Vector3.Angle(vUpper, vLower) > 175.0f)
                    {
                        if (boneID.Value == HumanBodyBones.LeftLowerArm || boneID.Value == HumanBodyBones.RightLowerArm)
                        {
                            ikTarget.position += Vector3.Lerp(-transform.forward, -transform.up, 0.35f).normalized * DEFAULT_TARGET_HINT_DISTANCE;
                        }
                        else
                        {
                            ikTarget.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                        }
                    }
                    else
                    {
                        ikTarget.position += (vUpper.normalized + vLower.normalized).normalized * DEFAULT_TARGET_HINT_DISTANCE;
                    }
                }
                else if (boneName.Contains("LowerArm"))
                {
                    ikTarget.position += Vector3.Lerp(-transform.forward, -transform.up, 0.35f).normalized * DEFAULT_TARGET_HINT_DISTANCE;
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
                    Transform boneHint = GetBoneHint(boneID.Value);
                    if (boneHint)
                    {
                        ikTarget.position = boneHint.position;
                        ikTarget.rotation = boneHint.rotation;
                    }
                    
                }
            }

			return ikTarget;
		}

        #endregion

        #region GET AND SET

        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikTargetsLeftHand;
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikTargetsRightHand;
            return _ikTargetsRoot;
        }

        protected virtual Vector3 GetIKTargetHipsOffset()
        {
            QuickIKSolver ikSolverHead = GetIKSolver<QuickIKSolver>(HumanBodyBones.Head);
            return ikSolverHead._targetLimb.position - ikSolverHead._boneLimb.position;
        }

        protected virtual Transform GetBoneHint(HumanBodyBones boneID)
        {
            return _animator.GetBoneTransform(boneID).Find("__BoneHint__");
        }

        public virtual QuickIKData GetInitialIKData(HumanBodyBones boneID)
        {
            return _initialIKPose.ContainsKey(boneID)? _initialIKPose[boneID] : null;
        }

        public virtual Vector3 GetInitialIKDataLocalPos(HumanBodyBones boneID)
        {
            QuickIKData ikData = GetInitialIKData(boneID);
            if (ikData != null)
            {
                if (IsBoneLimb(boneID))
                {
                    return ikData._targetLimbLocalPosition;
                }
                if (IsBoneMid(boneID))
                {
                    return ikData._targetHintLocalPosition;
                }
            }

            return Vector3.zero;
        }

        public virtual void SetInitialIKDataLocalPosHint(HumanBodyBones boneID, Vector3 localPos)
        {
            QuickIKData ikData = GetInitialIKData(boneID);
            if (ikData != null)
            {
                ikData._targetHintLocalPosition = localPos;
            }
        }

        public virtual Quaternion GetInitialIKDataLocalRot(HumanBodyBones boneID)
        {
            QuickIKData ikData = GetInitialIKData(boneID);
            return ((ikData != null) && (IsBoneLimb(boneID))) ? ikData._targetLimbLocalRotation : Quaternion.identity;
        }

        protected virtual Transform GetBoneUpper(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Spine);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone(boneLimbID)));
        }

        protected virtual Transform GetBoneMid(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Spine);
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
            IQuickIKSolver ikSolver = GetIKSolver(boneID);
            return IsBoneMid(boneID) ? ikSolver._targetHint : ikSolver._targetLimb;
        }

        protected virtual string GetIKTargetName(HumanBodyBones boneID)
        {
            return "_IKTarget_" + boneID.ToString();
        }

        public static List<HumanBodyBones> GetIKLimbBones()
        {
            if (_ikLimbBones == null)
            {
                _ikLimbBones = new List<HumanBodyBones>();
                foreach (IKLimbBones boneID in QuickUtils.GetEnumValues<IKLimbBones>())
                {
                    _ikLimbBones.Add(QuickUtils.ParseEnum<HumanBodyBones>(boneID.ToString()));
                }
            }

            return _ikLimbBones;
        }

        public static HumanBodyBones ToUnity(QuickHumanFingers finger, bool isLeft)
        {
            return (HumanBodyBones)QuickHumanTrait.GetBonesFromFinger(finger, isLeft)[2];
        }

        public virtual T GetIKSolver<T>(HumanBodyBones boneID) where T : IQuickIKSolver
        {
            if (!_ikSolvers.ContainsKey(boneID))
            {
                Transform t = GetIKSolversRoot(boneID).Find("_IKSolver_" + boneID.ToString());
                _ikSolvers[boneID] = t ? t.GetComponent<T>() : default(T);
            }

            return (T)_ikSolvers[boneID];
		}

        public virtual IQuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            HumanBodyBones boneLimbID = boneID;
            if (IsBoneMid(boneID))
            {
                boneLimbID = _hintToLimbBone[boneID];
            }
            
            return GetIKSolver<IQuickIKSolver>(boneLimbID);
        }

        public virtual List<IQuickIKSolver> GetIKSolvers() 
        {
            List<IQuickIKSolver> result = new List<IQuickIKSolver>();
            result.AddRange(GetIKSolversBody());
            result.AddRange(GetIKSolversHand(true));
            result.AddRange(GetIKSolversHand(false));

            return result;
        }

        public virtual List<IQuickIKSolver> GetIKSolversBody()
        {
            return new List<IQuickIKSolver>(_ikSolversBody.GetComponentsInChildren<IQuickIKSolver>());
        }

        public virtual List<IQuickIKSolver> GetIKSolversHand(bool isLeftHand)
        {
            Transform ikSolversRoot = isLeftHand ? _ikSolversLeftHand : _ikSolversRightHand;

            return new List<IQuickIKSolver>(ikSolversRoot.GetComponentsInChildren<IQuickIKSolver>());
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
            return GetIKLimbBones().Contains(boneID) || boneID.ToString().Contains("Distal");
        }

        protected virtual bool IsBoneMid(HumanBodyBones boneID)
        {
            return _hintToLimbBone.ContainsKey(boneID);
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
            List<IQuickIKSolver> srcHandIKSolvers = GetIKSolversHand(srcHandBoneID == HumanBodyBones.LeftHand);
            List<IQuickIKSolver> dstHandIKSolvers = GetIKSolversHand(dstHandBoneID == HumanBodyBones.LeftHand);
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

        protected virtual bool IsTrackedIKLimbBone(IKLimbBones boneID)
        {
            return (_ikMaskBody & (1 << (int)boneID)) != 0;
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (Application.isEditor)
            {
                UpdateTrackingEarly();
                UpdateTrackingLate();
            }
        }

        public override void UpdateTrackingEarly()
        {
            //Update the position of the hint targets
            //List<HumanBodyBones> ikLimbBones = GetIKLimbBones();
            //for (int i = 0; i <  ikLimbBones.Count; i++)
            //{
            //    HumanBodyBones boneID = ikLimbBones[i];
            //    IQuickIKSolver ikSolver = GetIKSolver(boneID);
            //    if (ikSolver._targetHint)
            //    {
            //        ParentConstraint constraint = GetIKSolver(boneID)._targetHint.GetComponent<ParentConstraint>();
            //        if ((_ikHintMaskUpdate & (1 << i)) > 0)
            //        {
            //            constraint.constraintActive = true;
            //            if (boneID == HumanBodyBones.LeftHand || boneID == HumanBodyBones.RightHand)
            //            {
            //                UpdateHintTargetElbow(boneID);
            //            }
            //        }
            //        else
            //        {
            //            constraint.constraintActive = false;
            //        }
            //    }
            //}
        }

        public override void UpdateTrackingLate()
        {
            //Update the IK for the fingers
            Transform leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.position = leftHand.position;
            _ikTargetsLeftHand.rotation = leftHand.rotation;

            Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.position = rightHand.position;
            _ikTargetsRightHand.rotation = rightHand.rotation;

            //foreach (QuickHumanFingers boneID in QuickHumanTrait.GetHumanFingers())
            //{
            //    QuickIKSolver ikSolver = GetIKSolver<QuickIKSolver>(ToUnity(boneID, true));
            //    if ((_ikMaskLeftHand & (1 << (int)boneID)) != 0) ikSolver.UpdateIK();
            //}

            //foreach (QuickHumanFingers boneID in QuickHumanTrait.GetHumanFingers())
            //{
            //    QuickIKSolver ikSolver = GetIKSolver<QuickIKSolver>(ToUnity(boneID, false));
            //    if ((_ikMaskRightHand & (1 << (int)boneID)) != 0) ikSolver.UpdateIK();
            //}
        }

        protected virtual void UpdateHintTargetElbow(HumanBodyBones boneLimbID)
        {
            IQuickIKSolver ikSolver = GetIKSolver(boneLimbID);
            ParentConstraint constraint = ikSolver._targetHint.GetComponent<ParentConstraint>();

            float r = Vector3.Distance(ikSolver._boneUpper.position, ikSolver._boneMid.position) + Vector3.Distance(ikSolver._boneMid.position, ikSolver._boneLimb.position);
            float yUpper = ikSolver._boneUpper.position.y;
            float maxY = yUpper + r;

            float yLimb = ikSolver._targetLimb.position.y;
            constraint.weight = Mathf.Clamp01((maxY - yLimb) / (r));
            constraint.translationAtRest = transform.InverseTransformPoint(ikSolver._boneMid.position - transform.up * DEFAULT_TARGET_HINT_DISTANCE);

            //float upperLength = Vector3.Distance(ikSolver._boneUpper.position, ikSolver._boneMid.position);
            //float sign = boneLimbID == HumanBodyBones.RightHand ? 1 : -1;
            //Vector3 v = (transform.forward - transform.up + sign * transform.right).normalized;
            //Vector3 p = ikSolver._boneUpper.position + v * upperLength;
            //constraint.translationAtRest = transform.InverseTransformPoint(p - transform.up * DEFAULT_TARGET_HINT_DISTANCE);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (IQuickIKSolver ikSolver in GetIKSolvers())
            {
                if (ikSolver._boneUpper && ikSolver._targetLimb) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._targetLimb.position);
            }

            Gizmos.color = Color.magenta;
            foreach (IQuickIKSolver ikSolver in GetIKSolvers())
            {
                if (ikSolver._boneUpper && ikSolver._boneMid) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._boneMid.position);
                if (ikSolver._boneMid && ikSolver._boneLimb) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._boneLimb.position);
            }

            Gizmos.color = Color.yellow;
            foreach (IQuickIKSolver ikSolver in GetIKSolvers())
            {
                if (ikSolver._boneMid && ikSolver._targetHint) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._targetHint.position);
            }
        }

        #endregion

    }

}