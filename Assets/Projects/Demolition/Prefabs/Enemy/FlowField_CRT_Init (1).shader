// Init Material pour la Custom Render Texture "FlowField".
// A assigner dans : CRT asset > Initialization > Material (Initialization Source = Material).
// Seed du bruit dans .xy (direction) et .z, avec .w = 1 pour activer la luminosité.

Shader "CustomRenderTexture/FlowFieldInit"
{
    Properties
    {
        _Seed ("Seed", Float) = 0
        _DirScale ("Direction Scale", Float) = 0.02
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            Name "Init"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.5

            float _Seed;
            float _DirScale;

            float2 hash22(float2 p)
            {
                float3 p3 = frac(p.xyx * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.xx + p3.yz) * p3.zy) * 2.0 - 1.0;
            }

            float4 frag(v2f_init_customrendertexture IN) : COLOR
            {
                float2 uv = IN.texcoord.xy;
                float2 pix = uv * _CustomRenderTextureInfo.xy;

                float2 dir = hash22(pix + _Seed);
                float  z   = hash22(pix + _Seed + 17.0).x;

                // xy = petit vecteur directionnel de depart, z = variation -1..1, w = 1 = pleine luminosite
                return float4(dir * _DirScale, z, 1.0);
            }
            ENDCG
        }
    }
}
