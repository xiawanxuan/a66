Shader "ArcSim/ArcGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.6, 0.2, 1)
        _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 3.0
        _GlowRadius ("Glow Radius", Range(0, 1)) = 0.3
        _TemperatureScale ("Temperature Scale", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float4 _CoreColor;
            float _GlowIntensity;
            float _GlowRadius;
            float _TemperatureScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = length(i.uv - center);

                float coreMask = smoothstep(0.15, 0.0, dist);
                float glowMask = smoothstep(_GlowRadius, 0.0, dist);

                float tempFactor = i.color.r * _TemperatureScale;

                float4 coreCol = _CoreColor * coreMask * (1.0 + tempFactor);
                float4 glowCol = _GlowColor * glowMask * _GlowIntensity * i.color.a;

                float4 col = coreCol + glowCol;
                col.a = glowMask * i.color.a;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Particles/Alpha Blended"
}
