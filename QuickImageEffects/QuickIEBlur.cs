using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.ImageEffects;

namespace QuickVR
{

    public class QuickIEBlur : Blur
    {

        // Use this for initialization
        protected virtual void Awake()
        {
            if (!blurShader) blurShader = Shader.Find("Hidden/BlurEffectConeTap");
        }

    }

}


