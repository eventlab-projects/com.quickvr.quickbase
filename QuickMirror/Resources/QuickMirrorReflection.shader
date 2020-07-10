Shader "QuickVR/MirrorReflection"
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

		// two texture cards: full thing
			Subshader
		{

			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
			}

			Pass
			{
				CGPROGRAM

					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					#include "QuickMirrorReflection.cginc"

					float4 frag(v2f i) : SV_Target
					{
						return ComputeFinalColor(float2(1.0 - i.uv.x, 1.0 - i.uv.y), i.uv);
					}

				ENDCG
			}
		}

	Fallback "Diffuse"
}