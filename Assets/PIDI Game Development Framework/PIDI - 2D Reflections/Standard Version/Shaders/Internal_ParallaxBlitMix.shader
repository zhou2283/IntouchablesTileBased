// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

/*
    Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

Shader "PIDI Shaders Collection/2D/Internal/Parallax Blit"
{

	Properties {

		[HideInInspector]_MainTex("First Reflection",2D ) = "black"{}
        [HideInInspector]_SecondReflection("Second Reflection",2D ) = "black"{}

        [HideInInspector]_BetaReflections("Beta Reflections", Float ) = 0
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
		Blend One OneMinusSrcAlpha
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
            half _BetaReflections;
            sampler2D _MainTex;
            sampler2D _SecondReflection;

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
                srfc = ComputeGrabScreenPos(srfc);
                srfc.xy /=srfc.w;
                o.uv = v.texcoord;
                o.color = v.color;
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
				float4 pos = ComputeGrabScreenPos(o.pos);
                pos.xy/=pos.w;
                o.screenUV = pos.xy;
                o.screenUV.y = lerp( srfc.y-pos.y, srfc.y+abs(srfc.y-pos.y), 1-_BetaReflections );
                return o;
            }

           

            half4 frag(v2f i) : SV_Target{
                
                half4 background = tex2D(_MainTex, i.uv );
                half4 foreground = tex2D(_SecondReflection, i.uv ); 

                return background*(1-foreground.a)+foreground*foreground.a;
            }
            ENDCG
        }

    }
}