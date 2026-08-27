Shader "Custom/HexaSprite_Full"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,0,0,1)
        _Multiplier("OneMinus Multiplier", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }

        Cull Off
        ZWrite Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TintColor;
            float _Multiplier;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 texCol = tex2D(_MainTex, i.uv);

                // OneMinus pour le blend avec TintColor
                float3 oneMinus = (1.0 - texCol.rgb) * _Multiplier;

                // Convertir en facteur unique pour uniformité
                float factor = saturate(dot(oneMinus, float3(0.299,0.587,0.114)));

                // Mélange couleur
                float3 result = lerp(texCol.rgb, _TintColor.rgb, factor);

                // Alpha = alpha de la texture originale (garde la pleine opacité)
                float alpha = texCol.a;

                return fixed4(result, alpha);
            }
            ENDCG
        }
    }
}
