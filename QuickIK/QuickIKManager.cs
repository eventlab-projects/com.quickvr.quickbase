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

            if (_animator.runtimeAnimatorController)
            {
                _animator.Update(0);
            }

            CreateIKSolversBody();
            CreateIKSolversHand(HumanBodyBones.LeftHand);
            CreateIKSolversHand(HumanBodyBones.RightHand);
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
            _vrManager.AddIKManagerSystem(this);
        }
        
        protected virtual void CreateIKSolversBody()
        {
            CreateConstraints(HumanBodyBones.LeftHand);
            CreateConstraints(HumanBodyBones.RightHand);
        }

        protected virtual void CreateConstraints(HumanBodyBones boneID)
        {
            IQuickIKSolver ikSolver = GetIKSolver(boneID);
            ParentConstraint p = ikSolver._targetHint.GetOrCreateComponent<ParentConstraint>();
            ConstraintSource s = new ConstraintSource();
            s.sourceTransform = ikSolver._boneUpper;
            s.weight = 1;
            p.AddSource(s);
            p.SetTranslationOffset(0, s.sourceTransform.InverseTransformPoint(ikSolver._targetHint.position));
            p.SetRotationOffset(0, (Quaternion.Inverse(s.sourceTransform.rotation) * transform.rotation).eulerAngles);

            float d = Vector3.Distance(ikSolver._boneUpper.position, ikSolver._boneMid.position) + DEFAULT_TARGET_HINT_DISTANCE;
            p.translationAtRest = transform.InverseTransformPoint(ikSolver._boneUpper.position - transform.up * d);

            p.constraintActive = true;
        }

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
                else if (boneName.Contains("LowerArm"))
                {
                    ikTarget.position = ikTarget.position + Vector3.Lerp(-transform.forward, -transform.up, 0.35f).normalized * DEFAULT_TARGET_HINT_DISTANCE;
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

        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikTargetsLeftHand;
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikTargetsRightHand;
            return _ikTargetsRoot;
        }

        #endregion

        #region GET AND SET

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
            return GetIKTargetParent(boneID).Find(GetIKTargetName(boneID));
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
            return GetIKSolver<IQuickIKSolver>(boneID);
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
            List<string> limbBones = QuickUtils.GetEnumValuesToString<IKLimbBones>();
            return boneID.ToString().Contains("Distal") || limbBones.Contains(boneID.ToString());
        }

        public static bool IsBoneMid(HumanBodyBones boneID)
        {
            foreach (HumanBodyBones b in GetIKLimbBones())
            {
                if (boneID == (HumanBodyBones)HumanTrait.GetParentBone((int)b)) return true;
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

        public override void UpdateTrackingEarly()
        {
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

        public override void UpdateTrackingLate()
        {
            ////Update the IK for the fingers
            //Transform leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            //_ikTargetsLeftHand.position = leftHand.position;
            //_ikTargetsLeftHand.rotation = leftHand.rotation;

            //Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            //_ikTargetsRightHand.position = rightHand.position;
            //_ikTargetsRightHand.rotation = rightHand.rotation;

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

            ////Update the position of the hint targets
            //if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.LeftHand)) > 0)
            //{
            //    UpdateHintTargetElbow(HumanBodyBones.LeftHand);
            //}
            //if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.RightHand)) > 0)
            //{
            //    UpdateHintTargetElbow(HumanBodyBones.RightHand);
            //}
            //if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.LeftFoot)) > 0)
            //{
            //    UpdateHintTargetKnee(HumanBodyBones.LeftFoot);
            //}
            //if ((_ikHintMaskUpdate & (1 << (int)IKLimbBones.RightFoot)) > 0)
            //{
            //    UpdateHintTargetKnee(HumanBodyBones.RightFoot);
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

        protected virtual void UpdateHintTargetKnee(HumanBodyBones boneLimbID)
        {
            IQuickIKSolver ikSolver = GetIKSolver(boneLimbID);

            //Compute the projection of the knee on the line passing by the limb bone and the upper bone. 
            float a = Vector3.Magnitude(ikSolver._boneMid.position - ikSolver._boneLimb.position);
            float b = Vector3.Magnitude(ikSolver._boneUpper.position - ikSolver._boneMid.position);
            float c = Vector3.Magnitude(ikSolver._boneUpper.position - ikSolver._boneLimb.position);

            float x = (b*b - (c*c) - (a*a)) / (-2 * c);

            Vector3 u = (ikSolver._boneUpper.position - ikSolver._boneLimb.position).normalized;
            Vector3 proj = ikSolver._boneLimb.position + u * x;

            //Apply the Pitagora's theorem to obtain the height of the triangle
            float h = Mathf.Sqrt(a * a - (x * x));

            Vector3 w = Vector3.Lerp(ikSolver._targetLimb.forward, transform.forward, 0.5f);
            Vector3 n = Vector3.Cross(u, w).normalized;


            //if (boneLimbID == HumanBodyBones.LeftFoot)
            //{
            //    Debug.Log(Vector3.Angle(u, ikSolver._targetLimb.forward).ToString("f3"));
            //    Debug.Log(Vector3.SignedAngle(u, ikSolver._targetLimb.forward, ikSolver._targetLimb.right).ToString("f3"));
            //    Debug.Log(Vector3.Dot(n, ikSolver._targetLimb.right).ToString("f3"));
            //}

            float angle = Vector3.SignedAngle(u, w, ikSolver._targetLimb.right);
            if (angle < 0 || angle > 170)
            {
                n = ikSolver._targetLimb.right;
            }
            Vector3 v = Vector3.Cross(n, u).normalized;

            Vector3 kneePos = proj + v * h;
            ikSolver._targetHint.position = kneePos + w * DEFAULT_TARGET_HINT_DISTANCE;
        }

        protected virtual void OnDrawGizmos()
        {
            Vector3 p1 = Vector3.ProjectOnPlane(_animator.GetBoneTransform(HumanBodyBones.Head).position, transform.up);
            Vector3 p2 = Vector3.ProjectOnPlane(_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position, transform.up);

            DebugExtension.DrawCylinder(transform.position, transform.position + transform.up * 2.0f, Vector3.Magnitude(p1 - p2));
        }

        #endregion

    }

}