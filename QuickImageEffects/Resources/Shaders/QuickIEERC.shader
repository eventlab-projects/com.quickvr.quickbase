Shader "Hidden/QuickIE/ERC" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color (RGB), Strength (A)", Color) = (1,1,1,1)
	}

	SubShader {
		
			Pass {
				ZTest Always Cull Off ZWrite Off
				
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform fixed4 _Color;

				half4 _MainTex_ST;

				fixed4 frag (v2f_img i) : SV_Target
				{	
					fixed4 original = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
					fixed3 c = _Color.rgb * _Color.a;
	
					return fixed4(original.rgb + c, 1);
				}
			ENDCG

		}
	}

	Fallback off

}
