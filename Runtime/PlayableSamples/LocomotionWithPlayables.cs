using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

using QuickVR;

public class LocomotionWithPlayables : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip[] _animationClips;
    public int _testAnimationID = 0;

    #endregion

    #region PROTECTED ATTRIBUTES

    protected AnimationMixerPlayable _mixerPlayable;

    #endregion

    protected virtual void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.
        int numClips = _animationClips.Length;
        _mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, numClips);
        _playableOutput.SetSourcePlayable(_mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        for (int i = 0; i < numClips; i++)
        {
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_playableGraph, _animationClips[i]);
            _playableGraph.Connect(clipPlayable, 0, _mixerPlayable, i);
        }
        
        // Plays the Graph.
        _playableGraph.Play();
    }

    #region UPDATE

    protected virtual void Update()
    {
        for (int i = 0; i < _animationClips.Length; i++)
        {
            _mixerPlayable.SetInputWeight(i, 0);
        }

        _mixerPlayable.SetInputWeight(_testAnimationID, 1);
    }

    [ButtonMethod]
    public virtual void TestUp()
    {
        _testAnimationID ++;
    }

    #endregion

}
