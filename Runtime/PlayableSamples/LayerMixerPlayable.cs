using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Playables;
using UnityEngine.Animations;

public class LayerMixerPlayable : MonoBehaviour
{
    public AnimationClip clip1;
    public AnimationClip clip2;
    public Transform leftShoulder;

    PlayableGraph m_Graph;
    AnimationLayerMixerPlayable m_Mixer;

    [Range(0.0f, 1.0f)]
    public float mixLevel = 0.5f;

    public AvatarMask mask;

    public void Start()
    {
        Animator animator = GetComponent<Animator>();

        m_Graph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(m_Graph, "LayerMixer", animator);
        
        // Create two clip playables
        var clipPlayable1 = AnimationClipPlayable.Create(m_Graph, clip1);
        var clipPlayable2 = AnimationClipPlayable.Create(m_Graph, clip2);

        // Create mixer playable
        m_Mixer = AnimationLayerMixerPlayable.Create(m_Graph, 2);

        // Create two layers, second is setup to override the first layer and affect only left shoulder and childs
        m_Mixer.ConnectInput(0, clipPlayable1, 0, 1.0f);
        m_Mixer.ConnectInput(1, clipPlayable2, 0, mixLevel);

        m_Mixer.SetLayerMaskFromAvatarMask(1, mask);
        playableOutput.SetSourcePlayable(m_Mixer);

        m_Graph.Play();
    }

    public void Update()
    {
        m_Mixer.SetInputWeight(1, mixLevel);
    }

    public void OnDestroy()
    {
        if (m_Graph.IsValid())
        {
            m_Graph.Destroy();
        }
    }
}