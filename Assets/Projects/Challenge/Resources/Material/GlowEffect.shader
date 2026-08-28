Shader "Custom/2DSpriteGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 1.5
        _GlowSize ("Glow Size", Range(0.0,0.05)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Convert GlowSize to pixel size for visibility
                float2 texelSize = float2(1.0/_ScreenParams.x, 1.0/_ScreenParams.y);
                float2 offsets[8] = {
                    float2(_GlowSize, 0),
                    float2(-_GlowSize, 0),
                    float2(0, _GlowSize),
                    float2(0, -_GlowSize),
                    float2(_GlowSize, _GlowSize),
                    float2(-_GlowSize, _GlowSize),
                    float2(_GlowSize, -_GlowSize),
                    float2(-_GlowSize, -_GlowSize)
                };

                fixed4 glow = fixed4(0,0,0,0);

                for (int j = 0; j < 8; j++)
                {
                    // Multiply by texel size to make offsets visible
                    fixed4 sampleCol = tex2D(_MainTex, i.uv + offsets[j] * texelSize * 100);
                    glow += sampleCol * sampleCol.a;
                }

                glow *= _GlowColor * (_GlowIntensity / 8.0);

                // Add glow to original color
                col.rgb += glow.rgb;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
