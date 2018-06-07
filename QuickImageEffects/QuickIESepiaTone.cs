using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.ImageEffects;

public class QuickIESepiaTone : SepiaTone
{

    #region PUBLIC PARAMETERS

    [Range(0.0f, 1.0f)]
    public float _sepiaStrength = 0.8f;

    #endregion

    #region CREATION AND DESTRUCTION

    protected override void Start()
    {
        if (!shader) shader = Shader.Find("Hidden/QuickIE/Sepiatone Effect");

        base.Start();
    }

    #endregion

    #region UPDATE

    protected virtual void LateUpdate()
    {
        _sepiaStrength = Mathf.Clamp01(_sepiaStrength);
        material.SetFloat("_SepiaStrength", _sepiaStrength);
    }

    #endregion
}
