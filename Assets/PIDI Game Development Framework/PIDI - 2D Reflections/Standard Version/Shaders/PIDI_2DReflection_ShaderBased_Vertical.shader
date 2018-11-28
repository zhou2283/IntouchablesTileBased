// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

/*
    Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/
Shader "PIDI Shaders Collection/2D/Reflection Shaders/Shader Based/Simple Vertical"
{

	Properties {

		[Space(12)]
		[Header(General Properties)]
		[Space(12)]
        [Toggle]_InvertProjection("Invert Projection", Float) = 0
        [PerRendererData]_MainTex( "Main Texture", 2D ) = "white"{}
		[Space(12)]
		[Header(Reflection Properties)]
		[Space(12)]
        _Color( "Reflection Color (RGB) Opacity(A)",Color) = (1,1,1,1)
		_SurfaceLevel("Surface Level",Range(-5,5)) = 0
	}

    SubShader
    {   
		
		// Draw ourselves after all opaque geometry
        Tags { "Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True" }

      
        GrabPass
        {
        }

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

			sampler2D _GrabTexture;
            sampler2D _MainTex;
			float _SurfaceLevel;
			half4 _Color;
            half _InvertProjection;

            struct v2f
            {
                float2 screenUV : TEXCOORD0;
                float2 uv : TEXCOORD1;
                fixed4 color : COLOR;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_full v) {
                 v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, v.vertex));
                o.uv = v.texcoord;
                o.color = v.color;
                float4 srfc = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(_SurfaceLevel,0,0,1) ) );
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
				float4 pos = ComputeGrabScreenPos(o.pos);
                o.screenUV = pos.xy;
                o.screenUV.x = lerp(pos.x+_SurfaceLevel, 1-pos.x+srfc.x, 1-_InvertProjection);
                return o;
            }

           

            half4 frag(v2f i) : SV_Target
            {   
                half4 col = tex2D(_MainTex, i.uv);
                half4 bgcolor = tex2D(_GrabTexture, i.screenUV)*col.a;
                return bgcolor*col*_Color*i.color;
            }
            ENDCG
        }

    }
}