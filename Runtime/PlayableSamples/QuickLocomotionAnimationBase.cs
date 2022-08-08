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

        public float _smoothness = 0.1f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected RuntimeAnimatorController _locomotionController = null;
        protected QuickLocomotionTracker _locomotionTracker = null;
        protected QuickIKManager _ikManager = null;

        protected AnimatorControllerPlayable _animatorPlayable;

        protected Vector3 _lastLocalDisplacement = Vector3.zero;
        protected float _lastSpeed = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _locomotionController = Resources.Load<RuntimeAnimatorController>("QuickLocomotionMaster");
            _locomotionTracker = gameObject.GetOrCreateComponent<QuickLocomotionTracker>();
            _ikManager = GetComponent<QuickIKManager>();
        }

        protected virtual void Start()
        {
            // Creates AnimationClipPlayable and connects them to the mixer.
            _animatorPlayable = AnimatorControllerPlayable.Create(_playableGraph, _locomotionController);
            _playableOutput.SetSourcePlayable(_animatorPlayable);

            // Plays the Graph.
            _playableGraph.Play();
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
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
            UpdateFootIKTarget(true);
            UpdateFootIKTarget(false);
        }

        protected virtual void UpdateFootIKTarget(bool isLeft)
        {
            QuickIKSolver ikSolver = _ikManager.GetIKSolver(isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
            ikSolver._targetLimb.position = ikSolver._boneLimb.position;
            ikSolver._targetLimb.GetChild(0).rotation = ikSolver._boneLimb.rotation;
        }

        protected virtual void UpdateFeetIKSolvers()
        {
            UpdateFootIKSolver(true);
            UpdateFootIKSolver(false);
        }

        protected virtual void UpdateFootIKSolver(bool isLeft)
        {
            _ikManager.GetIKSolver(isLeft ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot).UpdateIK();
        }

        #endregion

    }

}


