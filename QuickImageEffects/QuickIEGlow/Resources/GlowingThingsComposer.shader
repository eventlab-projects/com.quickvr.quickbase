// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GlowingThingsComposer" {

	Properties {
		_MainTex ("", 2D) = "white" {}
	}

	Subshader {
		ZTest Always 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off }
		Blend One One
		//Blend SrcAlpha OneMinusSrcAlpha
		
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "UnityCG.cginc"

				/////////////////////////////////////////////////
				// UNIFORM 
				/////////////////////////////////////////////////
				float4 _MainTex_TexelSize;
				sampler2D _MainTex;				//The blurred mask of the glowing objects
				
				/////////////////////////////////////////////////
				// VERTEX SHADER 
				/////////////////////////////////////////////////
				struct v2f {
					float4 pos : POSITION;
					half2 uv : TEXCOORD0;
				};

				v2f vert (appdata_img v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
					return o;
				}
				
				/////////////////////////////////////////////////
				// FRAGMENT SHADER 
				/////////////////////////////////////////////////
				fixed4 frag(v2f i) : COLOR {
					i.uv = UnityStereoTransformScreenSpaceTex(i.uv);
					fixed4 maskColor = tex2D(_MainTex, i.uv);
					return saturate(fixed4(maskColor.rgb, 1.0));
				}
			ENDCG
		}
	}
	Fallback off
}
