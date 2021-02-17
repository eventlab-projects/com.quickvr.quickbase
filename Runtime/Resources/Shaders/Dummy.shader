Shader "QuickVR/Dummy"
{
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			float4 vert(float4 v : POSITION) : SV_POSITION
			{
				return UnityObjectToClipPos(v);
			}
			
			fixed4 frag() : SV_Target
			{
				//if (true) discard;
				return fixed4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}
