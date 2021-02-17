using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickEnableRenderers : MonoBehaviour
{

    public bool _visible = true;

    protected virtual void Start()
    {
        SetVisible(_visible);
    }

    public virtual void SetVisible(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }

        _visible = visible;
    }

}
