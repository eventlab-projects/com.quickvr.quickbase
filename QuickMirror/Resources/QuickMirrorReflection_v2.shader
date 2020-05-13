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
				#pragma vertex vert_v2
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "QuickMirrorReflection.cginc"

				uniform float4x4 _mvpEyeLeft;
				uniform float4x4 _mvpEyeRight;

				v2f vert_v2(float4 pos : POSITION, float2 uv : TEXCOORD0)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(pos);
					o.uv = uv;
					float4x4 mvp = (unity_StereoEyeIndex == 0) ? _mvpEyeLeft : _mvpEyeRight;
					o.screenPos = ComputeScreenPos(mul(mvp, pos)); //ComputeScreenPos(o.pos);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//return ComputeFinalColor(GetProjUV(i.screenPos), i.uv);

					float2 projUV = GetProjUV(i.screenPos);
					//if (REFLECTION_INVERT_Y == 1) {
					//	projUV.y = 1.0 - projUV.y;
					//}
					
					return ComputeFinalColor(projUV, i.uv);
				}
			ENDCG
		}
	}

	Fallback "Diffuse"
}