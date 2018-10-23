#ifndef QUICK_MIRROR_CLIP_PLANE_INC
#define QUICK_MIRROR_CLIP_PLANE_INC

#include "QuickMirrorClipPlaneDefinitions.cginc"

sampler2D _MainTex;

struct Input {
	float2 uv_MainTex;
	float3 worldPos;
};

half _Glossiness;
half _Metallic;
fixed4 _Color;

void clipSurf(Input IN, inout SurfaceOutputStandard o)
{
	clipPlaneTest(IN.worldPos);

	// Albedo comes from a texture tinted by color
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	// Metallic and smoothness come from slider variables
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
	o.Alpha = c.a;
}

#endif