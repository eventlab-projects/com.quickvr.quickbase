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
    public class QuickIKManager : QuickBaseTrackingManager 
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
        protected Transform m_ikTargetsRoot;

        protected Transform _ikTargetsLeftHand
        {
            get
            {
                if (!m_ikTargetsLeftHand) m_ikTargetsLeftHand = transform.CreateChild("__IKTargetsLeftHand__");
                return m_ikTargetsLeftHand;
            }
        }
        protected Transform m_ikTargetsLeftHand;

        protected Transform _ikTargetsRightHand
        {
            get
            {
                if (!m_ikTargetsRightHand) m_ikTargetsRightHand = transform.CreateChild("__IKTargetsRightHand__");
                return m_ikTargetsRightHand;
            }
        }
        protected Transform m_ikTargetsRightHand;

        protected Transform _ikSolversBody
        {
            get
            {
                if (!m_ikSolversBody) m_ikSolversBody = transform.CreateChild("__IKSolversBody__");
                return m_ikSolversBody;
            }
        }
        protected Transform m_ikSolversBody;

        protected Transform _ikSolversLeftHand
        {
            get
            {
                if (!m_ikSolversLeftHand) m_ikSolversLeftHand = transform.CreateChild("__IKSolversLeftHand__");
                return m_ikSolversLeftHand;
            }
        }
        protected Transform m_ikSolversLeftHand;

        protected Transform _ikSolversRightHand
        {
            get
            {
                if (!m_ikSolversRightHand) m_ikSolversRightHand = transform.CreateChild("__IKSolversRightHand__");
                return m_ikSolversRightHand;
            }
        }
        protected Transform m_ikSolversRightHand;

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
        protected Dictionary<HumanBodyBones, HumanBodyBones> m_HintToLimbBone = null;

        protected Dictionary<HumanBodyBones, QuickIKSolver> _ikSolvers = new Dictionary<HumanBodyBones, QuickIKSolver>();
        protected static List<HumanBodyBones> _ikLimbBones = null;

        [SerializeField, HideInInspector]
        protected List<Quaternion> _initialLocalRotations = new List<Quaternion>();

        [SerializeField, HideInInspector]
        protected Vector3 _initialHipsLocalPosition = Vector3.zero;

        [SerializeField, HideInInspector]
        protected bool _isInitialized = false;

        #endregion

        #region CONSTANTS

        public static string IK_SOLVER_PREFIX = "_IKSolver_";
        public static string IK_TARGET_PREFIX = "_IKTarget_";
        public static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();

            SaveCurrentPose();
        }

        protected virtual void Reset()
        {
            if (!_isInitialized)
            {
                _initialHipsLocalPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;
                _initialLocalRotations.Clear();
                foreach (HumanBodyBones b in QuickHumanTrait.GetHumanBodyBones())
                {
                    Transform tBone = _animator.GetBoneTransform(b);
                    _initialLocalRotations.Add(tBone ? tBone.localRotation : Quaternion.identity);
                }

                CreateIKSolversBody();
                CreateIKSolversHand(HumanBodyBones.LeftHand);
                CreateIKSolversHand(HumanBodyBones.RightHand);

                ResetIKTargets();

                _isInitialized = true;
            }
        }

        protected virtual void OnDestroy()
        {
            ResetTPose();

            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                QuickUtils.Destroy(ikSolver._targetLimb);
                QuickUtils.Destroy(ikSolver._targetHint);
            }

            QuickUtils.Destroy(m_ikTargetsRoot);
            QuickUtils.Destroy(m_ikTargetsLeftHand);
            QuickUtils.Destroy(m_ikTargetsRightHand);

            QuickUtils.Destroy(m_ikSolversBody);
            QuickUtils.Destroy(m_ikSolversLeftHand);
            QuickUtils.Destroy(m_ikSolversRightHand);
        }

        protected override void RegisterTrackingManager()
        {
            if (Application.isPlaying)
            {
                _vrManager.AddIKManagerSystem(this);
            }
        }

        protected virtual void CreateIKSolversBody()
        {
            CreateIKSolver<QuickIKSolverHips>(HumanBodyBones.Hips);
            CreateIKSolver<QuickIKSolver>(HumanBodyBones.Head);
            CreateIKSolver<QuickIKSolverHand>(HumanBodyBones.LeftHand);
            CreateIKSolver<QuickIKSolverHand>(HumanBodyBones.RightHand);
            CreateIKSolver<QuickIKSolver>(HumanBodyBones.LeftFoot);
            CreateIKSolver<QuickIKSolver>(HumanBodyBones.RightFoot);
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
                QuickIKSolver ikSolver = CreateIKSolver<QuickIKSolver>(boneLimb);
                ikSolver._targetHint = CreateIKTargetHint(boneLimb);
            }
        }

        protected virtual T CreateIKSolver<T>(HumanBodyBones boneLimb) where T : QuickIKSolver
        {
            T ikSolver = GetIKSolversRoot(boneLimb).CreateChild(IK_SOLVER_PREFIX + boneLimb.ToString()).GetOrCreateComponent<T>();
    
            //And configure it according to the bone
            ikSolver._boneUpper = GetBoneUpper(boneLimb);
            ikSolver._boneMid = GetBoneMid(boneLimb);
            ikSolver._boneLimb = _animator.GetBoneTransform(boneLimb);

            ikSolver._targetLimb = CreateIKTargetLimb(boneLimb);
            ikSolver._weightIKPos = 1.0f;
            ikSolver._weightIKRot = 1.0f;

            if (GetIKTargetMidBoneID(boneLimb).HasValue)
            {
                ikSolver._targetHint = CreateIKTargetHint(boneLimb);
            }
            
            return ikSolver;
        }

        protected virtual Transform CreateIKTargetLimb(HumanBodyBones boneID)
        {
            Transform ikTargetLimb = GetIKSolver(boneID)._targetLimb;

            //Create the ikTarget if necessary
            if (!ikTargetLimb)
            {
                ikTargetLimb = GetIKTargetParent(boneID).CreateChild(GetIKTargetName(boneID));
                
                ResetIKTargetLimb(boneID, ikTargetLimb);

                //Create a child that will contain the real rotation of the limb bone
                ikTargetLimb.CreateChild("__BoneRotation__").rotation = _animator.GetBoneTransform(boneID).rotation;
            }

            return ikTargetLimb;
        }

        protected virtual Transform CreateIKTargetHint(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = GetIKSolver(boneID);
            Transform ikTargetHint = ikSolver._targetHint;

            //Create the ikTarget if necessary
            if (!ikTargetHint)
            {
                HumanBodyBones boneMidID = QuickHumanTrait.GetParentBone(boneID);
                ikTargetHint = transform.CreateChild(GetIKTargetName(boneMidID));
                Transform boneMid = _animator.GetBoneTransform(boneMidID);
                string boneName = boneMidID.ToString();

                //Set the position of the IKTarget
                ikTargetHint.position = boneMid.position;
                if (boneName.Contains("Spine"))
                {
                    ikTargetHint.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneMidID == HumanBodyBones.LeftLowerArm || boneMidID == HumanBodyBones.RightLowerArm)
                {
                    ikTargetHint.position += Vector3.Lerp(-transform.forward, -transform.up, 0.35f).normalized * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneMidID == HumanBodyBones.LeftLowerLeg || boneMidID == HumanBodyBones.RightLowerLeg)
                {
                    ikTargetHint.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneName.Contains("Intermediate"))
                {
                    ikTargetHint.position += boneName.Contains("Thumb") ? transform.forward * 0.1f : transform.up * 0.1f;
                }
            }

            return ikTargetHint;
        }

        #endregion

        #region GET AND SET

        public virtual void SaveCurrentPose()
        {
            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                ikSolver.SaveCurrentPose();
            }
        }

        protected virtual void ResetTPose()
        {
            _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = _initialHipsLocalPosition;
            foreach (HumanBodyBones b in QuickHumanTrait.GetHumanBodyBones())
            {
                Transform t = _animator.GetBoneTransform(b);
                if (t)
                {
                    t.localRotation = _initialLocalRotations[(int)b];
                }
            }
        }

        protected virtual void ResetIKTargetLimb(HumanBodyBones boneID)
        {
            ResetIKTargetLimb(boneID, GetIKSolver(boneID)._targetLimb);
        }

        protected virtual void ResetIKTargetLimb(HumanBodyBones boneID, Transform ikTargetLimb)
        {
            Transform bone = _animator.GetBoneTransform(boneID);

            //Set the position of the IKTarget
            ikTargetLimb.position = bone.position;

            //Set the rotation of the IKTarget
            if (boneID.ToString().Contains("Distal"))
            {
                ikTargetLimb.rotation = bone.rotation;
            }
            else
            {
                ikTargetLimb.rotation = transform.rotation;
                if (boneID == HumanBodyBones.LeftHand)
                {
                    ikTargetLimb.LookAt(ikTargetLimb.position - transform.right, transform.up);
                }
                else if (boneID == HumanBodyBones.RightHand)
                {
                    ikTargetLimb.LookAt(ikTargetLimb.position + transform.right, transform.up);
                }
            }
        }

        [ButtonMethod]
        public virtual void ResetIKTargets()
        {
            //Restore the TPose
            _ikTargetsRoot.ResetTransformation();
            
            _ikTargetsLeftHand.parent = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.ResetTransformation();

            _ikTargetsRightHand.parent = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.ResetTransformation();

            ResetTPose();

            //Temporally set the parent of each ikTargetLimb to be the boneLimb. This way, the 
            //target is automatically moved to the bone position when the animation is applied. 
            foreach (QuickIKSolver iKSolver in GetIKSolvers())
            {
                ResetIKTargetLimb(iKSolver._boneID);
                iKSolver._targetLimb.parent = iKSolver._boneLimb;
            }

            //If we have an animatorcontroller defined, the targets are moved at the position of the 
            //initial frame of the current animation in such controller. 
            if (_animator.runtimeAnimatorController)
            {
                _animator.Update(0);
                //Force the mouth to be closed
                Transform tJaw = _animator.GetBoneTransform(HumanBodyBones.Jaw);
                if (tJaw)
                {
                    tJaw.localRotation = _initialLocalRotations[(int)HumanBodyBones.Jaw];
                }
            }

            //Restore the ikTargetLimb real parent. 
            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                ikSolver._targetLimb.parent = GetIKTargetParent(ikSolver._boneID);
            }

            _ikTargetsLeftHand.parent = transform;
            _ikTargetsRightHand.parent = transform;

            SaveCurrentPose();
        }

        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikTargetsLeftHand;
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikTargetsRightHand;
            return _ikTargetsRoot;
        }

        protected virtual Vector3 GetIKTargetHipsOffset()
        {
            return Vector3.zero;
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
            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                ikSolver.Calibrate();
            }
        }

        protected virtual Transform GetIKSolversRoot(HumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikSolversLeftHand;
            else if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikSolversRightHand;
            return _ikSolversBody;
        }

        public virtual Transform GetIKTarget(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = GetIKSolver(boneID);
            if (ikSolver != null)
            {
                return IsBoneMid(boneID) ? ikSolver._targetHint : ikSolver._targetLimb;
            }

            return null;
        }

        protected virtual string GetIKTargetName(HumanBodyBones boneID)
        {
            return IK_TARGET_PREFIX + boneID.ToString();
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

        public virtual T GetIKSolver<T>(HumanBodyBones boneID) where T : QuickIKSolver
        {
            if (!_ikSolvers.ContainsKey(boneID))
            {
                Transform t = GetIKSolversRoot(boneID).Find(IK_SOLVER_PREFIX + boneID);
                _ikSolvers[boneID] = t ? t.GetComponent<T>() : default(T);
            }

            return (T)_ikSolvers[boneID];
		}

        public virtual QuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            HumanBodyBones boneLimbID = boneID;
            if (IsBoneMid(boneID))
            {
                boneLimbID = _hintToLimbBone[boneID];
            }
            
            return GetIKSolver<QuickIKSolver>(boneLimbID);
        }

        public virtual List<QuickIKSolver> GetIKSolvers() 
        {
            List<QuickIKSolver> result = new List<QuickIKSolver>();
            result.AddRange(GetIKSolversBody());
            result.AddRange(GetIKSolversHand(true));
            result.AddRange(GetIKSolversHand(false));

            return result;
        }

        public virtual List<QuickIKSolver> GetIKSolversBody()
        {
            return new List<QuickIKSolver>(_ikSolversBody.GetComponentsInChildren<QuickIKSolver>());
        }

        public virtual List<QuickIKSolver> GetIKSolversHand(bool isLeftHand)
        {
            Transform ikSolversRoot = isLeftHand ? _ikSolversLeftHand : _ikSolversRightHand;

            return new List<QuickIKSolver>(ikSolversRoot.GetComponentsInChildren<QuickIKSolver>());
        }

        public static HumanBodyBones? GetIKTargetMidBoneID(HumanBodyBones limbBoneID) {
			if (limbBoneID == HumanBodyBones.Head) return HumanBodyBones.Spine;

			if (limbBoneID == HumanBodyBones.LeftHand) return HumanBodyBones.LeftLowerArm;
			if (limbBoneID == HumanBodyBones.LeftFoot) return HumanBodyBones.LeftLowerLeg;

			if (limbBoneID == HumanBodyBones.RightHand) return HumanBodyBones.RightLowerArm;
			if (limbBoneID == HumanBodyBones.RightFoot) return HumanBodyBones.RightLowerLeg;

			return null;
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
            List<QuickIKSolver> srcHandIKSolvers = GetIKSolversHand(srcHandBoneID == HumanBodyBones.LeftHand);
            List<QuickIKSolver> dstHandIKSolvers = GetIKSolversHand(dstHandBoneID == HumanBodyBones.LeftHand);
            for (int i = 0; i < srcHandIKSolvers.Count; i++)
            {
                MirrorPose(srcHandIKSolvers[i], dstHandIKSolvers[i]);
            }
        }

        protected virtual void MirrorPose(QuickIKSolver srcIKSolver, QuickIKSolver dstIKSolver)
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
            if (!Application.isPlaying)
            {
                UpdateTrackingEarly();
                UpdateTrackingLate();
            }
        }

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
                    ikSolver.UpdateIK();
                }
            }

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

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                if (ikSolver._boneUpper && ikSolver._targetLimb) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._targetLimb.position);
            }

            Gizmos.color = Color.magenta;
            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                if (ikSolver._boneUpper && ikSolver._boneMid) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._boneMid.position);
                if (ikSolver._boneMid && ikSolver._boneLimb) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._boneLimb.position);
            }

            Gizmos.color = Color.yellow;
            foreach (QuickIKSolver ikSolver in GetIKSolvers())
            {
                if (ikSolver._boneMid && ikSolver._targetHint) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._targetHint.position);
            }
        }

        #endregion

    }

}