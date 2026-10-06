// Polyhedral Gears - URP port
// -----------------------------------------------------------------------
// Portage manuel (Shadertoy GLSL -> HLSL/URP) du raymarching "Polyhedral
// Gears" (roues dentees animees, plaquees sur un icosaedre/dodecaedre en
// arrangement de Goldberg). Le shader d'origine s'appuie notamment sur la
// technique de pliage d'espace "Icosahedron Weave" de DjinnKahn
// (https://www.shadertoy.com/view/Xty3Dy).
//
// UTILISATION
// -----------
// C'est un shader auto-contenu (il calcule sa propre "scene" par SDF,
// sa propre lumiere, ses propres ombres) : il ne lit ni la lumiere URP,
// ni la profondeur de scene. Le plus simple est de l'appliquer sur un
// Quad qui remplit le champ de la camera (un peu comme un fond Shadertoy).
// Il peut aussi servir de base a une Renderer Feature "Blit" plein ecran
// si tu veux un vrai post-effet ; dans ce cas fournis un UV plein ecran
// a la place de SV_POSITION dans frag().
//
// Le shader est lourd (raymarching + shadows + AO), fidele a l'original
// qui prevenait deja d'un framerate bas sur les machines lentes. Les deux
// grosses boucles (trace/softShadow) sont marquees [loop] pour eviter un
// deroulage complet et limiter le temps de compilation ; les [unroll]
// concernent des boucles courtes (3, 4, 6 iterations).
//
// _ColorScheme (0/1/2) reproduit les trois palettes d'origine :
//   0 = Or et aluminium, 1 = Noir et chrome, 2 = Rose et chrome.
// -----------------------------------------------------------------------

Shader "Custom/URP/PolyhedralGears"
{
    Properties
    {
        [IntRange] _ColorScheme ("Palette (0=Or/Alu, 1=Noir/Chrome, 2=Rose/Chrome)", Range(0,2)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "PolyhedralGears"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _ColorScheme;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            #define FAR 20.0

            // ------------------------------------------------------------
            // "Globales" par pixel (equivalent des uniforms Shadertoy et
            // des variables globales utilisees comme sortie annexe par
            // dist()/dist2()/dist3()/map()). Comme chaque pixel s'execute
            // dans sa propre invocation, ce sont bien des variables
            // locales a chaque thread malgre le mot-cle "static".
            // ------------------------------------------------------------
            static float iTime;
            static float2 iResolution;

            static float4 objID;
            static float spokes, sph;

            static float3 hexN, gN1, gN2;
            static float3x3 basisHex, basisHexSm1, basisHexSm2;

            // ------------------------------------------------------------
            // Utilitaires
            // ------------------------------------------------------------
            float mod(float x, float y) { return x - y * floor(x / y); }
            float3 mod(float3 x, float y) { return x - y * floor(x / y); }
            float4 mod(float4 x, float y) { return x - y * floor(x / y); }

            // Note sur les matrices : toutes les matrices ci-dessous sont
            // ecrites avec exactement le meme ordre d'arguments que dans
            // le GLSL d'origine. GLSL stocke ses mat2/mat3 par colonnes
            // alors que HLSL stocke par lignes ; pour obtenir la MEME
            // matrice mathematique M_GLSL avec les memes valeurs saisies
            // dans le meme ordre, on utilise systematiquement
            // mul(vecteur, M_HLSL) a la place de M_GLSL * vecteur.
            float2x2 rot2(float a)
            {
                float c = cos(a), s = sin(a);
                return float2x2(c, s, -s, c); // mat2(c, s, -s, c) en GLSL
            }

            float3 rotObj(float3 p)
            {
                p.yz = mul(p.yz, rot2(iTime * 0.2 / 2.0));
                p.xz = mul(p.xz, rot2(iTime * 0.5 / 2.0));
                return p;
            }

            float noise3D(float3 p)
            {
                const float3 s = float3(113.0, 157.0, 1.0);
                float3 ip = floor(p);
                float4 h = float4(0.0, s.y, s.z, s.y + s.z) + dot(ip, s);
                p -= ip;
                p = p * p * (3.0 - 2.0 * p);
                h = lerp(frac(sin(mod(h, 6.2831589)) * 43758.5453),
                         frac(sin(mod(h + s.x, 6.2831589)) * 43758.5453), p.x);
                h.xy = lerp(h.xz, h.yw, p.y);
                float n = lerp(h.x, h.y, p.z);
                return n;
            }

            float fbm(float3 p)
            {
                return 0.5333 * noise3D(p) + 0.2667 * noise3D(p * 2.02) +
                       0.1333 * noise3D(p * 4.03) + 0.0667 * noise3D(p * 8.03);
            }

            // ------------------------------------------------------------
            // Constantes icosaedriques (valeurs decimales precalculees,
            // comme dans les commentaires du shader d'origine, pour eviter
            // toute ambiguite de repliage de constantes a la compilation).
            // ------------------------------------------------------------
            static const float A = 0.850650808352;
            static const float B = 0.525731112119;
            static const float J = 0.309016994375;
            static const float K = 0.809016994375;

            static const float3x3 R0 = float3x3(0.5, -K, J,  K, J, -0.5,  J, 0.5, K);
            static const float3x3 R1 = float3x3(K, J, -0.5,  J, 0.5, K,  0.5, -K, J);
            static const float3x3 R2 = float3x3(-J, -0.5, K,  0.5, -K, -J,  K, J, 0.5);
            static const float3x3 R4 = float3x3(
                0.587785252292, -K, 0.0,
                -0.425325404176, -J, 0.850650808352,
                0.688190960236, 0.5, 0.525731112119);

            static const float3 v0 = float3(0.0, A, B);
            static const float3 v1 = float3(B, 0.0, A);
            static const float3 v2 = float3(-B, 0.0, A);
            static const float3 midPt = (float3(0.0, A, B) + float3(0.0, 0.0, A) * 2.0) / 3.0;

            float3x3 basisMat(float3 n)
            {
                float a = 1.0 / (1.0 + n.z);
                float b = -n.x * n.y * a;
                return float3x3(
                    1.0 - n.x * n.x * a, b, n.x,
                    b, 1.0 - n.y * n.y * a, n.y,
                    -n.x, -n.y, n.z);
            }

            float3 opIcosahedronWithPolarity(float3 p)
            {
                float3 pol = sign(p);
                p = mul(abs(p), R0); pol *= sign(p);
                p = mul(abs(p), R1); pol *= sign(p);
                p = mul(abs(p), R2); pol *= sign(p);
                float3 ret = abs(p);
                return ret * float3(pol.x * pol.y * pol.z, 1.0, 1.0);
            }

            // ------------------------------------------------------------
            // Les trois types de pignons (pentagonal / grand hexagonal /
            // petit hexagonal). Traduction directe, y compris les
            // "raccourcis" numeriques du shader d'origine.
            // ------------------------------------------------------------
            float dist(float3 p, float r1, float r2)
            {
                p.xy = mul(p.xy, rot2(mod(iTime / 1.5, 6.2831 / 5.0) + 3.14159265));
                float a = atan2(p.y, abs(p.x));
                r1 *= (0.9 + p.z);

                float3 q = p;
                float ia = floor(a / 6.2831 * 15.0) + 0.5;
                q.xy = mul(q.xy, rot2(ia * 6.2831 / 15.0));
                q.x += r1;
                q = abs(q);
                float spike = lerp(max(q.x - 0.05, q.y - 0.02), length(q.xy * float2(0.7, 1.0)) - 0.025, 0.5);
                float d2 = max(spike, q.z - r2);

                q = p;
                ia = floor(a / 6.2831 * 5.0) + 0.5;
                q.xy = mul(q.xy, rot2(ia * 6.2831 / 5.0));
                sph = max(max(abs(q.x), abs(q.y)) - 0.0225, abs(q.z - 0.07) - 0.06);
                q = abs(q + float3(r1 / 2.0, 0.0, -0.07));
                spokes = max(q.x - r1 / 2.0 + 0.02, max(max(q.y - 0.05, q.z - 0.015), (q.y + q.z) * 0.7071 - 0.025));

                p = abs(p);
                float d = length(p.xy);
                float di = abs(d - r1 + 0.1 / 2.0) - 0.05 / 2.0;
                d = abs(d - r1 + 0.075 / 2.0) - 0.075 / 2.0;
                d = min(max(d, p.z - r2), max(di, p.z - r2 - 0.01));

                return min(d, d2);
            }

            float dist2(float3 p, float r1, float r2)
            {
                p.xy = mul(p.xy, rot2(mod(iTime / 1.8, 6.2831 / 6.0) + 6.2831 / 6.0));
                float a = atan2(p.y, abs(p.x));

                p.z = -p.z;
                r1 *= (0.9 + p.z);

                float3 q = p;
                float ia = floor(a / 6.2831 * 18.0) + 0.5;
                q.xy = mul(q.xy, rot2(ia * 6.2831 / 18.0));
                q.x += r1;
                q = abs(q);
                float spike = lerp(max(q.x - 0.05, q.y - 0.02), length(q.xy * float2(0.7, 1.0)) - 0.025, 0.5);
                float d2 = max(spike, q.z - r2);

                q = p;
                ia = floor(a / 6.2831 * 6.0) + 0.5;
                q.xy = mul(q.xy, rot2(ia * 6.2831 / 6.0));
                sph = max(max(abs(q.x), abs(q.y)) - 0.0275, abs(q.z - 0.07) - 0.06);
                q = abs(q + float3(r1 / 2.0, 0.0, -0.07));
                spokes = max(q.x - r1 / 2.0 + 0.02, max(max(q.y - 0.05, q.z - 0.015), (q.y + q.z) * 0.7071 - 0.03));

                p = abs(p);
                float d = length(p.xy);
                float di = abs(d - r1 + 0.1 / 2.0) - 0.05 / 2.0;
                d = abs(d - r1 + 0.075 / 2.0) - 0.075 / 2.0;
                d = min(max(d, p.z - r2), max(di, p.z - r2 - 0.0115));

                return min(d, d2);
            }

            float dist3(float3 p, float r1, float r2)
            {
                float dir = p.x < 0.0 ? -1.0 : 1.0;

                p.x = abs(p.x);
                float3 q2 = p;
                p.xy = mul(p.xy, rot2(mod(iTime / 1.2 * dir + 3.14159 / 12.0, 6.2831 / 12.0) + 5.0 * 6.2831 / 12.0));

                float a = atan2(p.y, abs(p.x));
                r1 *= (0.9 + p.z);

                float3 q = p;
                float ia = floor(a / 6.2831 * 12.0) + 0.5;
                q.xy = mul(q.xy, rot2(ia * 6.2831 / 12.0));
                q.x += r1;
                q.xy = abs(q.xy);
                float spike = lerp(max(q.x - 0.05, q.y - 0.02), length(q.xy * float2(0.7, 1.0)) - 0.025, 0.5);
                float d2 = max(spike, abs(q.z) - r2);

                q = q2;
                q.xy = mul(q.xy, rot2(mod(iTime / 1.2 * dir, 6.2831 / 6.0) + 2.0 * 6.2831 / 6.0));
                a = atan2(q.y, abs(q.x));
                ia = floor(a / 6.2831 * 6.0) + 0.5;
                q.xy = mul(q.xy, rot2(ia * 6.2831 / 6.0));
                sph = max(max(abs(q.x), abs(q.y)) - 0.02, abs(q.z - 0.07) - 0.06);
                q = abs(q + float3(r1 / 2.0, 0.0, -0.07));
                spokes = max(q.x - r1 / 2.0 + 0.02, max(max(q.y - 0.04, q.z - 0.015), (q.y + q.z) * 0.7071 - 0.02));

                p = abs(p);
                float d = length(p.xy);
                float di = abs(d - r1 + 0.1 / 2.0) - 0.05 / 2.0;
                d = abs(d - r1 + 0.075 / 2.0) - 0.075 / 2.0;
                d = min(max(d, p.z - r2), max(di, p.z - r2 - 0.007));

                return min(d, d2);
            }

            float map(float3 p)
            {
                float pln = -p.z + 6.0;

                p = rotObj(p);
                float3 oP = p;

                float d = 1e5, d2 = 1e5, d3 = 1e5;

                float3 hexFace = opIcosahedronWithPolarity(p);
                float3 pentFace = mul(hexFace, R4);

                float3 p1 = (pentFace - float3(0.0, 0.0, 1.0));
                d3 = min(d3, dist(p1, 0.185, 0.1));
                d = min(d, spokes);
                d3 = min(d3, sph);

                p1 = mul((hexFace - midPt * 1.2425), basisHex);
                d3 = min(d3, dist2(p1, 0.25, 0.1));
                d = min(d, spokes);
                d3 = min(d3, sph);

                p1 = mul((hexFace - lerp(v0, v2, 0.333) * 1.1547), basisHexSm1);
                d2 = min(d2, dist3(p1, 0.16, 0.1));
                d = min(d, spokes);
                d3 = min(d3, sph);

                p1 = mul((hexFace - lerp(v0, v1, 0.333) * 1.1547), basisHexSm2);
                d2 = min(d2, dist3(p1, 0.16, 0.1));
                d = min(d, spokes);
                d3 = min(d3, sph);

                float mainSph = length(oP);
                d = max(d, mainSph - 1.0825);
                d2 = max(d2, mainSph - 1.116);
                d3 = max(d3, mainSph - 1.118);

                objID = float4(d, d2, d3, pln);

                return min(min(d, d2), min(d3, pln));
            }

            float3 calcNormal(float3 p, inout float edge, inout float crv, float t)
            {
                float eps = 3.0 / lerp(450.0, min(850.0, iResolution.y), 0.35);

                float d = map(p);

                float3 e = float3(eps, 0.0, 0.0);
                float3 da = float3(-2.0 * d, -2.0 * d, -2.0 * d);

                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    [unroll]
                    for (int j = 0; j < 2; j++)
                        da[i] += map(p + e * float(1 - 2 * j));
                    e = e.zxy;
                }
                da = abs(da);

                edge = da.x + da.y + da.z;
                edge = smoothstep(0.0, 1.0, sqrt(edge / e.x * 2.0));

                float3 n = float3(0.0, 0.0, 0.0);
                [unroll]
                for (int k = 0; k < 4; k++)
                {
                    float3 ee = 0.57735 * (2.0 * float3((((k + 3) >> 1) & 1), ((k >> 1) & 1), (k & 1)) - 1.0);
                    n += ee * map(p + 0.001 * ee);
                }

                return normalize(n);
            }

            float trace(float3 ro, float3 rd)
            {
                float t = 0.0, d;
                [loop]
                for (int i = 0; i < 64; i++)
                {
                    d = map(ro + rd * t);
                    if (abs(d) < 0.001 * (1.0 + t * 0.05) || t > FAR) break;
                    t += d;
                }
                return min(t, FAR);
            }

            float calcAO(float3 p, float3 n)
            {
                float sca = 4.0, occ = 0.0;
                [unroll]
                for (int i = 1; i < 6; i++)
                {
                    float hr = float(i) * 0.125 / 5.0;
                    float dd = map(p + hr * n);
                    occ += (hr - dd) * sca;
                    sca *= 0.75;
                }
                return clamp(1.0 - occ, 0.0, 1.0);
            }

            float softShadow(float3 ro, float3 lp, float3 n, float k)
            {
                const int maxIterationsShad = 32;
                ro += n * 0.0015;
                float3 rd = lp - ro;

                float shade = 1.0;
                float t = 0.0;
                float end = max(length(rd), 0.0001);
                rd /= end;

                [loop]
                for (int i = 0; i < maxIterationsShad; i++)
                {
                    float d = map(ro + rd * t);
                    shade = min(shade, k * d / max(t, 1e-4));
                    t += clamp(d, 0.01, 0.25);
                    if (d < 0.0 || t > end) break;
                }
                return max(shade, 0.0);
            }

            float3 envMap(float3 p)
            {
                p *= 3.0;
                float n3D2 = noise3D(p * 3.0);
                float c = noise3D(p) * 0.57 + noise3D(p * 2.0) * 0.28 + noise3D(p * 4.0) * 0.15;
                c = smoothstep(0.25, 1.0, c);
                p = float3(c, c * c, c * c * c);
                return lerp(p, p.zyx, n3D2 * 0.5 + 0.5);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                iTime = _Time.y;
                iResolution = _ScreenParams.xy;

                // Repere ces matrices une fois par pixel (bon marche par
                // rapport au reste du raymarching).
                hexN = normalize(cross((midPt - v0), (midPt - v1)));
                gN1 = normalize(lerp(v0, v2, 0.333));
                gN2 = normalize(lerp(v0, v1, 0.333));
                basisHex = basisMat(hexN);
                basisHexSm1 = basisMat(gN1);
                basisHexSm2 = basisMat(gN2);

                float2 fragCoord = IN.positionHCS.xy;
                float2 p = (fragCoord - iResolution.xy * 0.5) / min(850.0, iResolution.y);

                float3 rd = normalize(float3(p, 1.0));
                float3 ro = float3(0.0, 0.0, -2.75);
                float3 lp = ro + float3(-1.0, 2.0, -1.0);

                rd.xy = mul(rd.xy, rot2(sin(iTime / 8.0) * 0.2));
                rd.xz = mul(rd.xz, rot2(sin(iTime / 4.0) * 0.1));

                float t = trace(ro, rd);

                float svObjID = objID.x < objID.y && objID.x < objID.z && objID.x < objID.w ? 0.0 :
                                 objID.y < objID.z && objID.y < objID.w ? 1.0 :
                                 objID.z < objID.w ? 2.0 : 3.0;

                float3 sceneCol = float3(0.0, 0.0, 0.0);

                if (t < FAR)
                {
                    float3 pos = ro + rd * t;
                    float edge = 0.0, crv = 1.0;
                    float3 nor = calcNormal(pos, edge, crv, t);

                    float3 li = lp - pos;
                    float lDist = max(length(li), 0.001);
                    li /= lDist;

                    float atten = 1.0 / (1.0 + lDist * 0.05 + lDist * lDist * 0.025);

                    float shd = softShadow(pos, lp, nor, 8.0);
                    float ao = calcAO(pos, nor);

                    float diff = max(dot(li, nor), 0.0);
                    float spec = pow(max(dot(reflect(-li, nor), -rd), 0.0), 16.0);
                    diff = pow(diff, 4.0) * 2.0;

                    float3 col;
                    if (_ColorScheme < 0.5) col = float3(0.6, 0.6, 0.6);
                    else if (_ColorScheme < 1.5) col = float3(0.1, 0.1, 0.1);
                    else col = float3(0.9, 0.2, 0.4);

                    if (svObjID == 1.0)
                    {
                        if (_ColorScheme < 0.5) col = float3(1.0, 0.5, 0.2) * 0.7;
                        else col = float3(0.6, 0.6, 0.6);
                    }
                    if (svObjID == 2.0)
                    {
                        if (_ColorScheme < 0.5) col = float3(1.0, 0.65, 0.3) * 0.7;
                        else col = float3(0.6, 0.6, 0.6);
                    }
                    if (svObjID == 3.0)
                    {
                        if (_ColorScheme < 0.5) col = float3(1.0, 0.7, 0.4) * 0.045;
                        else if (_ColorScheme < 1.5) col = float3(0.7, 0.7, 0.7) * 0.045;
                        else col = float3(0.6, 0.7, 1.0) * 0.045;
                    }

                    float txSz = 1.0;
                    float3 txPos = pos;
                    if (svObjID == 3.0) txSz /= 4.0;
                    else txPos = rotObj(txPos);
                    col *= fbm(txPos * 64.0 * txSz) * 0.75 + 0.5;

                    sceneCol = col * (diff * shd + 0.25);

                    if (svObjID == 3.0) sceneCol += col * float3(1.0, 0.6, 0.2).zyx * spec * shd * 0.25;
                    else sceneCol += col * float3(0.5, 0.75, 1.0) * spec * shd * 2.0;

                    float envF = 4.0;
                    if (svObjID == 0.0) envF = 8.0;
                    sceneCol += sceneCol * envMap(reflect(rd, nor)) * envF;

                    sceneCol *= 1.0 - edge * 0.8;
                    sceneCol *= atten * ao;
                }

                return float4(sqrt(clamp(sceneCol, 0.0, 1.0)), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
