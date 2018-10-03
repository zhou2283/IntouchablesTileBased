// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Alpha Masked/Unlit Alpha Masked - World Coords"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[HideInInspector][Toggle] _Enabled("Mask Enabled", Float) = 1
		[HideInInspector][Toggle] _ClampHoriz("Clamp Alpha Horizontally", Float) = 0
		[HideInInspector][Toggle] _ClampVert("Clamp Alpha Vertically", Float) = 0
		[HideInInspector][Toggle] _UseAlphaChannel("Use Mask Alpha Channel (not RGB)", Float) = 0
		[HideInInspector]_AlphaTex("Alpha Mask", 2D) = "white" {}
		[HideInInspector]_ClampBorder("Clamping Border", Float) = 0.01
	}

		SubShader
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off Lighting Off ZWrite Off Fog { Color(0, 0, 0, 0) }

			Pass
			{
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag


			float _ClampHoriz;
			float _ClampVert;
			float _UseAlphaChannel;

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _ClampBorder;

			float4x4 _WorldToMask;

			float _Enabled;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uvMain : TEXCOORD1;
				float2 uvAlpha : TEXCOORD2;
				float4 color : COLOR;
			};

			float4 _MainTex_ST;
			float4 _AlphaTex_ST;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvMain = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;

				o.uvAlpha = float2(0, 0);
				if (_Enabled == 1)
				{
					o.uvAlpha = mul(_WorldToMask, mul(unity_ObjectToWorld, v.vertex)).xy + float2(0.5f, 0.5f);

					o.uvAlpha = o.uvAlpha * _AlphaTex_ST.xy + _AlphaTex_ST.zw;
				}

				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 texcol;
				if (_Enabled > 0){

					float2 alphaCoords = i.uvAlpha;

					if (_ClampHoriz)
						alphaCoords.x = clamp(alphaCoords.x, _ClampBorder, 1.0 - _ClampBorder);

					if (_ClampVert)
						alphaCoords.y = clamp(alphaCoords.y, _ClampBorder, 1.0 - _ClampBorder);

					texcol = tex2D(_MainTex, i.uvMain);

					if (_UseAlphaChannel)
						texcol.a *= tex2D(_AlphaTex, alphaCoords).a;
					else
						texcol.a *= tex2D(_AlphaTex, alphaCoords).rgb;
					
				}
				else
				{
					texcol = tex2D(_MainTex, i.uvMain);
				}

				return texcol * i.color;
			}

		ENDCG
		}
	}

			Fallback "Unlit/Texture"
}
