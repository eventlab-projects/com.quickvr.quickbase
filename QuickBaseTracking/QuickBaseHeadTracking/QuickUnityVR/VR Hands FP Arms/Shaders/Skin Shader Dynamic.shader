Shader "Nature Manufacture/Skin Shader Dynamic"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_RimPower("Rim Power", Range( 0 , 10)) = 0
		_RimColor("Rim Color", Color) = (0,0,0,0)
		_RampPower("Ramp Power", Range( 0 , 2)) = 0
		_TransluencyColor("Transluency Color", Color) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Ramp("Ramp", 2D) = "white" {}
		_Smothness("Smothness", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Transluency("Transluency", 2D) = "white" {}
		_Tint("Tint", Color) = (0,0,0,0)
		_NormalStrenght("Normal Strenght", Range( 0 , 5)) = 1
		_Smoothness("Smoothness", Range( 0 , 5)) = 1
		_Metalic("Metalic", Range( 0 , 5)) = 0
		_DynamicMask("Dynamic Mask", 2D) = "white" {}
		_DynamicMetalic("Dynamic Metalic", Range( 0 , 5)) = 0
		_DynamicTexturePower("Dynamic Texture Power", Range( 0 , 1)) = 0
		_DynamicSmoothness("Dynamic Smoothness", Range( 0 , 5)) = 0
		_DynamicColor("Dynamic Color", Color) = (0,0,0,0)
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
			float2 uv_Albedo;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float3 viewDir;
			float2 uv_Smothness;
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
		uniform sampler2D _Albedo;
		uniform sampler2D _Ramp;
		uniform fixed _RampPower;
		uniform fixed4 _DynamicColor;
		uniform sampler2D _DynamicMask;
		uniform fixed _DynamicTexturePower;
		uniform fixed _RimPower;
		uniform fixed4 _RimColor;
		uniform fixed _Metalic;
		uniform fixed _DynamicMetalic;
		uniform sampler2D _Smothness;
		uniform fixed _Smoothness;
		uniform fixed _DynamicSmoothness;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform sampler2D _Transluency;
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
			output.Normal = UnpackScaleNormal( tex2D( _Normal,input.uv_Albedo) ,_NormalStrenght );
			fixed2 temp_cast_1 = ( ( dot( WorldNormalVector( input , output.Normal ) , WorldSpaceLightDir( fixed4( input.worldPos, 0) ) ) * 0.5 ) + 0.5 );
			fixed4 temp_cast_3 = ( 1.0 / _DynamicTexturePower );
			output.Albedo = lerp( ( ( _Tint * tex2D( _Albedo,input.uv_Albedo) ) + lerp( float4( 0,0,0,0 ) , tex2D( _Ramp,temp_cast_1) , _RampPower ) ) , _DynamicColor , clamp( pow( ( tex2D( _DynamicMask,input.uv_Albedo) * clamp( ( _DynamicTexturePower * 5.0 ) , 0.0 , 1.0 ) ) , temp_cast_3 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) ).x ).rgb;
			fixed3 tex2DNode12 = UnpackScaleNormal( tex2D( _Normal,input.uv_Albedo) ,_NormalStrenght );
			output.Emission = ( pow( ( 1.0 - saturate( dot( tex2DNode12 , normalize( input.viewDir ) ) ) ) , _RimPower ) * _RimColor ).rgb;
			fixed4 temp_cast_7 = ( 1.0 / _DynamicTexturePower );
			output.Metallic = lerp( _Metalic , _DynamicMetalic , clamp( pow( ( tex2D( _DynamicMask,input.uv_Albedo) * clamp( ( _DynamicTexturePower * 5.0 ) , 0.0 , 1.0 ) ) , temp_cast_7 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) ).x );
			fixed4 tex2DNode19 = tex2D( _Smothness,input.uv_Smothness);
			fixed4 temp_cast_9 = ( 1.0 / _DynamicTexturePower );
			output.Smoothness = lerp( ( tex2DNode19.a * _Smoothness ) , ( tex2DNode19.a * _DynamicSmoothness ) , clamp( pow( ( tex2D( _DynamicMask,input.uv_Albedo) * clamp( ( _DynamicTexturePower * 5.0 ) , 0.0 , 1.0 ) ) , temp_cast_9 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) ).x );
			output.Translucency = ( tex2D( _Transluency,input.uv_Albedo) * _TransluencyColor ).rgb;
			output.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
