Shader "Custom/HeatmapZones"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_ZONES 16

            float4 _Zones[MAX_ZONES];        // xCenter, yCenter, sizeX, sizeY
            float4 _ZoneIntensity[MAX_ZONES];
            int _ZoneCount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 Heatmap(float v)
            {
                return saturate(float3(
                    1.5 - abs(4.0 * v - 3.0),
                    1.5 - abs(4.0 * v - 2.0),
                    1.5 - abs(4.0 * v - 1.0)
                ));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float heat = 0.0;

                for (int z = 0; z < _ZoneCount; z++)
                {
                    float2 center = _Zones[z].xy;
                    float2 size   = _Zones[z].zw * 0.5; // moitié taille
                    float intensity = _ZoneIntensity[z].x;

                    // calcul si le pixel est à l’intérieur du rectangle
                    float dx = saturate(1.0 - abs(i.uv.x - center.x) / size.x);
                    float dy = saturate(1.0 - abs(i.uv.y - center.y) / size.y);
                    heat += dx * dy * intensity;
                }

                return fixed4(Heatmap(saturate(heat)), 1.0);
            }
            ENDCG
        }
    }
}
