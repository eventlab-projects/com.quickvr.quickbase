using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.ImageEffects;

namespace QuickVR
{
    class QuickIEGlobalFog : GlobalFog
    {

        protected virtual void Awake()
        {
            if (!fogShader) fogShader = Shader.Find("Hidden/GlobalFog");
        }

    }
}


