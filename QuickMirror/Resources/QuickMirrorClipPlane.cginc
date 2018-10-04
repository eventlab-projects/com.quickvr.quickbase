#ifndef QUICK_MIRROR_CLIP_PLANE_INC
#define QUICK_MIRROR_CLIP_PLANE_INC

sampler2D _MainTex;

struct Input {
	float2 uv_MainTex;
	float3 worldPos;
};

half _Glossiness;
half _Metallic;
fixed4 _Color;

//Clipping plane definition in WS
float3 MIRROR_PLANE_POS;
float3 MIRROR_PLANE_NORMAL;

void surfBase(Input IN, inout SurfaceOutputStandard o)
{
	// Albedo comes from a texture tinted by color
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	// Metallic and smoothness come from slider variables
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
	o.Alpha = c.a;
}

void surf(Input IN, inout SurfaceOutputStandard o)
{
	float3 v = IN.worldPos - MIRROR_PLANE_POS;
	if (dot(v, MIRROR_PLANE_NORMAL) < 0) discard;

	surfBase(IN, o);
}

#endif