using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.ImageEffects;

namespace QuickVR
{
    public class QuickIESunShafts : SunShafts
    {

        protected virtual void Awake()
        {
            if (!sunShaftsShader) sunShaftsShader = Shader.Find("Hidden/SunShaftsComposite");
            if (!simpleClearShader) simpleClearShader = Shader.Find("Hidden/SimpleClear");
        }
    }

}


