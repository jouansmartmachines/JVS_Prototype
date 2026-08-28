Shader "Custom/SpriteGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowSize ("Glow Size", Range(0,1)) = 0.1
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

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
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowSize;
            float _GlowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Distance from center for glow
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // Glow factor
                float glowFactor = smoothstep(0.5, 0.5 - _GlowSize, dist);
                fixed4 glow = _GlowColor * glowFactor * _GlowIntensity;

                // Add glow only outside transparent areas
                col.rgb = col.rgb + glow.rgb * col.a;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
