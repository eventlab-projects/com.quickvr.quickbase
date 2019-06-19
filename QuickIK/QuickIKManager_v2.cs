using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;

using UnityEngine.Animations.Rigging;

namespace QuickVR {

	[System.Serializable]
    
    public class QuickIKManager_v2 : QuickBaseTrackingManager 
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

        [SerializeField, HideInInspector]
        protected Transform _deformRig = null;

        protected static List<IKLimbBones> _ikLimbBones = null;

        protected Dictionary<IKLimbBones, QuickIKData> _initialIKPose = new Dictionary<IKLimbBones, QuickIKData>();

        [SerializeField, HideInInspector]
        protected RigBuilder _rigBuilder = null;

        #endregion

        #region CONSTANTS

        private static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            base.Reset();

            _ikTargetsRoot = transform.CreateChild("__IKTargets__");
            
            CreateIKTarget(HumanBodyBones.Hips);
            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                CreateIKTarget(boneID);
                HumanBodyBones? midBoneID = GetIKTargetMidBoneID(boneID);
                if (midBoneID.HasValue)
                {
                    CreateIKTarget(midBoneID.Value);
                }
            }

            _ikSolversRoot = transform.CreateChild("__IKSolvers__");
            Rig ikSolversRig = _ikSolversRoot.gameObject.GetOrCreateComponent<Rig>();

            //Configure the IKSolver for the spine
            TwoBoneIKConstraint ikSolverHead = _ikSolversRoot.gameObject.GetOrCreateComponent<TwoBoneIKConstraint>();
            ikSolverHead.data.root = _animator.GetBoneTransform(HumanBodyBones.Hips);
            ikSolverHead.data.mid = _animator.GetBoneTransform(HumanBodyBones.Spine);
            ikSolverHead.data.tip = _animator.GetBoneTransform(HumanBodyBones.Head);
            ikSolverHead.data.target = GetIKTarget(HumanBodyBones.Head);
            ikSolverHead.data.maintainTargetPositionOffset = true;
            ikSolverHead.data.maintainTargetRotationOffset = true;

            //Configure the IKSolver for the body
            QuickIKSolverHumanoid ikSolverBody = _ikSolversRoot.gameObject.GetOrCreateComponent<QuickIKSolverHumanoid>();
            ikSolverBody.data._ikTargetHips = GetIKTarget(HumanBodyBones.Hips);

            ikSolverBody.data._ikTargetLeftHand = GetIKTarget(HumanBodyBones.LeftHand);
            ikSolverBody.data._ikTargetLeftHandHint = GetIKTarget(HumanBodyBones.LeftLowerArm);

            ikSolverBody.data._ikTargetRightHand = GetIKTarget(HumanBodyBones.RightHand);
            ikSolverBody.data._ikTargetRightHandHint = GetIKTarget(HumanBodyBones.RightLowerArm);

            ikSolverBody.data._ikTargetLeftFoot = GetIKTarget(HumanBodyBones.LeftFoot);
            ikSolverBody.data._ikTargetLeftFootHint = GetIKTarget(HumanBodyBones.LeftLowerLeg);

            ikSolverBody.data._ikTargetRightFoot = GetIKTarget(HumanBodyBones.RightFoot);
            ikSolverBody.data._ikTargetRightFootHint = GetIKTarget(HumanBodyBones.RightLowerLeg);

            if (!_rigBuilder)
            {
                _rigBuilder = gameObject.GetOrCreateComponent<RigBuilder>();
                _rigBuilder.layers.Add(new RigBuilder.RigLayer(ikSolversRig));
            }
        }

        protected virtual Transform CreateIKTarget(IKLimbBones boneID)
        {
            return CreateIKTarget(ToUnity(boneID));
        }

        protected virtual Transform CreateIKTarget(HumanBodyBones boneID) {
			Transform ikTarget = GetIKTarget(boneID);

            //Create the ikTarget if necessary
            if (!ikTarget)
            {
                ikTarget = GetIKTargetParent(boneID).CreateChild(GetIKTargetName(boneID));
                Transform bone = GetBoneLimb(boneID);

                //Set the position of the IKTarget
                ikTarget.position = bone.position;
                if (boneID.ToString().Contains("LowerArm") || boneID.ToString().Contains("Spine"))
                {
                    ikTarget.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                if (boneID.ToString().Contains("LowerLeg"))
                {
                    ikTarget.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }

                //Set the rotation of the IKTarget
                ikTarget.rotation = transform.rotation;
                if (boneID == HumanBodyBones.LeftHand)
                {
                    ikTarget.LookAt(bone.position - transform.right, transform.up);
                }
                else if (boneID == HumanBodyBones.RightHand)
                {
                    ikTarget.LookAt(bone.position + transform.right, transform.up);
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

        public virtual TwoBoneIKConstraint GetIKSolverHead()
        {
            return _ikSolversRoot.GetComponent<TwoBoneIKConstraint>();
        }

        public virtual QuickIKSolverHumanoid GetIKSolverBody()
        {
            return _ikSolversRoot.GetComponent<QuickIKSolverHumanoid>();
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
            //TwoBoneIKConstraint ikSolver = null;
            //foreach (IKLimbBones boneID in GetIKLimbBones())
            //{
            //    TwoBoneIKConstraint ikSolver = GetIKSolver(boneID);
            //    QuickIKData initialIKData = _initialIKPose[boneID];
            //    ikSolver.data.target.localPosition = initialIKData._targetLimbLocalPosition;
            //    ikSolver.data.target.localRotation = initialIKData._targetLimbLocalRotation;
            //    ikSolver.data.hint.localPosition = initialIKData._targetHintLocalPosition;
            //}

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

        protected virtual string GetIKTargetName(HumanBodyBones boneID)
        {
            return "_IKTarget_" + boneID.ToString();
        }

        public virtual List<Transform> GetIKTargets()
        {
            List<Transform> result = new List<Transform>();
            foreach (Transform t in _ikTargetsRoot) result.Add(t);

            return result;
        }

        public virtual Transform GetIKTarget(HumanBodyBones boneID)
        {
            return GetIKTargetParent(boneID).Find(GetIKTargetName(boneID));
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

        #endregion

		#region UPDATE

        public override void UpdateTracking() {
            //foreach (IKLimbBones boneID in GetIKLimbBones())
            //{
            //    TwoBoneIKConstraint ikSolver = GetIKSolver(ToUnity(boneID));
            //    if (ikSolver && ((_ikMask & (1 << (int)boneID)) != 0))
            //    {
            //        ikSolver.UpdateIK();
            //    }
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

        //protected virtual void UpdateHintTargetElbow(HumanBodyBones boneLimbID)
        //{
        //    TwoBoneIKConstraint ikSolver = GetIKSolver(boneLimbID);
        //    Vector3 u = (ikSolver.data.mid.position - ikSolver.data.root.position).normalized;
        //    Vector3 v = (ikSolver.data.mid.position - ikSolver.data.tip.position).normalized;
        //    if (Vector3.Angle(u, v) < 170.0f)
        //    {
        //        Vector3 n = Vector3.ProjectOnPlane((u + v) * 0.5f, transform.up).normalized;
        //        Vector3 w = (ikSolver.data.mid.position + n) - ikSolver.data.root.position;
        //        Vector3 t = ikSolver.data.tip.position - ikSolver.data.root.position;
        //        float d = Vector3.Dot(Vector3.Cross(w, t), transform.up);
        //        if ((boneLimbID == HumanBodyBones.LeftHand && d < 0) || (boneLimbID == HumanBodyBones.RightHand && d > 0)) n *= -1.0f;

        //        ikSolver.data.hint.position = ikSolver.data.mid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
        //    }
        //}

        //protected virtual void UpdateHintTargetKnee(HumanBodyBones boneLimbID)
        //{
        //    TwoBoneIKConstraint ikSolver = GetIKSolver(boneLimbID);
        //    Vector3 u = (ikSolver.data.mid.position - ikSolver.data.root.position).normalized;
        //    Vector3 v = (ikSolver.data.mid.position - ikSolver.data.tip.position).normalized;
        //    if (Vector3.Angle(u, v) < 170.0f)
        //    {
        //        Vector3 n = ((u + v) * 0.5f).normalized;
        //        ikSolver.data.hint.position = ikSolver.data.mid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
        //        //ikSolver._targetHint.position = ikSolver._boneMid.position + ikSolver._targetLimb.forward * DEFAULT_TARGET_HINT_DISTANCE;
        //    }
        //}

        #endregion

    }

}