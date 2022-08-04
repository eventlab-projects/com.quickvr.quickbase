using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

using QuickVR;

[RequireComponent(typeof(Animator))]
public class PauseSubGraphAnimationSample : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip _clip0;
    public AnimationClip _clip1;

    [Range(0.0f, 1.0f)]
    public float _weight = 0.0f;

    #endregion

    #region PROTECTED ATTRIBUTES

    AnimationMixerPlayable mixerPlayable;
    AnimationClipPlayable clipPlayable0;
    AnimationClipPlayable clipPlayable1;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Start()
    {
        // Creates the graph, the mixer and binds them to the Animator.
        mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, 2);
        _playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        clipPlayable0 = AnimationClipPlayable.Create(_playableGraph, _clip0);
        clipPlayable1 = AnimationClipPlayable.Create(_playableGraph, _clip1);
        _playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
        _playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);

        mixerPlayable.SetInputWeight(0, 1.0f);
        mixerPlayable.SetInputWeight(1, 1.0f);

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

    [ButtonMethod]
    public virtual void PlayClip1()
    {
        clipPlayable1.Play();
    }

    [ButtonMethod]
    public virtual void PauseClip1()
    {
        clipPlayable1.Pause();
    }

    #endregion

}