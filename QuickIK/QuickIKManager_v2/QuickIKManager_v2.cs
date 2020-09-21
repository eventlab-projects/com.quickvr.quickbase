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
            HumanPose pose = new HumanPose();
            QuickHumanPoseHandler.GetHumanPose(_animator, ref pose);
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
            CreateIKSolver<QuickIKSolverHips>(HumanBodyBones.Hips);
            CreateIKSolver<QuickIKSolverHead>(HumanBodyBones.Head);
            CreateIKSolver<QuickIKSolverHumanoid>(HumanBodyBones.LeftHand);
            CreateIKSolver<QuickIKSolverHumanoid>(HumanBodyBones.RightHand);
            CreateIKSolver<QuickIKSolverHumanoid>(HumanBodyBones.LeftFoot);
            CreateIKSolver<QuickIKSolverHumanoid>(HumanBodyBones.RightFoot);
            CreateIKSolverFinalStep();

            base.CreateIKSolversBody();
        }

        protected virtual QuickIKSolverFinalStep CreateIKSolverFinalStep()
        {
            QuickIKSolverFinalStep ikSolver = CreateIKSolverTransform(_ikSolversBody, "FinalStep").gameObject.GetOrCreateComponent<QuickIKSolverFinalStep>();
            //ikSolver.transform.gameObject.hideFlags = HideFlags.HideAndDontSave;
            ikSolver.transform.SetAsLastSibling();  //Ensure that this is the final transform of the list. 

            return ikSolver;
        }

        //protected override void CreateIKSolversHand(HumanBodyBones boneHandID)
        //{
        //    Transform ikSolversRoot = boneHandID == HumanBodyBones.LeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
        //    Transform tBone = _animator.GetBoneTransform(boneHandID);
            
        //    Transform ikTargetsRoot = boneHandID == HumanBodyBones.LeftHand ? _ikTargetsLeftHand : _ikTargetsRightHand;
        //    ikTargetsRoot.position = tBone.position;
        //    ikTargetsRoot.rotation = tBone.rotation;
            
        //    MultiParentConstraint constraint = ikTargetsRoot.GetOrCreateComponent<MultiParentConstraint>();
        //    constraint.data.constrainedObject = ikTargetsRoot;
        //    WeightedTransformArray sourceObjects = new WeightedTransformArray();
        //    sourceObjects.Add(new WeightedTransform(_animator.GetBoneTransform(boneHandID), 1.0f));
        //    constraint.data.sourceObjects = sourceObjects;

        //    string prefix = boneHandID.ToString().Contains("Left") ? "Left" : "Right";
        //    foreach (IKLimbBonesHand b in GetIKLimbBonesHand())
        //    {
        //        HumanBodyBones boneLimb = QuickUtils.ParseEnum<HumanBodyBones>(prefix + b.ToString() + "Distal");
        //        QuickIKSolverTwoBone ikSolver = CreateIKSolver<QuickIKSolverTwoBone>(boneLimb);
        //        //ikSolver.data.maintainTargetPositionOffset = false;
        //        //ikSolver.data.maintainTargetRotationOffset = false;
        //        ikSolver.data.hint = CreateIKTarget(QuickHumanTrait.GetParentBone(boneLimb));
        //    }
        //}

        #endregion

        #region UPDATE

        public override void UpdateTrackingEarly()
        {
            base.UpdateTrackingEarly();

            for (int i = 0; i < _ikLimbBones.Count; i++)
            {
                IQuickIKSolver ikSolver = GetIKSolver(_ikLimbBones[i]);
                ikSolver._weight = (_ikMaskBody & (1 << i)) != 0 ? 1 : 0;
            }

            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                IQuickIKSolver ikSolver = GetIKSolver(ToUnity(f, true));
                ikSolver._weight = (_ikMaskLeftHand & (1 << (int)f)) != 0 ? 1 : 0;
            }

            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                IQuickIKSolver ikSolver = GetIKSolver(ToUnity(f, false));
                ikSolver._weight = (_ikMaskRightHand & (1 << (int)f)) != 0 ? 1 : 0;
            }
        }

        #endregion

    }

}