// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Alpha Masked/Sprites Alpha Masked - World Coords"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData]_Color ("Tint", Color) = (1,1,1,1)
		[PerRendererData][Toggle] _PixelSnap("Pixel snap", Float) = 0
		[PerRendererData][Toggle] _Enabled ("Mask Enabled", Float) = 1
		[PerRendererData][Toggle] _ClampHoriz ("Clamp Alpha Horizontally", Float) = 0
		[PerRendererData][Toggle] _ClampVert ("Clamp Alpha Vertically", Float) = 0
		[PerRendererData][Toggle] _UseAlphaChannel ("Use Mask Alpha Channel (not RGB)", Float) = 0
		[PerRendererData]_AlphaTex ("Alpha Mask", 2D) = "white" {}
		[PerRendererData]_ClampBorder ("Clamping Border", Float) = 0.01
		[PerRendererData]_IsThisText("Is This Text?", Float) = 0
		
		[PerRendererData]_StencilComp ("Stencil Comparison", Float) = 8
		[PerRendererData]_Stencil ("Stencil ID", Float) = 0
		[PerRendererData]_StencilOp ("Stencil Operation", Float) = 0
		[PerRendererData]_StencilWriteMask ("Stencil Write Mask", Float) = 255
		[PerRendererData]_StencilReadMask ("Stencil Read Mask", Float) = 255

		[PerRendererData]_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		ColorMask [_ColorMask]
		
		Pass
		{
		CGPROGRAM
			#include "UnityCG.cginc"
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY _SCREEN_SPACE_UI
			
			float _ClampHoriz;
			float _ClampVert;
			float _UseAlphaChannel;
			float _PixelSnap;

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _ClampBorder;
			
			float _IsThisText;
			float _Enabled;
			
			
			float4x4 _WorldToMask;

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				float2 uvMain : TEXCOORD1;
				float2 uvAlpha : TEXCOORD2;
			};
			
			fixed4 _Color;
			float4 _MainTex_ST;
			float4 _AlphaTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvMain = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;
				

				o.uvAlpha = float2(0,0);
				
				//Using endVert and not already existing uvAlpha, because we need to retain y and z.
				float4 endVert;

				if (_Enabled == 1)
				{
					#ifdef _SCREEN_SPACE_UI

					endVert = v.vertex;

					#else

					endVert = mul(unity_ObjectToWorld, v.vertex);

					#endif

					o.uvAlpha = mul(_WorldToMask, endVert).xy + float2(0.5f, 0.5f);
					o.uvAlpha = o.uvAlpha * _AlphaTex_ST.xy + _AlphaTex_ST.zw;
				}

				if (_PixelSnap)
					o.pos = UnityPixelSnap(o.pos);
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 texcol;
				if (_Enabled > 0)
				{
					if (_ClampHoriz)
						i.uvAlpha.x = clamp(i.uvAlpha.x, _ClampBorder, 1.0 - _ClampBorder);
					
					if (_ClampVert)
						i.uvAlpha.y = clamp(i.uvAlpha.y, _ClampBorder, 1.0 - _ClampBorder);
					
					//Sample uv main
					texcol = tex2D(_MainTex, i.uvMain);
					texcol.a *= i.color.a;
					texcol.rgb = clamp(texcol.rgb + _IsThisText, 0, 1) * i.color.rgb;
					
					if (_UseAlphaChannel)
						texcol.a *= tex2D(_AlphaTex, i.uvAlpha).a;
					else
						texcol.a *= tex2D(_AlphaTex, i.uvAlpha).rgb;
					
					texcol.rgb *= texcol.a;
				}
				else
				{
					texcol = tex2D(_MainTex, i.uvMain);
					texcol.a *= i.color.a;
					texcol.rgb = clamp(texcol.rgb + _IsThisText, 0, 1) * i.color.rgb;
					texcol.rgb *= texcol.a;
				}
				
				return texcol;
			}
			
		ENDCG
		}
	}
	
	Fallback "Unlit/Texture"
}
