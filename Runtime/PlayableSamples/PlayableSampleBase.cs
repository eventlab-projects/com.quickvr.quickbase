using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class PlayableSampleBase : MonoBehaviour
{

    #region PROTECTED ATTRIBUTES

    protected Animator _animator = null;

    protected PlayableGraph _playableGraph;
    protected AnimationPlayableOutput _playableOutput;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    protected virtual void OnEnable()
    {
        if (!_playableGraph.IsValid())
        {
            _playableGraph = PlayableGraph.Create();
            _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
        }
    }

    protected virtual void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        if (_playableGraph.IsValid())
        {
            _playableGraph.Destroy();
        }
    }

    #endregion

}
