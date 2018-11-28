// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

/*
    Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

Shader "PIDI Shaders Collection/2D/Reflection Shaders/Render Texture/Water Lite"
{

	Properties {

		[Space(12)]
		[Header(General Properties)]
		[Space(12)]
        [PerRendererData]_MainTex( "Main Texture", 2D ) = "white"{}
		
        [Space(12)]
		[Header(Reflection Properties)]
		[Space(12)]
        [PerRendererData]_Color( "Reflection Color",Color) = (1,1,1,1)
        _ReflectionFade("Reflection Fade", Range(0,1)) = 1
        [PerRendererData]_Reflection2D( "Reflection 2D", 2D ) = "white" {}
        _Normalmap( "Normalmap", 2D ) = "bump"{}
        _DistortionSpeed( "DistortionSpeed", Vector ) = (0,0,0,0)
		[PerRenderData]_SurfaceLevel("Surface Level",Range(-5,5)) = 0
        _SurfaceDistortion( "Surface Distortion", Range(0,0.1)) = 0.05
        _TransparencyFactor("Transparency",Range(0,2)) = 0.5
        _ReflectivityFactor("Reflectivity",Range(0,2)) = 0.5
        [PerRendererData]_GameCam("Game Cam",Float) = 0
        [PerRendererData]_AlphaBackground("Alpha Background",Float) = 0
	}

    SubShader
    {   
		
		// Draw ourselves after all opaque geometry
        Tags { "Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"  }

        Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
        // Render the object with the texture generated above, and invert the colors
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
			sampler2D _Reflection2D;
            sampler2D _Normalmap;
            float4 _Normalmap_ST;
			float _SurfaceLevel;
            float _SurfaceDistortion;
            float4 _DistortionSpeed;
			half4 _Color;
            float _GameCam;
            float _AlphaBackground;
            half _InvertProjection;
            half _ReflectivityFactor;
            half _TransparencyFactor;
            half _ReflectionFade;

            struct v2f
            {
                float2 screenUV : TEXCOORD0;
                float2 screenUV2 : TEXCOORD2;
                float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float fade : COLOR1;
            };

            v2f vert(appdata_full v) {
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, v.vertex));
                float4 srfc = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(0,_SurfaceLevel,0,1) ) );
                srfc = ComputeScreenPos(srfc);
                srfc.xy /=srfc.w;
                o.uv = v.texcoord;
                o.color = v.color;
                o.fade = v.vertex.y;
				float4 pos = ComputeScreenPos(o.pos);
                
                pos.xy/=pos.w;
                o.screenUV = pos.xy;
                o.screenUV2 = pos.xy;
                o.screenUV.y = srfc.y+abs(srfc.y-pos.y)*2;
                return o;
            }

           

            half4 frag(v2f i) : SV_Target{
                
                float2 mov = _DistortionSpeed.xy*_Time;
                half4 col = tex2D(_MainTex, i.uv);
                fixed3 norms = UnpackNormal(tex2D(_Normalmap, i.uv*_Normalmap_ST.xy+mov));
                half4 bgcolor = tex2D(_Reflection2D, i.screenUV+norms*_SurfaceDistortion)*_ReflectivityFactor*( pow( i.uv.y, 16*(1-_ReflectionFade) ) )+tex2D(_Reflection2D, i.screenUV2+norms*_SurfaceDistortion*0.65)*half4(0.5,1,.9,1)*_TransparencyFactor;
                bgcolor = half4(bgcolor.rgb,col.a);
                return bgcolor*col*_Color*i.color;
            }
            ENDCG
        }

    }
}