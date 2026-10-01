Shader "Custom/VortexFractal_ScreenSpace"
{
    Properties
    {
        _Color1("Color 1 (Electric Blue)", Color) = (0, 0.333, 1, 1)
        _Color2("Color 2 (Hot Pink)", Color) = (1, 0, 0.667, 1)
        _BgColor("Background Color", Color) = (0.02, 0.012, 0.039, 1)
        _Speed("Speed", Float) = 0.3
        _Complexity("Complexity (iterations)", Range(1, 20)) = 8
        _Density("Density", Float) = 3.2535
        _Intensity("Intensity", Float) = 0.03758
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _Color1, _Color2, _BgColor;
            half _Speed, _Complexity, _Density, _Intensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float2 rot(float2 v, float a)
            {
                float s = sin(a), c = cos(a);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Coordonnée basée sur l'espace écran, indépendante de l'UV du mesh ---
                float2 screenUV = IN.positionCS.xy / _ScreenParams.xy;
                float2 p = (screenUV * 2.0 - 1.0);
                p.x *= _ScreenParams.x / _ScreenParams.y; // correction d'aspect

                float2 originalP = p;

                half3 finalColor = 0;
                float time = _Time.y * _Speed * 0.5;

                p = rot(p, 0.2);

                int iterations = (int)_Complexity;

                for (int i = 1; i <= 20; i++)
                {
                    if (i > iterations) break;

                    p = rot(p, sin(time * 0.05) * 0.1 + 0.08);

                    float2 q = p;
                    float dist = length(p);
                    q = rot(q, dist * _Density * 0.25 - time * 0.3);

                    float freq = _Density * 0.8;
                    q.x += sin(q.y * freq + time * 0.5 + i * 0.15) * 0.5;
                    q.y += cos(q.x * freq - time * 0.5 - i * 0.15) * 0.5;

                    float2 r = q;
                    r.x += sin(q.y * freq * 2.0 - time * 0.8) * 0.25;
                    r.y += cos(q.x * freq * 2.0 + time * 0.8) * 0.25;

                    float wave = sin(r.x * freq * 1.5 + time) * 0.6
                               + cos(r.y * freq * 0.5 - time * 0.7) * 0.4;

                    float d = abs(r.y - wave);

                    float core = 0.005 / max(d, 0.002);
                    float soft1 = exp(-d * 8.0) * 0.6;
                    float soft2 = exp(-d * 2.0) * 0.2;

                    float mixFactor = sin(r.x * 3.0 + r.y * 2.0 + time + i * 1.6) * 0.5 + 0.5;
                    half3 layerColor = lerp(_Color1.rgb, _Color2.rgb, mixFactor);

                    float attenuation = 1.0 / (i * 0.6 + 1.0);
                    finalColor += layerColor * (core + soft1 + soft2) * _Intensity * attenuation * 30.0;

                    p = r * 1.05;
                }

                finalColor += _BgColor.rgb * 0.5;

                float vignette = 1.0 - smoothstep(0.5, 2.5, length(originalP));
                finalColor *= vignette;

                // Tone mapping ACES (approximation)
                finalColor = finalColor * (2.51 * finalColor + 0.03) / (finalColor * (2.43 * finalColor + 0.59) + 0.14);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
