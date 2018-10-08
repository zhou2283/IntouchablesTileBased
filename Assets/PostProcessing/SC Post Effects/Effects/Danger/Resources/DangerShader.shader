Shader "Hidden/SC Post Effects/Danger"
{
	HLSLINCLUDE

	#include "PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_Overlay, sampler_Overlay);
	float4 _Color;
	float _Size;
	float _Refraction;


	float Vignette(float2 uv)
	{
		float vignette = uv.x * uv.y * (1 - uv.x) * (1 - uv.y);
		return clamp(16.0 * vignette, 0, 1);
	}


	float4 Frag(VaryingsDefault i): SV_Target
	{

		float overlay = SAMPLE_TEXTURE2D(_Overlay, sampler_Overlay, i.texcoordStereo).a;

		float vignette = Vignette(i.texcoordStereo);
		overlay = (overlay * _Size) ;
		vignette = (vignette / overlay);
		vignette = 1-saturate(vignette);

		//return float4(vignette, vignette, vignette, 1);

		float3 blendedColor = _Color.rgb;

		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo + (vignette)*_Refraction);

		screenColor.rgb = lerp(screenColor, blendedColor, vignette * _Color.a);

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