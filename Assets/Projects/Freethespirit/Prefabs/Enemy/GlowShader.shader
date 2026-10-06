Shader "Custom/FlowFieldLines"
{
    Properties
    {
        _FlowMap("Flow Map (iChannel0: RG=velocity, B/A=extra)", 2D) = "black" {}
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

            TEXTURE2D(_FlowMap); SAMPLER(sampler_FlowMap);
            float4 _FlowMap_TexelSize; // .zw = résolution en pixels

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.xx + p3.yz) * p3.zy) * 2.0 - 1.0;
            }

            float2 vf(float2 v)
            {
                return SAMPLE_TEXTURE2D_LOD(_FlowMap, sampler_FlowMap, v, 0).xy;
            }

            float ln(float2 p, float2 a, float2 b)
            {
                return length(p - a - (b - a) * clamp(dot(p - a, b - a) / dot(b - a, b - a), 0.0, 1.0));
            }

            float ff(float2 U, float2 o, float resX)
            {
                float q = 0.3 * resX;
                float2 V = floor(U * q + 0.5 + o) / q;
                V += 0.5 * hash22(floor(V * _FlowMap_TexelSize.zw)) / q;

                float2 v = vf(V);
                float a = 1e3;

                for (int i = 0; i < 4; i++)
                {
                    v = 0.5 * vf(V);
                    a = min(a, 1.2 * ln(U, V, V + v));
                    V += v;
                }

                return max(1.0 - resX * 0.4 * a, 0.0);
            }

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 U = IN.localTexcoord.xy;
                float2 texRes = _CustomRenderTextureInfo.xy;

                float c = 0.0;
                [unroll]
                for (int x = -2; x <= 2; x++)
                {
                    [unroll]
                    for (int y = -2; y <= 2; y++)
                    {
                        c += 0.33 * ff(U, float2(x, y), texRes);
                    }
                }

                float4 me = tex2D(_SelfTexture2D, U);

                // build components as plain scalars first
                float3 col = c.xxx * (0.3 + 5.0 * me.w);
                col.y *= length(me.xy) * 10.0;
                col.x *= max(0.0, 0.2 + me.z);
                col.z *= max(0.0, 0.2 - me.z);

                float4 C = float4(col, 0.0);
                C *= float4(1, 1.5, 2, 1) * (1.0 + 0.5 * C * C);

                return C;
            }
            ENDHLSL
        }
    }
}