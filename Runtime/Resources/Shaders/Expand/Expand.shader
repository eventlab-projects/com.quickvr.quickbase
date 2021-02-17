// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Expand" {

	Properties {
		_MainTex ("", 2D) = "white" {}
	}

	Subshader {
		ZTest Always 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off }
				
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma target 3.0

				#include "UnityCG.cginc"

				/////////////////////////////////////////////////
				// UNIFORM 
				/////////////////////////////////////////////////
				float4 _MainTex_TexelSize;
				sampler2D _MainTex;			//The blurred mask of the glowing objects
				
				/////////////////////////////////////////////////
				// VERTEX SHADER 
				/////////////////////////////////////////////////
				struct v2f {
					float4 pos : POSITION;
					half2 uv : TEXCOORD0;
				};

				v2f vert (appdata_img v) {
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
					return o;
				}
				
				/////////////////////////////////////////////////
				// FRAGMENT SHADER 
				/////////////////////////////////////////////////
				fixed4 frag(v2f i) : COLOR {
					fixed4 result = fixed4(0,0,0,0);
					//This is a black pixel. Check if any of its neighbors is not black, so it has
					//to be expanded
					for (float offX = -1.0; offX < 1.5; offX += 1.0) {
						for (float offY = -1.0; offY < 1.5; offY += 1.0) {
							result = max(result, tex2D(_MainTex, i.uv + float2(offX, offY) * _MainTex_TexelSize.xy));
						}
					}
					return result;
				}
			ENDCG
		}
	}
	Fallback off
}
