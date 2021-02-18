// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "QuickVR/Unlit/TransparentGradient" 
{

    Properties
    {
        _ColorTop("ColorTop", Color) = (1,1,1,1)
        _ColorBottom("ColorBottom", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
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
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 vertex   :   SV_POSITION;
                    half3 texcoord  :   TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                fixed4 _ColorTop;
                fixed4 _ColorBottom;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.texcoord.z = v.vertex.y;
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord.xy) * lerp(_ColorBottom, _ColorTop, i.texcoord.z);
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
                }
            ENDCG
        }
    }

}