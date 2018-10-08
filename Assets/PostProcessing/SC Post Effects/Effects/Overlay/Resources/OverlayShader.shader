Shader "Hidden/SC Post Effects/Overlay"
{
	HLSLINCLUDE

	#include "PostProcessing/Shaders/StdLib.hlsl"
	#include "../../../Editor/ShaderLibraries/Blending.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_OverlayTex, sampler_OverlayTex);
	half _BlendMode;
	float _Intensity;
	float _Tiling;

	float4 Frag(VaryingsDefault i): SV_Target
	{
		float2 uv = i.texcoordStereo;
		float4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		float4 overlay = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, uv * _Tiling);

		float3 color = 0;

		if (_BlendMode == 0) color = lerp(screenColor.rgb, overlay.rgb, overlay.a * _Intensity);
		if (_BlendMode == 1) color = lerp(screenColor.rgb, BlendAdditive(overlay.rgb, screenColor.rgb), overlay.a * _Intensity);
		if (_BlendMode == 2) color = lerp(screenColor.rgb, overlay.rgb * screenColor.rgb, overlay.a * _Intensity);
		if (_BlendMode == 3) color = lerp(screenColor.rgb, BlendScreen(overlay.rgb, screenColor.rgb), overlay.a * _Intensity);
		
		return float4(color.rgb, screenColor.a);
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