using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace QuickVR
{

    public class QuickPlaybackHumanoid : MonoBehaviour
    {
        #region PUBLIC ATTRIBUTES

        public bool _loop = false;

        public AnimationClip _animationClip = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator = null;

        protected PlayableGraph _playableGraph;
        protected PlayableOutput _playableOutput;
        protected AnimationClipPlayable _clipPlayable;

        protected QuickRecorderHumanoid _recorder = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnDisable()
        {
            // Destroys all Playables and PlayableOutputs created by the graph.
            if (_playableGraph.IsValid()) _playableGraph.Destroy();
        }

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            _recorder = QuickSingletonManager.GetInstance<QuickRecorderHumanoid>();

            _playableGraph = PlayableGraph.Create();
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);

            // Plays the Graph.
            _playableGraph.Play();
        }

        #endregion

        #region GET AND SET

        public virtual void Play(AnimationClip clip)
        {
            _animationClip = clip;

            // Wrap the clip in a playable
            _clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip);
            _clipPlayable.SetApplyFootIK(false);

            // Connect the Playable to an output
            _playableOutput.SetSourcePlayable(_clipPlayable);
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (_clipPlayable.IsValid() && _loop)
            {
                float currentTime = (float)_clipPlayable.GetTime();
                float clipTime = _clipPlayable.GetAnimationClip().length;
                if (currentTime > clipTime)
                {
                    _clipPlayable.SetTime(Mathf.Repeat(currentTime, clipTime));
                }
            }
        }

        #endregion
    }

}
