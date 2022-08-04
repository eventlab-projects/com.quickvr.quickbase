using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayQueuePlayable : PlayableBehaviour
{

    #region PROTECTED ATTRIBUTES

    protected int m_CurrentClipIndex = -1;
    protected float m_TimeToNextClip;
    protected Playable mixer;

    #endregion

    #region CREATION AND DESTRUCTION

    public virtual void Initialize(AnimationClip[] clipsToPlay, Playable owner, PlayableGraph graph)
    {
        owner.SetInputCount(1);
        mixer = AnimationMixerPlayable.Create(graph, clipsToPlay.Length);
        graph.Connect(mixer, 0, owner, 0);
        owner.SetInputWeight(0, 1);

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {
            graph.Connect(AnimationClipPlayable.Create(graph, clipsToPlay[clipIndex]), 0, mixer, clipIndex);
            mixer.SetInputWeight(clipIndex, 1.0f);
        }
    }

    #endregion

    #region UPDATE

    override public void PrepareFrame(Playable owner, FrameData info)
    {
        if (mixer.GetInputCount() == 0)
        {
            return;
        }

        // Advance to next clip if necessary
        m_TimeToNextClip -= (float)info.deltaTime;
        if (m_TimeToNextClip <= 0.0f)
        {
            m_CurrentClipIndex++;
            if (m_CurrentClipIndex >= mixer.GetInputCount())
            {
                m_CurrentClipIndex = 0;
            }

            AnimationClipPlayable currentClip = (AnimationClipPlayable)mixer.GetInput(m_CurrentClipIndex);

            // Reset the time so that the next clip starts at the correct position
            currentClip.SetTime(0);
            m_TimeToNextClip = currentClip.GetAnimationClip().length;
        }

        // Adjust the weight of the inputs
        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {

            if (clipIndex == m_CurrentClipIndex)
            {
                mixer.SetInputWeight(clipIndex, 1.0f);
            }
            else
            {
                mixer.SetInputWeight(clipIndex, 0.0f);
            }
        }
    }

    #endregion

}

public class PlayQueueSample : PlayableSampleBase
{

    #region PUBLIC ATTRIBUTES

    public AnimationClip[] _clipsToPlay;

    #endregion

    #region CREATION AND DESTRUCTION

    void Start()
    {
        ScriptPlayable<PlayQueuePlayable> playQueuePlayable = ScriptPlayable<PlayQueuePlayable>.Create(_playableGraph);
        PlayQueuePlayable playQueue = playQueuePlayable.GetBehaviour();
        playQueue.Initialize(_clipsToPlay, playQueuePlayable, _playableGraph);
        _playableOutput.SetSourcePlayable(playQueuePlayable, 0);

        _playableGraph.Play();
    }

    #endregion

}