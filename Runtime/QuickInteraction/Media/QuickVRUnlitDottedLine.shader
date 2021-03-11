// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "QuickVR/Unlit/DottedLine" 
{

    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _Velocity("Velocity", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
            "RenderType" = "Transparent"
        }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata_t {
                    fixed4 color : COLOR;
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    half2 texcoord : TEXCOORD0;
                    fixed4 color : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color;
                half2 _Velocity;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) + _Velocity * _Time.y;
                    o.color = v.color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                    return col;
                }
            ENDCG
        }
    }

}