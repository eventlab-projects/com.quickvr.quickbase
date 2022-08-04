using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class PlayAnimationSample : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip _clip;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Start()
    {
        //AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _clip, out _playableGraph);
        AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", GetComponent<Animator>());

        // Wrap the clip in a playable
        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_playableGraph, _clip);

        // Connect the Playable to an output
        playableOutput.SetSourcePlayable(clipPlayable);

        // Plays the Graph.
        _playableGraph.Play();
    }

    #endregion

}
