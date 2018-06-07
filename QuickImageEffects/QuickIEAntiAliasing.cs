using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.ImageEffects;

namespace QuickVR
{

    public class QuickIEAntiAliasing : Antialiasing
    {

        protected virtual void Awake()
        {
            if (!ssaaShader) ssaaShader = Shader.Find("Hidden/SSAA");
            if (!dlaaShader) dlaaShader = Shader.Find("Hidden/DLAA");
            if (!nfaaShader) nfaaShader = Shader.Find("Hidden/NFAA");
            if (!shaderFXAAPreset2) shaderFXAAPreset2 = Shader.Find("Hidden/FXAA Preset 2");
            if (!shaderFXAAPreset3) shaderFXAAPreset3 = Shader.Find("Hidden/FXAA Preset 3");
            if (!shaderFXAAII) shaderFXAAII = Shader.Find("Hidden/FXAA II");
            if (!shaderFXAAIII) shaderFXAAIII = Shader.Find("Hidden/FXAA III (Console)");
        }
    }

}
