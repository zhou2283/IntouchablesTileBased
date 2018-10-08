Shader "Hidden/SC Post Effects/Tube Distortion"
{
	HLSLINCLUDE

	#include "PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float _Amount;


	float2 BuldgedUV(half2 uv, half amount, half zoom)
	{
		half2 center = uv.xy - half2(0.5, 0.5);
		half CdotC = dot(center, center);
		half f = 1.0 + CdotC * (amount * sqrt(CdotC));
		return f * zoom * center + 0.5;
	}

	float2 PinchUV(float2 uv)
	{
		uv = uv * 2.0 - 1.0;
		float2 offset = abs(uv.yx) * float2(_Amount , _Amount);
		uv = uv + uv * offset * offset;
		uv = uv * 0.5 + 0.5;
		return uv;
	}

	float EdgesUV(float2 uv)
	{
		float vignette = uv.x * uv.y * (1 - uv.x) * (1 - uv.y);
		//Shader warning in 'Hidden/SC Post Effects/Tube Distortion': pow(f, e) will not work for negative f, use abs(f) or conditionally handle negative values if you expect them at line 33 (on d3d11)
		vignette = clamp(pow(32.0 * vignette, 0.1), 0, 1);
		vignette = vignette + 0.5 * 0.1;
		return vignette;
	}


	float4 FragBuldge(VaryingsDefault i) : SV_Target
	{
		float2 uv = BuldgedUV(i.texcoordStereo, _Amount, lerp(1, 0.75, _Amount));
		
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		return float4(screenColor.rgb, screenColor.a);
	}

	float4 FragPinch(VaryingsDefault i) : SV_Target
	{
		float2 uv = PinchUV(i.texcoordStereo);

		float2 blackEdge = 1-ceil((uv.xy -1) * (uv.xy / i.texcoordStereo * 0.001)).rg;
		float crop = (blackEdge.r * blackEdge.g);

		//return float4(crop, crop, crop, 0);

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

		return float4(screenColor.rgb * crop, screenColor.a);
	}

	float4 FragBevel(VaryingsDefault i) : SV_Target
	{
		float2 uv = lerp(i.texcoordStereo, i.texcoordStereo * EdgesUV(i.texcoordStereo), _Amount);

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

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
			#pragma fragment FragBuldge

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragPinch

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragBevel

			ENDHLSL
		}
	}
}