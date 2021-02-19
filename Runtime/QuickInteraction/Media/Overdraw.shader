Shader "Overdraw" {
	Properties{
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" }
		Blend OneMinusDstColor One
		ZTest Always
		Cull Off
		LOD 200

		CGPROGRAM
#pragma surface surf Lambert

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		o.Albedo.rgb = (0.1);
		o.Emission.b = (0.1);
		o.Alpha = 1;
	}
	ENDCG
	}
		FallBack "Diffuse"
}