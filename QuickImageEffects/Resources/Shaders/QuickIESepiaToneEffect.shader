Shader "Hidden/QuickIE/Sepiatone Effect" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_SepiaStrength ("Sepia Strength", Range(0.0, 1.0)) = 0.8
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
				
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float _SepiaStrength;

half4 _MainTex_ST;

fixed4 frag (v2f_img i) : SV_Target
{	
	fixed4 original = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
	
	// get intensity value (Y part of YIQ color space)
	fixed Y = dot (fixed3(0.299, 0.587, 0.114), original.rgb);

	// Convert to Sepia Tone by adding constant
	fixed4 sepiaConvert = float4 (0.191, -0.054, -0.221, 0.0);
	fixed4 output = sepiaConvert + Y;
	output.a = original.a;
	
	return output *_SepiaStrength + original * (1.0 - _SepiaStrength);
}
ENDCG

	}
}

Fallback off

}
