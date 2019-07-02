using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine.Playables;

using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;

namespace QuickVR {

	[System.Serializable]
    
    public class QuickIKManager_v2 : QuickIKManager 
    {

        #region PUBLIC PARAMETERS

        [BitMask(typeof(IKLimbBonesHand))]
        public int _ikMaskLeftHand = -1;

        [BitMask(typeof(IKLimbBonesHand))]
        public int _ikMaskRightHand = -1;

        #endregion

        #region PROTECTED PARAMETERS

        protected PlayableGraph? _initialPoseGraph = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

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

        protected override void CreateIKSolversBody()
        {
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

        protected override void CreateIKSolversHand(HumanBodyBones boneHandID)
        {
            Transform ikSolversRoot = boneHandID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
            Transform tBone = _animator.GetBoneTransform(boneHandID);
            
            Transform ikTargetsRoot = boneHandID == HumanBodyBones.LeftHand ? _ikTargetsLeftHand : _ikTargetsRightHand;
            ikTargetsRoot.position = tBone.position;
            ikTargetsRoot.rotation = tBone.rotation;
            
            MultiParentConstraint constraint = ikTargetsRoot.GetOrCreateComponent<MultiParentConstraint>();
            constraint.data.constrainedObject = ikTargetsRoot;
            WeightedTransformArray sourceObjects = new WeightedTransformArray();
            sourceObjects.Add(new WeightedTransform(_animator.GetBoneTransform(boneHandID), 1.0f));
            constraint.data.sourceObjects = sourceObjects;

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

        #endregion

        #region GET AND SET

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

            base.UpdateTracking();
        }

        #endregion

    }

}