Shader "Hidden/SC Post Effects/Black Bars"
{
	HLSLINCLUDE

	#include "PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float _Size;

	float4 FragHorizontal(VaryingsDefault i): SV_Target
	{
		float2 uv = i.texcoordStereo;
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		half bars = (uv.y * (1 - uv.y)/0.9);
		bars = step(_Size, bars);

		//return float4(uv.x, uv.y, bars, 1);

		return float4(screenColor.rgb * bars, screenColor.a);
	}

	float4 FragVertical(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoordStereo;
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		//Max is 4:3 ratio
		half bars = (uv.x * (1-uv.x)/1.12);
		bars = step(_Size, bars);

		//return float4(bars, bars, bars, 1);

		return float4(screenColor.rgb * bars, screenColor.a);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragHorizontal

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment FragVertical

			ENDHLSL
		}
	}
}