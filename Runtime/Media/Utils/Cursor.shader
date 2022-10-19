Shader "Custom/Cursor" {

	Properties {
		_MainTex("Main Texture", 2D) = "white" {}								
	}

    SubShader {
    
    	Tags 
	    { 
	    	"Queue" = "Overlay+1"
	    }

		Pass {
	    	Lighting Off
	        Cull Off
	        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
			ZTest Always
	            
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
					
				///////////////////////////////////////////////////
				// UNIFORMS
				///////////////////////////////////////////////////
				uniform sampler2D _MainTex;		//The texture that will be distorted
				uniform float4x4 SCALE_MATRIX;	
					
				struct a2v {
				    float4 position : POSITION;	
				    float2 uv 		: TEXCOORD0;
				};

				struct v2f {
				    float4 position : POSITION;
				    float2 uv 		: TEXCOORD0;
				};

				///////////////////////////////////////////////////
				// VERTEX SHADER
				///////////////////////////////////////////////////
				v2f vert(a2v IN){
				    v2f OUT;
				    float4 originVS = mul(UNITY_MATRIX_MV, float4(0,0,0,1));	//The origin of the object in View Space
				    float4 pos = mul(SCALE_MATRIX, float4(IN.position.xy, 0, 0));
				    OUT.position = mul(UNITY_MATRIX_P, originVS - float4(pos.xy, 0, 0));
				    OUT.position.z = 0;
				    OUT.uv = IN.uv;
				    return OUT;
				}
					
				///////////////////////////////////////////////////
				// FRAGMENT SHADER
				///////////////////////////////////////////////////
				half4 frag(v2f IN) : COLOR {   
				    half4 col = tex2D(_MainTex, IN.uv);
				    return col;
				}

			ENDCG
	    }
	}
}
