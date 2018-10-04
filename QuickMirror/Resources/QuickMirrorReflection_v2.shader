// original source from: http://wiki.unity3d.com/index.php/MirrorReflection4
Shader "QuickVR/MirrorReflection_v2"
{
	Properties
	{
		_LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
		_RightEyeTexture("Right Eye Texture", 2D) = "white" {}
		_ReflectionPower("Reflection Power", Range(0.0, 1.0)) = 1.0
		_NoiseMask("Noise Mask", 2D) = "white" {}
		_NoiseColor("Noise Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_NoisePower("Noise Power", Range(0.0, 1.0)) = 0.0
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "QuickMirrorReflection.cginc"

			fixed4 frag(v2f i) : SV_Target
			{
				return ComputeFinalColor(GetProjUV(i.screenPos), i.uv);
			}
			ENDCG
		}
	}

	Fallback "Diffuse"
}