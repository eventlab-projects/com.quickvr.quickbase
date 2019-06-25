using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;
using UnityEngine.Playables;

using UnityEngine.Animations.Rigging;

namespace QuickVR {

	[System.Serializable]
    
    public class QuickIKManager_v2 : QuickBaseTrackingManager 
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

        protected Transform _ikTargetsRoot = null;
        protected Transform _ikTargetsLeftHand = null;
        protected Transform _ikTargetsRightHand = null;

        protected Transform _ikSolversBody = null;
        protected Transform _ikSolversLeftHand = null;
        protected Transform _ikSolversRightHand = null;

        protected static List<IKLimbBones> _ikLimbBones = null;
        protected static List<IKLimbBonesHand> _ikLimbBonesHand = null;

        protected Dictionary<IKLimbBones, QuickIKData> _initialIKPose = new Dictionary<IKLimbBones, QuickIKData>();
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
            _ikTargetsLeftHand = transform.CreateChild("__IKTargetsLeftHand__");
            _ikTargetsRightHand = transform.CreateChild("__IKTargetsRightHand__");

            CreateIKSolversBody();
            CreateIKSolversHands();
        }

        protected override void Awake()
        {
            base.Awake();

            Reset();

            _rigBuilder = gameObject.GetOrCreateComponent<RigBuilder>();
            _rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikSolversBody.gameObject.GetOrCreateComponent<Rig>()));
            _rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikTargetsLeftHand.gameObject.GetOrCreateComponent<Rig>()));
            _rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikSolversLeftHand.gameObject.GetOrCreateComponent<Rig>()));
            _rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikTargetsRightHand.gameObject.GetOrCreateComponent<Rig>()));
            _rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikSolversRightHand.gameObject.GetOrCreateComponent<Rig>()));
            _rigBuilder.Build();
        }

        protected virtual Transform CreateIKSolverTransform(Transform ikSolversRoot, string name)
        {
            string tName = "_IKSolver_" + name;
            return ikSolversRoot.CreateChild(tName);
        }

        protected virtual void CreateIKSolversBody()
        {
            _ikSolversBody = transform.CreateChild("__IKSolversBody__");
            CreateIKSolverHips();
            CreateIKSolverHead();
            CreateIKSolverHumanoid(HumanBodyBones.LeftHand);
            CreateIKSolverHumanoid(HumanBodyBones.RightHand);
            CreateIKSolverHumanoid(HumanBodyBones.LeftFoot);
            CreateIKSolverHumanoid(HumanBodyBones.RightFoot);
            CreateIKSolverFinalStep();
        }

        protected virtual QuickIKSolverHips CreateIKSolverHips()
        {
            QuickIKSolverHips ikSolver = CreateIKSolverTransform(_ikSolversBody, HumanBodyBones.Hips.ToString()).gameObject.GetOrCreateComponent<QuickIKSolverHips>();
            ikSolver.data._ikTargetHips = CreateIKTarget(HumanBodyBones.Hips);

            return ikSolver;
        }

        protected virtual TwoBoneIKConstraint CreateIKSolverHead()
        {
            TwoBoneIKConstraint ikSolver = CreateIKSolverTransform(_ikSolversBody, HumanBodyBones.Head.ToString()).gameObject.GetOrCreateComponent<TwoBoneIKConstraint>();

            ikSolver.data.root = _animator.GetBoneTransform(HumanBodyBones.Hips);
            ikSolver.data.mid = _animator.GetBoneTransform(HumanBodyBones.Spine);
            ikSolver.data.tip = _animator.GetBoneTransform(HumanBodyBones.Head);
            ikSolver.data.target = CreateIKTarget(HumanBodyBones.Head);
            ikSolver.data.maintainTargetPositionOffset = true;
            ikSolver.data.maintainTargetRotationOffset = true;

            return ikSolver;
        }

        protected virtual QuickIKSolverHumanoid CreateIKSolverHumanoid(HumanBodyBones boneID) 
        {
            QuickIKSolverHumanoid ikSolver = CreateIKSolverTransform(_ikSolversBody, boneID.ToString()).gameObject.GetOrCreateComponent<QuickIKSolverHumanoid>();

            ikSolver.data._ikTarget = CreateIKTarget(boneID);
            HumanBodyBones? midBoneID = GetIKTargetMidBoneID(boneID);
            if (midBoneID.HasValue)
            {
                ikSolver.data._ikTargetHint = CreateIKTarget(midBoneID.Value);
            }

            ikSolver.data._avatarIKGoal = (int)QuickUtils.ParseEnum<AvatarIKGoal>(boneID.ToString());

            return ikSolver;
        }

        protected virtual QuickIKSolverFinalStep CreateIKSolverFinalStep()
        {
            QuickIKSolverFinalStep ikSolver = CreateIKSolverTransform(_ikSolversBody, "FinalStep").gameObject.GetOrCreateComponent<QuickIKSolverFinalStep>();
            //ikSolver.transform.gameObject.hideFlags = HideFlags.HideAndDontSave;
            ikSolver.transform.SetAsLastSibling();  //Ensure that this is the final transform of the list. 

            return ikSolver;
        }

        protected virtual void CreateIKSolversHands()
        {
            _ikSolversLeftHand = transform.CreateChild("__IKSolversLeftHand__");
            CreateIKSolversHand(HumanBodyBones.LeftHand);

            _ikSolversRightHand = transform.CreateChild("__IKSolversRightHand__");
            CreateIKSolversHand(HumanBodyBones.RightHand);
        }

        protected virtual void CreateIKSolversHand(HumanBodyBones boneHandID)
        {
            Transform ikSolversRoot = boneHandID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
            Transform tBone = _animator.GetBoneTransform(boneHandID);
            
            Transform ikTargetsRoot = boneHandID == HumanBodyBones.LeftHand ? _ikTargetsLeftHand : _ikTargetsRightHand;
            ikTargetsRoot.position = tBone.position;
            ikTargetsRoot.rotation = tBone.rotation;
            QuickCopyTransformConstraint constraint = ikTargetsRoot.gameObject.GetOrCreateComponent<QuickCopyTransformConstraint>();
            constraint.data._dstTransform = ikTargetsRoot;
            constraint.data._srcTransform = tBone;

            string prefix = boneHandID.ToString().Contains("Left") ? "Left" : "Right";
            foreach (IKLimbBonesHand b in GetIKLimbBonesHand())
            {
                HumanBodyBones boneLimb = QuickUtils.ParseEnum<HumanBodyBones>(prefix + b.ToString() + "Distal");
                TwoBoneIKConstraint ikSolver = CreateIKSolverTransform(ikSolversRoot, boneLimb.ToString()).gameObject.GetOrCreateComponent<TwoBoneIKConstraint>();

                ikSolver.data.root = _animator.GetBoneTransform((HumanBodyBones)QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone((int)boneLimb))); 
                ikSolver.data.mid = _animator.GetBoneTransform((HumanBodyBones)QuickHumanTrait.GetParentBone((int)boneLimb));
                ikSolver.data.tip = _animator.GetBoneTransform(boneLimb);
                ikSolver.data.target = CreateIKTarget(boneLimb);
                ikSolver.data.maintainTargetPositionOffset = true;
                ikSolver.data.maintainTargetRotationOffset = true;
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
            int i = (int)boneID;
            if (i >= (int)HumanBodyBones.LeftThumbProximal && i <= (int)HumanBodyBones.LeftLittleDistal) return _ikTargetsLeftHand;
            if (i >= (int)HumanBodyBones.RightThumbProximal && i <= (int)HumanBodyBones.RightLittleDistal) return _ikTargetsRightHand;
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

        public static List<IKLimbBonesHand> GetIKLimbBonesHand()
        {
            if (_ikLimbBonesHand == null) _ikLimbBonesHand = QuickUtils.GetEnumValues<IKLimbBonesHand>();

            return _ikLimbBonesHand;
        }
        
        public virtual bool IsIKLimbBoneTracked(IKLimbBones ikLimbBone)
        {
            return (_ikMaskBody & (1 << (int)ikLimbBone)) != 0;
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
            return ((_ikMaskBody & (1 << (int)boneID)) != 0);
        }

        protected virtual string GetIKTargetName(HumanBodyBones boneID)
        {
            return "_IKTarget_" + boneID.ToString();
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
            //UpdateIKTargetsHand(HumanBodyBones.LeftHand);
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