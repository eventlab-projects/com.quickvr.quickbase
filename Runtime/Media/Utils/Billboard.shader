Shader "Custom/Billboard" {

 	Properties {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }

	SubShader {
		
		Tags 
		{
			"Queue" = "Overlay" 
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
    		
    		/////////////////////////////////////
			// UNIFORMS
			/////////////////////////////////////
			uniform sampler2D _MainTex;
			uniform half4 _Color;
			uniform float4x4 BILLBOARD_MATRIX;

			/////////////////////////////////////
			// VERTEX SHADER
			/////////////////////////////////////
		    struct v2f {
		        float4 pos	: POSITION;
		        float2 uv 	: TEXCOORD0;
		    };

    		v2f vert(v2f IN) {
    			v2f OUT;
    			OUT.pos = mul(BILLBOARD_MATRIX, IN.pos);
    			OUT.uv = IN.uv;
        		return OUT;
    		}

    		/////////////////////////////////////
			// FRAPGMENT SHADER
			/////////////////////////////////////
			half4 frag(v2f IN) : COLOR{
				half4 col = tex2D(_MainTex, IN.uv) * _Color;
				return col;
			}
    		
    		ENDCG
	
		}
	}
}