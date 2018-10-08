Shader "Hidden/SC Post Effects/Pixelize"
{
	HLSLINCLUDE

	#include "PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Resolution;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 uv = half2((int)(i.texcoordStereo.x / _Resolution) * _Resolution, (int)((i.texcoordStereo.y) / _Resolution) * _Resolution);
		float4 pixelatedColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		return pixelatedColor;
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			ENDHLSL
		}
	}
}