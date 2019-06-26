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

	public class QuickIKManager : QuickBaseTrackingManager 
    {

        #region PUBLIC PARAMETERS

        public bool _ikActive = true;

        [BitMask(typeof(IKLimbBones))]
        public int _ikMask = -1;

        [BitMask(typeof(IKLimbBones))]
        public int _ikHintMaskUpdate = -1;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected Transform _ikTargetsRoot = null;

        [SerializeField, HideInInspector] 
        protected Transform _ikSolversRoot = null;

        protected static List<IKLimbBones> _ikLimbBones = null;

        [SerializeField, HideInInspector]
        protected Transform _boneRotator = null;

        protected float _boundingRadius = 0.0f;

        protected Dictionary<IKLimbBones, QuickIKData> _initialIKPose = new Dictionary<IKLimbBones, QuickIKData>();

        #endregion

        #region CONSTANTS

        private static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            base.Reset();

            _animator = GetComponent<Animator>();
            _ikTargetsRoot = transform.CreateChild("__IKTargets__");
            _ikSolversRoot = transform.CreateChild("__IKSolvers__");

            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                HumanBodyBones uBone = ToUnity(boneID);
                CreateIKSolver<QuickIKSolver>(uBone);
            }

            _boneRotator = transform.CreateChild("__BoneRotator__");
        }

        protected override void Awake() {
			base.Awake();

            if (!_ikSolversRoot) Reset();

            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                _initialIKPose[boneID] = new QuickIKData(ikSolver._targetLimb.localPosition, ikSolver._targetLimb.localRotation, ikSolver._targetHint.localPosition);
            }

            Transform lShoulder = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform rShoulder = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _boundingRadius = Vector3.Distance(lShoulder.position, rShoulder.position) * 0.5f;
        }

        protected virtual T CreateIKSolver<T>(HumanBodyBones boneID) where T : QuickIKSolver
        {
            string tName = "_IKSolver_" + boneID.ToString();
            Transform t = _ikSolversRoot.CreateChild(tName);
            T ikSolver = t.gameObject.GetOrCreateComponent<T>();

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

        public virtual bool IsIKHintBoneActive(IKLimbBones ikLimbBone)
        {
            return (_ikHintMaskUpdate & (1 << (int)ikLimbBone)) != 0;
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
            //QuickIKSolver ikSolver = null;
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                ResetIKSolver(boneID);
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                QuickIKData initialIKData = _initialIKPose[boneID];
                ikSolver._targetLimb.localPosition = initialIKData._targetLimbLocalPosition;
                ikSolver._targetLimb.localRotation = initialIKData._targetLimbLocalRotation;
                ikSolver._targetHint.localPosition = initialIKData._targetHintLocalPosition;
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

        protected virtual bool IsIKLimbBoneActive(IKLimbBones boneID)
        {
            return ((_ikMask & (1 << (int)boneID)) != 0);
        }

        public virtual void ResetIKSolver(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = GetIKSolver(boneID);
            if ((_ikMask & (1 << (int)boneID)) != 0)
            {
                ikSolver.ResetIKChain();
            }
        }

        public virtual void ResetIKSolver(IKLimbBones boneID)
        {
            ResetIKSolver(ToUnity(boneID));
        }

        //public virtual void ResetIKChains()
        //{
        //    QuickIKSolver ikSolver = null;
        //    for (int i = 0; i < _ikLimbBones.Count; i++)
        //    {
        //        IKLimbBones boneID = _ikLimbBones[i];
        //        ikSolver = GetIKSolver(ToUnity(boneID));
        //        if (ikSolver && ((_ikMask & (1 << i)) != 0))
        //        {
        //            ikSolver.ResetIKChain();
        //        }
        //    }
        //}

        public virtual QuickIKSolver GetIKSolver(IKLimbBones boneID)
        {
            return GetIKSolver(ToUnity(boneID));
        }

        public virtual QuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            HumanBodyBones limbBoneID = boneID;
            if (boneID == HumanBodyBones.LeftLowerArm || boneID == HumanBodyBones.LeftUpperArm)
            {
                limbBoneID = HumanBodyBones.LeftHand;
            }
            else if (boneID == HumanBodyBones.RightLowerArm || boneID == HumanBodyBones.RightUpperArm)
            {
                limbBoneID = HumanBodyBones.RightHand;
            }
            else if (boneID == HumanBodyBones.LeftLowerLeg || boneID == HumanBodyBones.LeftUpperLeg)
            {
                limbBoneID = HumanBodyBones.LeftFoot;
            }
            else if (boneID == HumanBodyBones.RightLowerLeg || boneID == HumanBodyBones.RightUpperLeg)
            {
                limbBoneID = HumanBodyBones.RightFoot;
            }

            Transform t = _ikSolversRoot.Find("_IKSolver_" + limbBoneID.ToString());
            if (!t) return null;

            return t.GetComponent<QuickIKSolver>();
		}

        public static HumanBodyBones? GetIKTargetMidBoneID(IKLimbBones limbBoneID)
        {
            return GetIKTargetMidBoneID(ToUnity(limbBoneID));
        }

        public static HumanBodyBones? GetIKTargetMidBoneID(HumanBodyBones limbBoneID) {
			if (limbBoneID == HumanBodyBones.Head) return HumanBodyBones.Spine;

			if (limbBoneID == HumanBodyBones.LeftHand) return HumanBodyBones.LeftLowerArm;
			if (limbBoneID == HumanBodyBones.LeftFoot) return HumanBodyBones.LeftLowerLeg;

			if (limbBoneID == HumanBodyBones.RightHand) return HumanBodyBones.RightLowerArm;
			if (limbBoneID == HumanBodyBones.RightFoot) return HumanBodyBones.RightLowerLeg;

			return null;
		}

        public static HumanBodyBones? GetIKTargetUpperBoneID(IKLimbBones limbBoneID)
        {
            return GetIKTargetUpperBoneID(ToUnity(limbBoneID));
        }

        public static HumanBodyBones? GetIKTargetUpperBoneID(HumanBodyBones limbBoneID)
        {
            if (limbBoneID == HumanBodyBones.Head) return HumanBodyBones.Spine;

            if (limbBoneID == HumanBodyBones.LeftHand) return HumanBodyBones.LeftUpperArm;
            if (limbBoneID == HumanBodyBones.LeftFoot) return HumanBodyBones.LeftUpperLeg;

            if (limbBoneID == HumanBodyBones.RightHand) return HumanBodyBones.RightUpperArm;
            if (limbBoneID == HumanBodyBones.RightFoot) return HumanBodyBones.RightUpperLeg;

            return null;
        }

        public static bool IsBoneLimb(HumanBodyBones boneID)
        {
            List<string> limbBones = QuickUtils.GetEnumValuesToString<IKLimbBones>();
            return limbBones.Contains(boneID.ToString());
        }

        public static bool IsBoneMid(HumanBodyBones boneID)
        {
            foreach (IKLimbBones b in GetIKLimbBones())
            {
                if (boneID == (HumanBodyBones)HumanTrait.GetParentBone((int)ToUnity(b))) return true;
            }

            return false;
        }

        protected virtual Transform GetBoneLimb(HumanBodyBones boneID) {
			return _animator.GetBoneTransform(boneID);
		}

        protected virtual Transform GetBoneMid(HumanBodyBones boneID)
        {
            return boneID == HumanBodyBones.Head ? _animator.GetBoneTransform(HumanBodyBones.Hips) : GetBoneLimb(boneID).parent;
		}

        protected virtual Transform GetBoneUpper(HumanBodyBones boneID)
        {
            return boneID == HumanBodyBones.Head ? _animator.GetBoneTransform(HumanBodyBones.Hips) : GetBoneMid(boneID).parent;
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

        public override void UpdateTracking() {
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                QuickIKSolver ikSolver = GetIKSolver(ToUnity(boneID));
                if (ikSolver && ((_ikMask & (1 << (int)boneID)) != 0))
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
            QuickIKSolver ikSolver = GetIKSolver(boneLimbID);
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
            QuickIKSolver ikSolver = GetIKSolver(boneLimbID);
            Vector3 u = (ikSolver._boneMid.position - ikSolver._boneUpper.position).normalized;
            Vector3 v = (ikSolver._boneMid.position - ikSolver._boneLimb.position).normalized;
            if (Vector3.Angle(u, v) < 170.0f)
            {
                Vector3 n = ((u + v) * 0.5f).normalized;
                ikSolver._targetHint.position = ikSolver._boneMid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
                //ikSolver._targetHint.position = ikSolver._boneMid.position + ikSolver._targetLimb.forward * DEFAULT_TARGET_HINT_DISTANCE;
            }
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

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            //if (!Application.isPlaying) UpdateTracking();

            //Gizmos.DrawSphere(_testPosition, 0.025f);
            //Debug.Log("_testPosition = " + _testPosition);

            //if (_animator)
            //{
            //    DebugExtension.DrawCylinder(_animator.GetBoneTransform(HumanBodyBones.Hips).position, _animator.GetBoneTransform(HumanBodyBones.Neck).position, Color.red, _boundingRadius);
            //}
        }

        #endregion

    }

}