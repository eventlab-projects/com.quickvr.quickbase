#ifndef QUICK_MIRROR_REFLECTION_INC
#define QUICK_MIRROR_REFLECTION_INC

/////////////////////////////////////////////////////////
// UNIFORM PARAMETERS 
/////////////////////////////////////////////////////////
uniform sampler2D _LeftEyeTexture;			//The texture containing the reflection of the DEFAULT geometry for the left eye
uniform sampler2D _RightEyeTexture;			//The texture containing the reflection of the DEFAULT geometry for the right eye
uniform sampler2D _NoiseMask;				//A texture used to create imperfections in the reflection
uniform float _ReflectionPower;				//Indicates how much light is the reflection. It is used to simulate the light lost during the reflection
uniform float _NoisePower;					//Indicates how much powerful is the noise texture
uniform float4 _NoiseColor;					//The color of the noise

uniform int REFLECTION_INVERT_Y;
uniform int STEREO_TARGET_EYE;

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 screenPos : TEXCOORD1;
	float4 pos : SV_POSITION;
};

float2 GetProjUV(float4 screenPos) 
{
	float2 projUV = screenPos.xy / screenPos.w;

#if UNITY_SINGLE_PASS_STEREO
	float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
	projUV = (projUV - scaleOffset.zw) / scaleOffset.xy;
#endif

	return projUV;
}

v2f vert(float4 pos : POSITION, float2 uv : TEXCOORD0)
{
	v2f o;
	o.pos = UnityObjectToClipPos(pos);
	o.uv = uv;
	o.screenPos = ComputeScreenPos(o.pos);
	return o;
}

int isEyeLeft() 
{
	if (STEREO_TARGET_EYE == 1) return 1;
	else if (STEREO_TARGET_EYE == 2) return 0;

	return unity_StereoEyeIndex == 0 ? 1 : 0;
}

fixed4 ComputeFinalColor(float2 uvReflection, float2 uvTex) 
{
	//return isEyeLeft() ? fixed4(1,0,0,1) : fixed4(0,1,0,1);

	fixed4 refl = isEyeLeft() ? tex2D(_LeftEyeTexture, uvReflection) : tex2D(_RightEyeTexture, uvReflection);
	fixed4 noiseColor = tex2D(_NoiseMask, uvTex) * _NoiseColor;
	fixed4 finalColor = refl * _ReflectionPower + noiseColor * _NoisePower;

	return saturate(finalColor);
}

fixed4 ComputeFinalColor_MOBILE(float2 uvReflection, float2 uvTex)
{
	//return isEyeLeft() ? fixed4(1,0,0,1) : fixed4(0,1,0,1);

	return isEyeLeft() ? tex2D(_LeftEyeTexture, uvReflection) : tex2D(_RightEyeTexture, uvReflection);
}

#endif