// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//SOURCE:
//http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/

Shader "Hidden/Blur_9" {
	
	Properties {
		_MainTex("", 2D) = "white" {}
		_BlurDirection("Blur Direction", int) = 0
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

				#include "UnityCG.cginc"

				/////////////////////////////////////////////////////
				// UNIFORMS 
				/////////////////////////////////////////////////////
				uniform float4 _MainTex_TexelSize;
				uniform sampler2D _MainTex;
				uniform int _BlurDirection;	//0 => Horizontal; 1 => Vertical
				
				/////////////////////////////////////////////////////
				// VERTEX SHADER 
				/////////////////////////////////////////////////////
				struct v2f {
					float4 pos	: POSITION;
					float2 uv	: TEXCOORD0;
				};
				
				v2f vert (appdata_img v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
					return o;
				}
				
				/////////////////////////////////////////////////////
				// FRAGMENT SHADER 
				/////////////////////////////////////////////////////
				float2 computeOffset(float pOffset) {
					return (_BlurDirection == 0)? float2(pOffset * _MainTex_TexelSize.x, 0) : float2(0, pOffset * _MainTex_TexelSize.y);
				}
				
				fixed4 frag(v2f IN) : COLOR {
					fixed4 c = tex2D(_MainTex, IN.uv) * (70.0 / 256.0);
					
					c += tex2D(_MainTex, IN.uv + computeOffset(1)) * (56.0 / 256.0);
					c += tex2D(_MainTex, IN.uv + computeOffset(2)) * (28.0 / 256.0);
					c += tex2D(_MainTex, IN.uv + computeOffset(3)) * (8.0 / 256.0);
					c += tex2D(_MainTex, IN.uv + computeOffset(4)) * (1.0 / 256.0);
					
					c += tex2D(_MainTex, IN.uv - computeOffset(1)) * (56.0 / 256.0);
					c += tex2D(_MainTex, IN.uv - computeOffset(2)) * (28.0 / 256.0);
					c += tex2D(_MainTex, IN.uv - computeOffset(3)) * (8.0 / 256.0);
					c += tex2D(_MainTex, IN.uv - computeOffset(4)) * (1.0 / 256.0);
					
					return c;
				}
			ENDCG
		}
	}
	Fallback off
}