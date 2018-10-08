Shader "Hidden/SC Post Effects/Refraction"
{
	HLSLINCLUDE

	#include "PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_RefractionTex, sampler_RefractionTex);
	float _Amount;

	float4 Frag(VaryingsDefault i): SV_Target
	{
		float2 dudv = SAMPLE_TEXTURE2D(_RefractionTex, sampler_RefractionTex, i.texcoordStereo).rg;

		float2 refraction = lerp(i.texcoordStereo, (i.texcoordStereo ) + dudv.rg, _Amount);

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, refraction);

		return float4(screenColor.rgb, screenColor.a);
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