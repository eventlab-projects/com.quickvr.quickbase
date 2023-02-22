using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickEnableRenderers : MonoBehaviour
{

    #region PUBLIC ATTRIBUTES

    public bool _visible = true;
    public List<Renderer> _renderers = null;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Start()
    {
        if (_renderers == null)
        {
            Reset();
        }

        SetVisible(_visible);
    }

    protected virtual void Reset()
    {
        _renderers = new List<Renderer>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.gameObject.layer != LayerMask.NameToLayer("UI"))  //Avoid hidding the interaction rays. 
            {
                _renderers.Add(r);
            }
        }
    }

    #endregion

    #region GET AND SET

    public virtual void SetVisible(bool visible)
    {
        if (_renderers != null)
        {
            foreach (Renderer r in _renderers)
            {
                r.enabled = visible;
            }
        }

        _visible = visible;
    }

    #endregion

}
