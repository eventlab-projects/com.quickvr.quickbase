using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace QuickVR
{

    public class QuickLocomotionAnimationBase : PlayableSampleBase
    {

        #region PUBLIC ATTRIBUTES

        [Range(0.0f, 1.0f)]
        public float _weight = 1.0f;

        public float _smoothness = 0.1f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickLocomotionTracker _locomotionTracker = null;
        protected QuickIKManager _ikManager = null;

        protected AnimatorControllerPlayable _animatorPlayable;

        protected Vector3 _lastLocalDisplacement = Vector3.zero;
        protected float _lastSpeed = 0;

        protected Vector3 _ikTargetLeftFootPos = Vector3.zero;
        protected Quaternion _ikTargetLeftFootRot = Quaternion.identity;

        protected Vector3 _ikTargetRightFootPos = Vector3.zero;
        protected Quaternion _ikTargetRightFootRot = Quaternion.identity;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _locomotionTracker = gameObject.GetOrCreateComponent<QuickLocomotionTracker>();
            _ikManager = GetComponent<QuickIKManager>();
        }

        protected virtual void Start()
        {
            // Creates AnimationClipPlayable and connects them to the mixer.
            _animatorPlayable = AnimatorControllerPlayable.Create(_playableGraph, Resources.Load<RuntimeAnimatorController>("QuickLocomotionMaster"));
            _playableOutput.SetSourcePlayable(_animatorPlayable);

            // Plays the Graph.
            _playableGraph.Play();
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            _playableOutput.SetWeight(_weight);

            float speed = _locomotionTracker._speed;
            speed = Mathf.Lerp(_lastSpeed, speed, _smoothness);

            _animatorPlayable.SetBool("ShouldMove", speed > 0.001f);

            /*
            if (speed > 0.01f && speed < _minSpeed)
            {
                speed = _minSpeed;
               _animator.speed = _tracker.getSpeed() / _minSpeed;
            }
            else
            {
               _animator.speed = 1.0f;
            }
            */

            Vector3 localDisplacement = _locomotionTracker._localDisplacement;
            localDisplacement = Vector3.Lerp(_lastLocalDisplacement, localDisplacement, _smoothness);
            localDisplacement = localDisplacement.normalized * speed;

            _animatorPlayable.SetFloat("VelX", localDisplacement.x);
            _animatorPlayable.SetFloat("VelZ", localDisplacement.z);

            _lastLocalDisplacement = localDisplacement;
            _lastSpeed = speed;
        }

        protected virtual void LateUpdate()
        {
            UpdateFeetIKTargets();
            UpdateFeetIKSolvers();
        }

        protected virtual void UpdateFeetIKTargets()
        {
            if (_ikManager)
            {
                UpdateFootIKTarget(true);
                UpdateFootIKTarget(false);
            }
        }

        protected virtual void UpdateFootIKTarget(bool isLeft)
        {
            QuickIKSolver ikSolver = _ikManager.GetIKSolver(isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
            ikSolver._targetLimb.position = isLeft ? _ikTargetLeftFootPos : _ikTargetRightFootPos;
            ikSolver._targetLimb.rotation = isLeft ? _ikTargetLeftFootRot : _ikTargetRightFootRot;
        }

        protected virtual void UpdateFeetIKSolvers()
        {
            if (_ikManager)
            {
                UpdateFootIKSolver(true);
                UpdateFootIKSolver(false);
            }
        }

        protected virtual void OnAnimatorIK()
        {
            if (_ikManager)
            {
                UpdateFootIKTargetData(true, out _ikTargetLeftFootPos, out _ikTargetLeftFootRot);
                UpdateFootIKTargetData(false, out _ikTargetRightFootPos, out _ikTargetRightFootRot);
            }
        }

        protected virtual void UpdateFootIKTargetData(bool isLeft, out Vector3 ikTargetPos, out Quaternion ikTargetRot)
        {
            AvatarIKGoal ikGoal = isLeft ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
            HumanBodyBones boneID = isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;

            QuickIKSolver ikSolverFoot = _ikManager.GetIKSolver(boneID);
            ikSolverFoot.ResetIKChain();

            ikTargetPos = Vector3.Lerp(ikSolverFoot._targetLimb.position, _animator.GetIKPosition(ikGoal), _weight);
            ikTargetRot = Quaternion.Lerp(ikSolverFoot._targetLimb.rotation, _animator.GetIKRotation(ikGoal), _weight);
        }

        protected virtual void UpdateFootIKSolver(bool isLeft)
        {
            _ikManager.GetIKSolver(isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot).UpdateIK();
        }

        #endregion

    }

}


