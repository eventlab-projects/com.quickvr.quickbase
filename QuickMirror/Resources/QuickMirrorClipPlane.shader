Shader "QuickVR/QuickMirrorClipPlane" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		LOD 200

		CGPROGRAM

		#include "QuickMirrorClipPlane.cginc"

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface clipSurf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		ENDCG
	}

	SubShader{
			Tags{ "RenderType" = "Transparent" }
			LOD 200

			CGPROGRAM

			#include "QuickMirrorClipPlane.cginc"

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface clipSurf Standard fullforwardshadows alpha:fade

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			ENDCG
		}

		SubShader{
			Tags{ "RenderType" = "TransparentCutout" }
			LOD 200

			CGPROGRAM

			#include "QuickMirrorClipPlane.cginc"

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface clipSurf Standard fullforwardshadows alphatest:_Cutoff

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			ENDCG
		}
	
		FallBack "Diffuse"
}
