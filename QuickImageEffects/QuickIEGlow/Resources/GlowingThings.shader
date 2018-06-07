// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GlowingThings" {
	Properties {
		_GlowColor("Glow Color", Color) = (1,1,1,1)
		_GlowIntensity("Glow Intensity", Range(0.0, 10.0)) = 1.0
	}
	SubShader {
		Tags 
		{ 
				"RenderType" = "Glowing" 
				"Queue" = "Geometry+1"
		}
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				
				/////////////////////////////////////////////////
				//VERTEX SHADER
				/////////////////////////////////////////////////
				float4 vert (appdata_base v) : SV_POSITION {
				    return UnityObjectToClipPos(v.vertex);
				}
	
				/////////////////////////////////////////////////
				//FRAGMENT SHADER
				/////////////////////////////////////////////////
				float4 frag() : COLOR {
				    discard;
				    return float4(0,0,0,0);
				}
			ENDCG
		}
	}
}
