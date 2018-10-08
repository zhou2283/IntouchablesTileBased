Shader "Hidden/SC Post Effects/Fog"
{
	HLSLINCLUDE
	
	#include "PostProcessing/Shaders/StdLib.hlsl"
	#include "../../../Editor/ShaderLibraries/SCPE.hlsl"
	#include "../../../Editor/ShaderLibraries/Blending.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);
	TEXTURE2D_SAMPLER2D(_ColorGradient, sampler_ColorGradient);

	#pragma fragmentoption ARB_precision_hint_nicest

	uniform half _FarClippingPlane;
	uniform float4 _HeightParams;
	uniform float4 _DistanceParams;
	uniform int4 _SceneFogMode;
	uniform float4 _SceneFogParams;
	uniform float4 _DensityParams;
	uniform float4 _NoiseParams;
	uniform float4 _FogColor;
	uniform float4x4 clipToWorld;

	struct v2f {
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 worldDirection : TEXCOORD2;
		float time : TEXCOORD3;
	};

	v2f Vert(AttributesDefault v) {
		v2f o;
		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
		o.uv.xy = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
		o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
		float4 clip = float4(o.uv.xy * 2 - 1, 0.0, 1.0);
		o.worldDirection = mul(clipToWorld, clip.rgba) - _WorldSpaceCameraPos;
		o.time = _Time.y;

		//UNITY_SINGLE_PASS_STEREO
		o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);

		return o;
	}

	half ComputeFogFactor(float coord)
	{
		float fogFac = 0.0;
		if (_SceneFogMode.x == 1) // linear
		{
			// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
			fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
		}
		if (_SceneFogMode.x == 2) // exp
		{
			// factor = exp(-density*z)
			fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
		}
		if (_SceneFogMode.x == 3) // exp2
		{
			// factor = exp(-(density*z)^2)
			fogFac = _SceneFogParams.x * coord; fogFac = exp2(-fogFac * fogFac);
		}
		return saturate(fogFac);
	}

	float ComputeDistance(float3 wpos, float depth)
	{
		float3 wsDir = _WorldSpaceCameraPos.xyz - wpos;
		float dist;
		//Radial distance
		if (_SceneFogMode.y == 1)
			dist = length(wsDir);
		else
			dist = depth * _ProjectionParams.z;
		//Start distance
		dist -= _ProjectionParams.y;
		//Density
		dist *= _DensityParams.x;
		return dist;
	}

	float ComputeHeight(float3 wpos)
	{
		float3 wsDir = _WorldSpaceCameraPos.xyz - wpos;
		float FH = _HeightParams.x;
		float3 C = _WorldSpaceCameraPos;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = _HeightParams.w * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y - FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1 - 2 * k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
		return g;
	}

	float4 ComputeFog(v2f i) {

		half4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
		float depth = Linear01Depth(rawDepth);

		float skyMask = 1;
		if (depth > 0.99) skyMask = 0;

		float3 worldPos = i.worldDirection * LinearEyeDepth(rawDepth) + _WorldSpaceCameraPos;

		//Fog start distance
		float g = _DistanceParams.x;

		//Distance fog
		float distanceFog = 0;
		if (_DistanceParams.z == 1) {
			distanceFog = ComputeDistance(worldPos, depth);
			g += distanceFog;
		}

		//Height fog
		float heightFog = 0;
		if (_DistanceParams.w == 1) {
				float noise = 1;
				if (_SceneFogMode.w == 1)
				{
					noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldPos.xz * _NoiseParams.x + (i.time * _NoiseParams.y * float2(0, 1))).r;
					noise = lerp(1, noise, _DensityParams.y * skyMask);
				}
				heightFog = ComputeHeight(worldPos);
				g += heightFog * noise;
			}

		//Fog density
		half fogFac = ComputeFogFactor(max(0.0, g));

		//Exclude skybox
		if (depth == _DistanceParams.y)
			fogFac = 1.0;

		//Color
		float4 fogColor = _FogColor.rgba;
		if (_SceneFogMode.z == 1)
		{
			fogColor = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, float2(LinearEyeDepth(rawDepth) / _FarClippingPlane, 0));
		}
	
		fogColor.a = fogFac;

		return fogColor;
	}

	float4 FragBlend(v2f i) : SV_Target
	{
		half4 screenColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

		float4 fogColor = ComputeFog(i);

		//Linear blend
		float3 blendedColor = lerp(fogColor.rgb, screenColor.rgb, fogColor.a);

		//Screen blend
		//blendedColor = BlendScreen(fogColor.rgb, screenColor.rgb);

		return float4(blendedColor.rgb, screenColor.a);
	}


	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment FragBlend

			ENDHLSL
		}
	}
}