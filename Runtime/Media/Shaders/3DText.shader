Shader "GUI/3D Text Shader" {
	Properties{
		_MainTex("Font Texture", 2D) = "white" {}
		_Color("Text Color", Color) = (1,1,1,1)
		_ZTest("ZTest", Int) = 0
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "Overlay" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent" 
		}
		
		Lighting Off 
		Cull Off 
		ZWrite Off 
		ZTest Always//[_ZTest]
		Fog { Mode Off }
		
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass 
		{

			Color[_Color]
			SetTexture[_MainTex] {
				combine primary, texture * primary
			}

		}
	}
}