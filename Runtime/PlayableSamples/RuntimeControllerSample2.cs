using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class RuntimeControllerSample2 : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public RuntimeAnimatorController _controller1;
    public RuntimeAnimatorController _controller2;

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
        AnimatorControllerPlayable ctrlPlayable1 = AnimatorControllerPlayable.Create(_playableGraph, _controller1);
        AnimatorControllerPlayable ctrlPlayable2 = AnimatorControllerPlayable.Create(_playableGraph, _controller2);
        _playableGraph.Connect(ctrlPlayable1, 0, mixerPlayable, 0);
        _playableGraph.Connect(ctrlPlayable2, 0, mixerPlayable, 1);

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