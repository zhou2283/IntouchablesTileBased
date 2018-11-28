// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

/*
    Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

Shader "PIDI Shaders Collection/2D/Reflection Shaders/Render Texture/Distorted (Parallax Composite)"
{

	Properties {

		[Space(12)]
		[Header(General Properties)]
		[Space(12)]
        [PerRendererData]_MainTex( "Main Texture", 2D ) = "white"{}
		
        [Space(12)]
		[Header(Reflection Properties)]
		[Space(12)]
        [PerRenderData]_Color( "Reflection Color (RGB) Opacity(A)",Color) = (1,1,1,1)
        [PerRenderData]_ColorB("Background reflection Color (RGB) Opacity (A)", Color ) = (1,1,1,1)
        [PerRendererData]_Reflection2D( "Reflection 2D", 2D ) = "white" {}
        [PerRendererData]_BackgroundReflection( "Reflected Background", 2D ) = "black" {}
        _Normalmap( "Normalmap", 2D ) = "bump"{}
        _DistortionSpeed( "DistortionSpeed", Vector ) = (0,0,0,0)
		[PerRenderData]_SurfaceLevel("Surface Level",Range(-5,5)) = 0
        _SurfaceDistortion( "Surface Distortion", Range(0,0.1)) = 0.05
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
            sampler2D _BackgroundReflection;
            float4 _BackgroundReflection_ST;
            float4 _Reflection2D_ST;
            sampler2D _Normalmap;
            float4 _Normalmap_ST;
			float _SurfaceLevel;
            float _SurfaceDistortion;
            float4 _DistortionSpeed;
			half4 _Color;
            half4 _ColorB;
            float _GameCam;
            float _AlphaBackground;
            half _InvertProjection;

            struct v2f
            {
                float2 screenUV : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
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
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
				float4 pos = ComputeScreenPos(o.pos);
                pos.xy/=pos.w;
                o.screenUV = pos.xy;
                o.screenUV.y = srfc.y+abs(srfc.y-pos.y);
                return o;
            }

           

            half4 frag(v2f i) : SV_Target{
                
                float2 mov = _DistortionSpeed.xy*_Time;
                half4 col = tex2D(_MainTex, i.uv);
                fixed3 norms = UnpackNormal(tex2D(_Normalmap, i.uv*_Normalmap_ST.xy+mov));
                half4 fGround = tex2D(_Reflection2D, i.screenUV+norms*_SurfaceDistortion);
                half4 bgcolor = fGround*_Color*_Color.a*col.a+(1-fGround.a)*tex2D(_BackgroundReflection, i.screenUV+norms*_SurfaceDistortion)*_ColorB*_ColorB.a*col.a;
                return bgcolor*i.color.a+col*col.a*i.color*i.color.a*_AlphaBackground;
            }
            ENDCG
        }

    }
}