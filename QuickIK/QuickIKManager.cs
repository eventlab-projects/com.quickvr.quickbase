using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;

namespace QuickVR {

	[System.Serializable]
    
    public enum IKLimbBones
    {
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

        [BitMask(typeof(IKLimbBones))]
        public int _ikHintMaskUpdate = -1;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector] protected Transform _ikTargetsRoot = null;
        [SerializeField, HideInInspector] protected Transform _ikTargetsLeftHand = null;
        [SerializeField, HideInInspector] protected Transform _ikTargetsRightHand = null;

        [SerializeField, HideInInspector] protected Transform _ikSolversBody = null;
        [SerializeField, HideInInspector] protected Transform _ikSolversLeftHand = null;
        [SerializeField, HideInInspector] protected Transform _ikSolversRightHand = null;

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

            _ikTargetsRoot = transform.CreateChild("__IKTargets__");
            _ikTargetsLeftHand = transform.CreateChild("__IKTargetsLeftHand__");
            _ikTargetsRightHand = transform.CreateChild("__IKTargetsRightHand__");

            _ikSolversBody = transform.CreateChild("__IKSolversBody__");
            _ikSolversLeftHand = transform.CreateChild("__IKSolversLeftHand__");
            _ikSolversRightHand = transform.CreateChild("__IKSolversRightHand__");

            CreateIKSolversBody();
            CreateIKSolversHand(HumanBodyBones.LeftHand);
            CreateIKSolversHand(HumanBodyBones.RightHand);
        }

        protected abstract void CreateIKSolversBody();
        protected virtual void CreateIKSolversHand(HumanBodyBones boneHandID) { }
        
        protected override void Awake()
        {
			base.Awake();

            Reset();

            foreach (QuickIKSolver ikSolver in GetIKSolvers())
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

        public virtual IQuickIKSolver GetIKSolver(string boneName)
        {
            if (QuickUtils.IsEnumValue<HumanBodyBones>(boneName)) return GetIKSolver(QuickUtils.ParseEnum<HumanBodyBones>(boneName));
            return null;
        }

        public virtual IQuickIKSolver GetIKSolver(IKLimbBones boneID)
        {
            return GetIKSolver(ToUnity(boneID));
        }

        public virtual IQuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            Transform t = GetIKSolversRoot(boneID).Find("_IKSolver_" + boneID.ToString());
            if (!t) return null;

            return t.GetComponent<IQuickIKSolver>();
		}

        public virtual List<IQuickIKSolver> GetIKSolvers()
        {
            List<IQuickIKSolver> result = new List<IQuickIKSolver>();
            result.AddRange(GetIKSolversBody());
            result.AddRange(GetIKSolversLeftHand());
            result.AddRange(GetIKSolversRightHand());

            return result;
        }

        public virtual List<IQuickIKSolver> GetIKSolversBody()
        {
            return new List<IQuickIKSolver>(_ikSolversBody.GetComponentsInChildren<IQuickIKSolver>());
        }

        public virtual List<IQuickIKSolver> GetIKSolversLeftHand()
        {
            return new List<IQuickIKSolver>(_ikSolversLeftHand.GetComponentsInChildren<IQuickIKSolver>());
        }

        public virtual List<IQuickIKSolver> GetIKSolversRightHand()
        {
            return new List<IQuickIKSolver>(_ikSolversRightHand.GetComponentsInChildren<IQuickIKSolver>());
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
        public virtual void ResetIKTargets()
        {
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                QuickIKSolver ikSolver = (QuickIKSolver)GetIKSolver(ToUnity(boneID));
                
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

        public override void UpdateTracking() {
            
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
            Vector3 u = (ikSolver._boneMid.position - ikSolver._boneUpper.position).normalized;
            Vector3 v = (ikSolver._boneMid.position - ikSolver._boneLimb.position).normalized;
            if (Vector3.Angle(u, v) < 170.0f)
            {
                Vector3 n = ((u + v) * 0.5f).normalized;
                ikSolver._targetHint.position = ikSolver._boneMid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
                //ikSolver._targetHint.position = ikSolver._boneMid.position + ikSolver._targetLimb.forward * DEFAULT_TARGET_HINT_DISTANCE;
            }
        }

        #endregion

    }

}