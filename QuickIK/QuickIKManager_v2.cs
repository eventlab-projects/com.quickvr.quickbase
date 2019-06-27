using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.SceneManagement;
using UnityEngine.Playables;

using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;

using UnityEngine.Experimental.Animations;

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

        [SerializeField, HideInInspector] protected Transform _ikTargetsRoot = null;
        [SerializeField, HideInInspector] protected Transform _ikTargetsLeftHand = null;
        [SerializeField, HideInInspector] protected Transform _ikTargetsRightHand = null;

        [SerializeField, HideInInspector] protected Transform _ikSolversBody = null;
        [SerializeField, HideInInspector] protected Transform _ikSolversLeftHand = null;
        [SerializeField, HideInInspector] protected Transform _ikSolversRightHand = null;

        protected static List<IKLimbBones> _ikLimbBones = null;
        protected static List<IKLimbBonesHand> _ikLimbBonesHand = null;

        protected Dictionary<IKLimbBones, QuickIKData> _initialIKPose = new Dictionary<IKLimbBones, QuickIKData>();

        protected PlayableGraph? _initialPoseGraph = null;

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

            CreateGraphInitialPose();
            CreateGraphIK();
        }

        protected virtual void OnDestroy()
        {
            if (_initialPoseGraph != null) _initialPoseGraph.Value.Destroy();
        }

        protected virtual void CreateGraphInitialPose()
        {
            if (_animator.runtimeAnimatorController != null) return;

            _initialPoseGraph = PlayableGraph.Create(name + "__InitialPose__");
            AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(_initialPoseGraph.Value, "Animation", _animator);

            // Wrap the clip in a playable
            AnimationClip animation = new AnimationClip();
            HumanPoseHandler poseHandler = new HumanPoseHandler(_animator.avatar, _animator.transform);
            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);
            for (int i = 0; i < pose.muscles.Length; i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0, pose.muscles[i]);
                animation.SetCurve("", typeof(Animator), muscleName, curve);
                //Debug.Log(muscleName + " = " + _pose.muscles[i].ToString("f3"));
            }

            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_initialPoseGraph.Value, animation);
            clipPlayable.SetApplyFootIK(false);

            // Connect the Playable to an output
            playableOutput.SetSourcePlayable(clipPlayable);

            _initialPoseGraph.Value.Play();
        }

        protected virtual void CreateGraphIK()
        {
            RigBuilder rigBuilder = gameObject.GetOrCreateComponent<RigBuilder>();
            //rigBuilder.layers.Add(new RigBuilder.RigLayer(transform.Find("GameObject").gameObject.GetOrCreateComponent<Rig>()));
            rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikSolversBody.gameObject.GetOrCreateComponent<Rig>()));
            rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikTargetsLeftHand.gameObject.GetOrCreateComponent<Rig>()));
            rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikSolversLeftHand.gameObject.GetOrCreateComponent<Rig>()));
            rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikTargetsRightHand.gameObject.GetOrCreateComponent<Rig>()));
            rigBuilder.layers.Add(new RigBuilder.RigLayer(_ikSolversRightHand.gameObject.GetOrCreateComponent<Rig>()));
            rigBuilder.Build();
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

        protected virtual QuickIKSolverTwoBone CreateIKSolverHead()
        {
            QuickIKSolverTwoBone ikSolver = CreateIKSolverTransform(_ikSolversBody, HumanBodyBones.Head.ToString()).gameObject.GetOrCreateComponent<QuickIKSolverTwoBone>();

            ikSolver._boneUpper = _animator.GetBoneTransform(HumanBodyBones.Hips);
            ikSolver._boneMid = _animator.GetBoneTransform(HumanBodyBones.Head);//_animator.GetBoneTransform(HumanBodyBones.Spine);
            ikSolver._boneLimb = _animator.GetBoneTransform(HumanBodyBones.Head);
            ikSolver._targetLimb = CreateIKTarget(HumanBodyBones.Head);
            //ikSolver._targetHint = CreateIKTarget(HumanBodyBones.Spine);
            ikSolver.data.maintainTargetPositionOffset = true;
            ikSolver.data.maintainTargetRotationOffset = true;

            return ikSolver;
        }

        protected virtual QuickIKSolverHumanoid CreateIKSolverHumanoid(HumanBodyBones boneID) 
        {
            QuickIKSolverHumanoid ikSolver = CreateIKSolverTransform(_ikSolversBody, boneID.ToString()).gameObject.GetOrCreateComponent<QuickIKSolverHumanoid>();

            ikSolver._targetLimb = CreateIKTarget(boneID);
            HumanBodyBones? midBoneID = GetIKTargetMidBoneID(boneID);
            if (midBoneID.HasValue)
            {
                ikSolver._targetHint = CreateIKTarget(midBoneID.Value);
            }

            ikSolver.data._posWeightHint = 1.0f;
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
                QuickIKSolverTwoBone ikSolver = CreateIKSolverTransform(ikSolversRoot, boneLimb.ToString()).gameObject.GetOrCreateComponent<QuickIKSolverTwoBone>();

                ikSolver._boneUpper = _animator.GetBoneTransform((HumanBodyBones)QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone((int)boneLimb))); 
                ikSolver._boneMid = _animator.GetBoneTransform((HumanBodyBones)QuickHumanTrait.GetParentBone((int)boneLimb));
                ikSolver._boneLimb = _animator.GetBoneTransform(boneLimb);
                ikSolver._targetLimb = CreateIKTarget(boneLimb);
                ikSolver.data.maintainTargetPositionOffset = true;
                ikSolver.data.maintainTargetRotationOffset = true;
            }
        }

        protected virtual Transform CreateIKTarget(HumanBodyBones boneID) {
			Transform ikTarget = GetIKTarget(boneID);

            //Create the ikTarget if necessary
            if (!ikTarget)
            {
                ikTarget = GetIKTargetParent(boneID).CreateChild(GetIKTargetName(boneID));
                Transform bone = _animator.GetBoneTransform(boneID);

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
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikTargetsLeftHand;
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikTargetsRightHand;
            return _ikTargetsRoot;
        }

        #endregion

        #region GET AND SET

        public virtual IQuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            Transform ikSolversRoot = null;
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) ikSolversRoot = _ikSolversLeftHand;
            else if (QuickHumanTrait.IsBoneFingerRight(boneID)) ikSolversRoot = _ikSolversRightHand;
            else ikSolversRoot = _ikSolversBody;

            Transform t = ikSolversRoot.Find("_IKSolver_" + boneID.ToString());
            if (!t) return null;

            return t.GetComponent<IQuickIKSolver>();
        }

        public virtual IQuickIKSolver GetIKSolver(IKLimbBones boneID)
        {
            return GetIKSolver(ToUnity(boneID));
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

        public virtual List<IQuickIKSolver> GetIKSolvers()
        {
            List<IQuickIKSolver> result = new List<IQuickIKSolver>();
            result.AddRange(GetIKSolversBody());
            result.AddRange(GetIKSolversLeftHand());
            result.AddRange(GetIKSolversRightHand());

            return result;
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

        public override void Calibrate()
        {
            //QuickIKSolverTwoBone ikSolver = null;
            //foreach (IKLimbBones boneID in GetIKLimbBones())
            //{
            //    QuickIKSolverTwoBone ikSolver = GetIKSolver(boneID);
            //    QuickIKData initialIKData = _initialIKPose[boneID];
            //    ikSolver._targetLimb.localPosition = initialIKData._targetLimbLocalPosition;
            //    ikSolver._targetLimb.localRotation = initialIKData._targetLimbLocalRotation;
            //    ikSolver._targetHint.localPosition = initialIKData._targetHintLocalPosition;
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

        protected virtual string GetIKTargetName(HumanBodyBones boneID)
        {
            return "_IKTarget_" + boneID.ToString();
        }

        public virtual Transform GetIKTarget(HumanBodyBones boneID)
        {
            return GetIKTargetParent(boneID).Find(GetIKTargetName(boneID));
        }

        public static HumanBodyBones? GetIKTargetMidBoneID(HumanBodyBones limbBoneID) {
			if (limbBoneID == HumanBodyBones.Head) return HumanBodyBones.Spine;

			if (limbBoneID == HumanBodyBones.LeftHand) return HumanBodyBones.LeftLowerArm;
			if (limbBoneID == HumanBodyBones.LeftFoot) return HumanBodyBones.LeftLowerLeg;

			if (limbBoneID == HumanBodyBones.RightHand) return HumanBodyBones.RightLowerArm;
			if (limbBoneID == HumanBodyBones.RightFoot) return HumanBodyBones.RightLowerLeg;

			return null;
		}

        #endregion

		#region UPDATE

        public override void UpdateTracking()
        {

            foreach (IKLimbBones boneID in GetIKLimbBones())
            {
                IQuickIKSolver ikSolver = GetIKSolver(boneID);
                ikSolver._weight = (_ikMaskBody & (1 << (int)boneID)) != 0 ? 1 : 0;
            }

            foreach (IKLimbBonesHand boneID in GetIKLimbBonesHand())
            {
                IQuickIKSolver ikSolver = GetIKSolver(QuickUtils.ParseEnum<HumanBodyBones>("Left" + boneID.ToString() + "Distal"));
                ikSolver._weight = (_ikMaskLeftHand & (1 << (int)boneID)) != 0 ? 1 : 0;
            }

            foreach (IKLimbBonesHand boneID in GetIKLimbBonesHand())
            {
                IQuickIKSolver ikSolver = GetIKSolver(QuickUtils.ParseEnum<HumanBodyBones>("Right" + boneID.ToString() + "Distal"));
                ikSolver._weight = (_ikMaskRightHand & (1 << (int)boneID)) != 0 ? 1 : 0;
            }

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
        //    QuickIKSolverTwoBone ikSolver = GetIKSolver(boneLimbID);
        //    Vector3 u = (ikSolver._boneMid.position - ikSolver._boneUpper.position).normalized;
        //    Vector3 v = (ikSolver._boneMid.position - ikSolver._boneLimb.position).normalized;
        //    if (Vector3.Angle(u, v) < 170.0f)
        //    {
        //        Vector3 n = Vector3.ProjectOnPlane((u + v) * 0.5f, transform.up).normalized;
        //        Vector3 w = (ikSolver._boneMid.position + n) - ikSolver._boneUpper.position;
        //        Vector3 t = ikSolver._boneLimb.position - ikSolver._boneUpper.position;
        //        float d = Vector3.Dot(Vector3.Cross(w, t), transform.up);
        //        if ((boneLimbID == HumanBodyBones.LeftHand && d < 0) || (boneLimbID == HumanBodyBones.RightHand && d > 0)) n *= -1.0f;

        //        ikSolver._targetHint.position = ikSolver._boneMid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
        //    }
        //}

        //protected virtual void UpdateHintTargetKnee(HumanBodyBones boneLimbID)
        //{
        //    QuickIKSolverTwoBone ikSolver = GetIKSolver(boneLimbID);
        //    Vector3 u = (ikSolver._boneMid.position - ikSolver._boneUpper.position).normalized;
        //    Vector3 v = (ikSolver._boneMid.position - ikSolver._boneLimb.position).normalized;
        //    if (Vector3.Angle(u, v) < 170.0f)
        //    {
        //        Vector3 n = ((u + v) * 0.5f).normalized;
        //        ikSolver._targetHint.position = ikSolver._boneMid.position + n * DEFAULT_TARGET_HINT_DISTANCE;
        //        //ikSolver._targetHint.position = ikSolver._boneMid.position + ikSolver._targetLimb.forward * DEFAULT_TARGET_HINT_DISTANCE;
        //    }
        //}

        #endregion

    }

}