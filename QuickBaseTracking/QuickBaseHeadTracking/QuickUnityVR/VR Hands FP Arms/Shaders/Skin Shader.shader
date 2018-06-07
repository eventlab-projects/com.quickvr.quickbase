Shader "Nature Manufacture/Skin Shader"
{
	Properties
	{
		_RimPower("Rim Power", Range( 0 , 10)) = 3.4
		_RampPower("Ramp Power", Range( 0 , 2)) = 0.9
		_TransluencyColor("Transluency Color", Color) = (1,1,1,1)
		_RimColor("Rim Color", Color) = (0.3568628,0.3568628,0.3568628,0.3568628)
		[HideInInspector] __dirty( "", Int ) = 1
		_SmothnessA("Smothness (A)", 2D) = "black" {}
		_RampRGB("Ramp (RGB)", 2D) = "white" {}
		_AlbedoRGB("Albedo (RGB)", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_TransluencyRGB("Transluency (RGB)", 2D) = "white" {}
		_MetalicRAOG("Metalic (R) AO (G)", 2D) = "white" {}
		_Tint("Tint", Color) = (0.8823529,0.9215686,1,0)
		_AmbientOcclusionPower("Ambient Occlusion Power", Range( 0 , 2)) = 0
		_Smoothness("Smoothness", Range( 0 , 5)) = 0.8
		_NormalStrenght("Normal Strenght", Range( 0 , 5)) = 1
		_Metalic("Metalic", Range( 0 , 5)) = 0
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustom keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_AlbedoRGB;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float3 viewDir;
		};

		struct SurfaceOutputStandardCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 Translucency;
		};

		uniform fixed _NormalStrenght;
		uniform sampler2D _Normal;
		uniform fixed4 _Tint;
		uniform sampler2D _AlbedoRGB;
		uniform sampler2D _RampRGB;
		uniform fixed _RampPower;
		uniform fixed _RimPower;
		uniform fixed4 _RimColor;
		uniform fixed _Metalic;
		uniform sampler2D _MetalicRAOG;
		uniform sampler2D _SmothnessA;
		uniform fixed _Smoothness;
		uniform fixed _AmbientOcclusionPower;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform sampler2D _TransluencyRGB;
		uniform fixed4 _TransluencyColor;

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			UNITY_GI(gi, s, data);
		}

		void surf( Input input , inout SurfaceOutputStandardCustom output )
		{
			output.Normal = UnpackScaleNormal( tex2D( _Normal,input.uv_AlbedoRGB) ,_NormalStrenght );
			fixed2 temp_cast_0 = ( ( dot( WorldNormalVector( input , output.Normal ) , WorldSpaceLightDir( fixed4( input.worldPos, 0) ) ) * 0.5 ) + 0.5 );
			output.Albedo = ( ( _Tint * tex2D( _AlbedoRGB,input.uv_AlbedoRGB) ) + lerp( float4( 0,0,0,0 ) , tex2D( _RampRGB,temp_cast_0) , _RampPower ) ).xyz;
			fixed3 tex2DNode12 = UnpackScaleNormal( tex2D( _Normal,input.uv_AlbedoRGB) ,_NormalStrenght );
			fixed4 temp_cast_2 = pow( ( 1.0 - saturate( dot( tex2DNode12 , normalize( input.viewDir ) ) ) ) , _RimPower );
			output.Emission = ( temp_cast_2 * _RimColor ).rgb;
			fixed4 tex2DNode100 = tex2D( _MetalicRAOG,input.uv_AlbedoRGB);
			output.Metallic = ( _Metalic * tex2DNode100.r );
			output.Smoothness = ( tex2D( _SmothnessA,input.uv_AlbedoRGB).a * _Smoothness );
			output.Occlusion = ( _AmbientOcclusionPower * tex2DNode100.g );
			output.Translucency = ( tex2D( _TransluencyRGB,input.uv_AlbedoRGB) * _TransluencyColor ).rgb;
			output.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	
}
