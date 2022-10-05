// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "QuickVR/CalibrationScreen" {

	Properties {
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
	}

    SubShader {
    
    	Tags 
	    { 
	    	"Queue" = "Overlay-1"
	    }

		Pass {
	    	
	    	Lighting Off
	        Cull Off
	        ZWrite Off
	        ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
	        	            
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#include "UnityCG.cginc"

				///////////////////////////////////////////////////
				// UNIFORMS
				///////////////////////////////////////////////////
				uniform sampler2D _MainTex;		//The texture that will be distorted
				uniform float4 _Color;
					
				struct a2v {
				    float4 position : POSITION;	
				    float2 uv 		: TEXCOORD0;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
				    float4 position : POSITION;
				    float2 uv 		: TEXCOORD0;

					UNITY_VERTEX_OUTPUT_STEREO
				};

				///////////////////////////////////////////////////
				// VERTEX SHADER
				///////////////////////////////////////////////////
				v2f vert(a2v IN){
				    v2f OUT;

					UNITY_SETUP_INSTANCE_ID(IN); //Insert
					UNITY_INITIALIZE_OUTPUT(v2f, OUT); //Insert
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); //Insert

				    OUT.position = UnityObjectToClipPos(IN.position);
				    OUT.uv = float2(IN.uv.x, 1.0 - IN.uv.y);
				    return OUT;
				}
					
				///////////////////////////////////////////////////
				// FRAGMENT SHADER
				///////////////////////////////////////////////////
				half4 frag(v2f IN) : COLOR {   
				    half4 col = tex2D(_MainTex, IN.uv) * _Color;
				    return col;
				}

			ENDCG
	    }
	}
}
