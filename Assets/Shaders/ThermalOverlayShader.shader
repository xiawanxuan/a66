Shader "ArcSim/ThermalOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HeatTex ("Heat Map", 2D) = "white" {}
        _MinTemp ("Min Temperature", Float) = 300
        _MaxTemp ("Max Temperature", Float) = 20000
        _Opacity ("Overlay Opacity", Range(0, 1)) = 0.6
        _ColorRamp ("Color Ramp", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+5" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 heatUv : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _HeatTex;
            float4 _HeatTex_ST;
            sampler2D _ColorRamp;
            float _MinTemp;
            float _MaxTemp;
            float _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.heatUv = TRANSFORM_TEX(v.uv, _HeatTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            float4 ThermalColor(float t)
            {
                float4 c;
                if (t < 0.25)
                {
                    c = float4(0, 0, 0.5 + t * 2, 1);
                }
                else if (t < 0.5)
                {
                    float s = (t - 0.25) * 4;
                    c = float4(0, s, 1 - s * 0.5, 1);
                }
                else if (t < 0.75)
                {
                    float s = (t - 0.5) * 4;
                    c = float4(s, 1, 0, 1);
                }
                else
                {
                    float s = (t - 0.75) * 4;
                    c = float4(1, 1 - s * 0.5, s, 1);
                }
                return c;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 baseCol = tex2D(_MainTex, i.uv);

                float heatVal = tex2D(_HeatTex, i.heatUv).r;
                float normalizedTemp = saturate((heatVal - _MinTemp) / (_MaxTemp - _MinTemp));

                float4 heatCol = ThermalColor(normalizedTemp);

                float4 col = lerp(baseCol, heatCol, _Opacity * normalizedTemp);
                col.a = 1.0;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
