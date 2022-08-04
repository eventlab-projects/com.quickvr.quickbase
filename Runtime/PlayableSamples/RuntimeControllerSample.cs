using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class RuntimeControllerSample : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip _clip;
    public RuntimeAnimatorController _controller;

    [Range(0.0f, 1.0f)]
    public float _weight = 0.0f;

    #endregion

    #region PROTECTED ATTRIBUTES

    protected AnimationMixerPlayable mixerPlayable;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.
        mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, 2);
        _playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_playableGraph, _clip);
        AnimatorControllerPlayable ctrlPlayable = AnimatorControllerPlayable.Create(_playableGraph, _controller == null? _animator.runtimeAnimatorController : _controller);
        _playableGraph.Connect(clipPlayable, 0, mixerPlayable, 0);
        _playableGraph.Connect(ctrlPlayable, 0, mixerPlayable, 1);

        // Plays the Graph.
        _playableGraph.Play();
    }

    #endregion

    #region UPDATE

    protected virtual void Update()
    {
        _weight = Mathf.Clamp01(_weight);
        mixerPlayable.SetInputWeight(0, 1.0f - _weight);
        mixerPlayable.SetInputWeight(1, _weight);
    }

    #endregion

}