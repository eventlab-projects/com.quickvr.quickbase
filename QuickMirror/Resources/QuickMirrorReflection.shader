// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "QuickVR/MirrorReflection"
{
	Properties
	{
		_LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
		_RightEyeTexture("Right Eye Texture", 2D) = "white" {}
		_ReflectionPower("Reflection Power", Range(0.0, 1.0)) = 1.0
		_NoiseMask("Noise Mask", 2D) = "white" {}
		_NoiseColor("Noise Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_NoisePower("Noise Power", Range(0.0, 1.0)) = 0.0
	}

	// two texture cards: full thing
	Subshader
	{

		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Transparent+1"		//The mirror will be rendered AFTER all transparent objects has been rendered
		}

		Pass
		{
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				/////////////////////////////////////////////////////////
				// UNIFORM PARAMETERS 
				/////////////////////////////////////////////////////////
				uniform sampler2D _LeftEyeTexture;	//The texture containing the reflection for the left eye
				uniform sampler2D _RightEyeTexture;	//The texture containing the reflection for the right eye
				uniform sampler2D _NoiseMask;		//A texture used to create imperfections in the reflection
				uniform float _ReflectionPower;		//Indicates how much light is the reflection. It is used to simulate the light lost during the reflection
				uniform float _NoisePower;			//Indicates how much powerful is the noise texture
				uniform float4 _NoiseColor;			//The color of the noise

													//Structure that defines the vertex shader input			
				struct vertexInput
				{
					float4 vertex	: POSITION;		//Vertex Position in Object Space
					float2 uv 		: TEXCOORD0;	//Vertex Texture Coordinates
				};

				//Structure that defines the fragment shader input
				struct fragmentInput
				{
					float4 position 	: SV_POSITION;	//Position in Screen Space
					float2 uv 			: TEXCOORD0;	//"Standard" texture coordinates
				};

				/////////////////////////////////////////////////////////
				// VERTEX SHADER //
				/////////////////////////////////////////////////////////
				fragmentInput vert(vertexInput i)
				{
					fragmentInput o;
					o.position = UnityObjectToClipPos(i.vertex);
					o.uv = i.uv;

					return o;
				}

				/////////////////////////////////////////////////////////
				// FRAGMENT SHADER //
				/////////////////////////////////////////////////////////
				float4 frag(fragmentInput i) : SV_Target
				{
					float2 uvReflection = float2(1.0 - i.uv.x, 1.0 - i.uv.y);
					float4 texColor = (unity_StereoEyeIndex == 0) ? tex2D(_LeftEyeTexture, uvReflection) : tex2D(_RightEyeTexture, uvReflection);
					float4 noiseColor = tex2D(_NoiseMask, i.uv) * _NoiseColor * _NoisePower;
					float4 finalColor = texColor + noiseColor;

					return saturate(finalColor);
				}

			ENDCG
		}
	}

	Fallback "Diffuse"
}