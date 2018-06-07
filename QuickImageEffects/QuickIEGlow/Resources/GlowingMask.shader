// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GlowingMask" {

	Properties {
		_GlowColor("Glow Color", Color) = (1,1,1,1)
		_GlowIntensity("Glow Intensity", Range(0.0, 10.0)) = 1.0
	}
	
	//The glowing objects are rendered with its glowing tint
	SubShader {
		Tags { 
			"RenderType" = "Glowing" 
		}
		Pass {
			Lighting Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				
				/////////////////////////////////////////////////
				//UNIFORMS
				/////////////////////////////////////////////////
				uniform float4 _GlowColor;
				uniform float _GlowIntensity;
			
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
				    return float4(_GlowColor.rgb * _GlowIntensity, _GlowColor.a);
				    
				}
			ENDCG
		}
	}
	
	//Opaque Objects are rendered in Black
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Lighting Off
			Fog { Mode Off }
			Color (0,0,0,0)
		}
	} 
}