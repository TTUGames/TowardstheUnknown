// The water of the cave, drawn on a plain cube scaled to the pool: nothing enters it, it is scenery. The view ray is
// followed through the box to the ground under it (the depth texture) or out of its bottom, and the water absorbs the
// light along the way, red first (Beer-Lambert), so that shallow water shows its bottom and deep water sinks into
// _DeepColor. The ground is seen refracted, lit by caustics where a light reaches it, and a sharp line outlines
// everything crossing the surface. The surface breathes (Water.hlsl's WaterWave, shared with the floating plants),
// ripples in its normals (faster where a gust of Wind passes, rings where snowflakes and drops touch it), mirrors the
// room (WaterReflection's planar reflection) and catches the lights. The sides show the water in cross section.
// The camera is orthographic: the depth is rebuilt in world space, never with the perspective helpers
Shader "Towards the Unknown/Water"
{
    Properties
    {
        [Header(Color)]
        _Absorption ("Absorption per meter (rgb): the most absorbed channel fades first", Vector) = (3.2, 1.4, 0.9, 0)
        [HDR] _DeepColor ("Deep Color: what the water turns to where it is deep", Color) = (0.01, 0.045, 0.085, 1)
        _Turbidity ("Turbidity: how much the lights brighten the deep color", Range(0, 2)) = 0.15
        [HDR] _ScatterEmission ("Scatter Emission: a glow of the water itself", Color) = (0, 0, 0, 1)
        _FloorDepth ("Floor Depth: the water below the box, where the ground lies deeper", Float) = 4

        [Header(Surface)]
        [Normal][NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Size, in meters", Float) = 3
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.35
        _NormalSpeed ("Normal Drift, in meters per second", Float) = 0.04
        _Flow ("Flow: the current, xz in meters per second (0 for still water)", Vector) = (0, 0, 0, 0)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.92
        _Specular ("Specular Strength", Range(0, 8)) = 1.5
        _SpecularClamp ("Specular Clamp, against the bloom", Float) = 6
        _Refraction ("Refraction, in screen units", Range(0, 0.1)) = 0.02

        [Header(Level)]
        _WaveSpeed ("Breath Speed (Water.hlsl's WaterWave)", Float) = 0.35
        _WaveFrequency ("Breath Frequency: 0 rises everywhere together", Float) = 0
        _WaveScale ("Breath Height, in meters", Float) = 0.015

        [Header(Edge)]
        [HDR] _EdgeColor ("Edge Color", Color) = (0.75, 0.9, 1, 1)
        _EdgeWidth ("Edge Width, in meters of water", Float) = 0.07
        _EdgeNoiseScale ("Edge Noise Size", Float) = 2.2
        _EdgeSecond ("Second Line", Range(0, 1)) = 0.6

        [Header(Reflection)]
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.45
        _ReflectionDistortion ("Reflection Distortion", Range(0, 0.1)) = 0.03
        _ReflectionBlur ("Reflection Blur with the ripples, in mips", Range(0, 6)) = 2.5

        [Header(Caustics)]
        [HDR] _CausticsColor ("Caustics Color", Color) = (0.8, 0.95, 1, 1)
        _Caustics ("Caustics Strength", Range(0, 4)) = 1
        _CausticsScale ("Caustics Size, cells per meter", Float) = 1.4
        _CausticsSpeed ("Caustics Speed", Float) = 0.6
        _CausticsFade ("Caustics Fade per meter of depth", Float) = 0.8

        [Header(Wind and Ripples)]
        _GustRipple ("Gust Ripples: how much a gust roughens the water", Range(0, 6)) = 2.5
        _SplashGlow ("Splash Glow: how much the snowflakes' rings catch the light", Range(0, 2)) = 1
        _GustRings ("Gust Rings: share of the water ringing as a gust passes", Range(0, 1)) = 0.5
        _DepthBlur ("Depth Blur: how much the ground blurs as it lies deeper", Range(0, 4)) = 1.5
        _Glints ("Glints: sparkles where a light strikes the ripples", Range(0, 4)) = 1
        [HDR] _GlintColor ("Glint Color", Color) = (1, 0.97, 0.9, 1)
        _RippleStrength ("Ring Strength", Range(0, 4)) = 1
    }

    SubShader
    {
        // Before the rift's air (Transparent-50), which mists over it as over the ground around
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-60" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Water"
            Tags { "LightMode" = "UniversalForward" }
            // The water composes the whole color itself, from the opaque texture: it replaces what is behind it, and hides
            // the transparent effects drawn later that lie under the surface
            Blend Off
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Wind.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Absorption;
                half4 _DeepColor;
                half _Turbidity;
                half4 _ScatterEmission;
                float _FloorDepth;
                float _NormalScale;
                half _NormalStrength;
                float _NormalSpeed;
                float4 _Flow;
                half _Smoothness;
                half _Specular;
                half _SpecularClamp;
                half _Refraction;
                float _WaveSpeed;
                float _WaveFrequency;
                float _WaveScale;
                half4 _EdgeColor;
                float _EdgeWidth;
                float _EdgeNoiseScale;
                half _EdgeSecond;
                half _ReflectionStrength;
                half _ReflectionDistortion;
                half _ReflectionBlur;
                half4 _CausticsColor;
                half _Caustics;
                float _CausticsScale;
                float _CausticsSpeed;
                float _CausticsFade;
                half _GustRipple;
                half _SplashGlow;
                half _GustRings;
                half _DepthBlur;
                half _Glints;
                half4 _GlintColor;
                half _RippleStrength;
            CBUFFER_END

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            // Set by WaterReflection: the room mirrored under its water plane
            TEXTURE2D(_WaterReflectionTex);
            SAMPLER(sampler_WaterReflectionTex);
            // x the height of the mirror plane, y 1 while the reflection is drawn
            float4 _WaterReflectionPlane;

            // Set by WaterRipples: the drops touching the water, xyz where, w when (_Time.y)
            #define WATER_MAX_RIPPLES 16
            float4 _WaterRipples[WATER_MAX_RIPPLES];
            int _WaterRippleCount;
            // Set by WaterRipples: the snowflakes touching the water (WaterSplash), xyz where, w when
            #define WATER_MAX_SPLASHES 48
            float4 _WaterSplashes[WATER_MAX_SPLASHES];
            int _WaterSplashCount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                // 1 on the top face, 0 on the sides
                half top : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 WaveSettings()
            {
                return float3(_WaveSpeed, _WaveFrequency, _WaveScale);
            }

            // The height of the surface over a point: the top of the box, breathing
            float SurfaceHeight(float2 positionXZ)
            {
                float3 slope;
                return TransformObjectToWorld(float3(0, 0.5, 0)).y + WaterWave(positionXZ, WaveSettings(), slope);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                // The top vertices, of the top face and of the sides, rise and fall with the surface
                float3 slope;
                positionWS.y += step(0.49, input.positionOS.y) * WaterWave(positionWS.xz, WaveSettings(), slope);
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.top = step(0.5, input.normalOS.y);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float WaterHash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float2 WaterHash2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
            }

            float WaterNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3 - 2 * f);
                return lerp(lerp(WaterHash(i), WaterHash(i + float2(1, 0)), f.x),
                            lerp(WaterHash(i + float2(0, 1)), WaterHash(i + float2(1, 1)), f.x), f.y);
            }

            // The screen uv of a world point, as the scene textures are sampled
            float2 ScreenUV(float3 positionWS)
            {
                float4 positionCS = TransformWorldToHClip(positionWS);
                float4 screen = ComputeScreenPos(positionCS);
                return screen.xy / screen.w;
            }

            // The ground seen at a screen uv, in world space
            float3 SceneAt(float2 uv)
            {
                return ComputeWorldSpacePosition(uv, SampleSceneDepth(uv), UNITY_MATRIX_I_VP);
            }

            // How far the ray from positionWS along direction (normalized) runs inside the box before leaving it
            float BoxExit(float3 positionWS, float3 direction)
            {
                float3 originOS = TransformWorldToObject(positionWS);
                // Not normalized: the distances along it stay in world units
                float3 directionOS = TransformWorldToObjectDir(direction, false);
                float3 inverse = 1.0 / (directionOS + (abs(directionOS) < 1e-5) * 1e-5);
                float3 t0 = (-0.5 - originOS) * inverse;
                float3 t1 = (0.5 - originOS) * inverse;
                float3 tMax = max(t0, t1);
                return max(min(min(tMax.x, tMax.y), tMax.z), 0);
            }

            // A ring spreading from a point: the push it gives the normal (xz), from the slope of its crest
            float2 Ring(float2 positionXZ, float2 center, float age, float speed, float life)
            {
                if (age < 0 || age > life) return 0;
                float2 away = positionXZ - center;
                float distance = length(away);
                float front = age * speed;
                float fade = 1 - age / life;
                float wave = cos((distance - front) * 38) * exp(-(distance - front) * (distance - front) * 900);
                return away / max(distance, 1e-4) * wave * fade * fade;
            }

            // The small rings where the snowflakes touched the water (WaterSplash): xy the push, z the crest, for the glow
            float3 SplashRings(float2 positionXZ)
            {
                float3 splash = 0;
                for (int i = 0; i < _WaterSplashCount; i++)
                {
                    float4 flake = _WaterSplashes[i];
                    float age = _Time.y - flake.w;
                    if (age < 0 || age > 0.9) continue;
                    float2 away = positionXZ - flake.xz;
                    float distance = length(away);
                    // A cheap reject: the ring never grows past 0.2 m
                    if (distance > 0.25) continue;
                    float crest = distance - age * 0.2;
                    float fade = 1 - age / 0.9;
                    float wave = exp(-crest * crest * 4000) * fade * fade;
                    splash.xy += away / max(distance, 1e-4) * cos(crest * 60) * wave;
                    splash.z += wave;
                }
                return splash;
            }

            // The rings of the drops (WaterRipples): three crests following each other
            float2 DropRings(float2 positionXZ)
            {
                float2 push = 0;
                for (int i = 0; i < _WaterRippleCount; i++)
                {
                    float4 ripple = _WaterRipples[i];
                    float age = _Time.y - ripple.w;
                    for (int crest = 0; crest < 3; crest++)
                        push += Ring(positionXZ, ripple.xz, age - crest * 0.18, 0.35, 1.8) * (1 - crest * 0.25);
                }
                return push;
            }

            // The rings a gust scatters over the water as its front passes: in cells of 0.8 m, each ringing now and then,
            // more of them where the gust is strong
            float2 GustRings(float2 positionXZ, half gust)
            {
                if (gust <= 0.05 || _GustRings <= 0) return 0;
                float2 p = positionXZ / 0.8;
                float2 cell = floor(p);
                const float period = 1.1;
                float2 seed = WaterHash2(cell + 5.3);
                float cycle = floor(_Time.y / period + seed.x);
                float age = frac(_Time.y / period + seed.x) * period;
                if (WaterHash(cell + cycle * 13.7) > gust * _GustRings) return 0;
                float2 center = (cell + 0.3 + 0.4 * WaterHash2(cell + cycle * 2.9)) * 0.8;
                return Ring(positionXZ, center, age, 0.3, 0.9) * 0.8;
            }

            // Two layers of the normal map drifting apart, a third, finer, running with the wind where a gust passes,
            // and the rings; in world space. gust: 0 to 1
            float3 SurfaceNormal(float3 positionWS, half gust, float2 splash)
            {
                float2 uv = (positionWS.xz - _Flow.xz * _Time.y) / _NormalScale;
                float2 wind = _WindDirection.xz * _WindDirection.w;
                float time = _Time.y;
                float drift = _NormalSpeed / _NormalScale;
                float3 a = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv + (float2(0.7, 0.4) + wind * 0.6) * drift * time));
                float2 rotated = float2(uv.x * 0.8 - uv.y * 0.6, uv.x * 0.6 + uv.y * 0.8) * 1.37;
                float3 b = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, rotated + (float2(-0.5, 0.6) + wind * 0.4) * drift * time));
                float3 c = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv * 3.1 + wind * drift * time * 6));
                half strength = _NormalStrength * (1 + gust * _GustRipple);
                float2 slope = (a.xy + b.xy) * strength + c.xy * gust * _NormalStrength * _GustRipple * 0.5;
                slope += (splash * 0.6 + DropRings(positionWS.xz) + GustRings(positionWS.xz, gust)) * _RippleStrength * 0.35;
                // Tangent space of a face turned up: x along world x, y along world z
                return normalize(float3(slope.x, 1, slope.y));
            }

            // Caustics: the light gathered by the ripples, bright lines of two cell patterns drifting over each other
            half CausticsPattern(float2 positionXZ)
            {
                half lines = 0;
                for (int layer = 0; layer < 2; layer++)
                {
                    float2 p = positionXZ * _CausticsScale * (layer == 0 ? 1 : 1.43) + layer * 7.1;
                    float2 cell = floor(p);
                    float2 f = frac(p);
                    float first = 8, second = 8;
                    for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbour = float2(x, y);
                        float2 seed = WaterHash2(cell + neighbour);
                        float2 cellPoint = neighbour + 0.5 + 0.4 * sin(_Time.y * _CausticsSpeed * (layer == 0 ? 1 : -0.8) + seed * 6.2831);
                        float distance = length(cellPoint - f);
                        if (distance < first) { second = first; first = distance; }
                        else if (distance < second) second = distance;
                    }
                    lines += 1 - smoothstep(0, 0.14, second - first);
                }
                return saturate(lines * 0.6) * saturate(lines * 0.6);
            }

            // The light reaching a point: the main light and the room's lights, with their shadows and cookies
            half3 LightAt(float3 positionWS, float3 normalWS)
            {
                Light main = GetMainLight(TransformWorldToShadowCoord(positionWS));
                half3 light = main.color * main.distanceAttenuation * main.shadowAttenuation * saturate(dot(normalWS, main.direction));
                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint i = 0; i < count; i++)
                {
                    Light additional = GetAdditionalLight(i, positionWS, half4(1, 1, 1, 1));
                    light += additional.color * additional.distanceAttenuation * additional.shadowAttenuation * saturate(dot(normalWS, additional.direction));
                }
                #endif
                return light;
            }

            // The glints of the lights on the ripples
            half3 SpecularAt(float3 positionWS, float3 normalWS, float3 viewWS)
            {
                BRDFData brdf;
                half alpha = 1;
                InitializeBRDFData(half3(0, 0, 0), 0, half3(0.04, 0.04, 0.04) * _Specular, _Smoothness, alpha, brdf);
                Light main = GetMainLight(TransformWorldToShadowCoord(positionWS));
                half3 specular = DirectBRDFSpecular(brdf, normalWS, main.direction, viewWS) * brdf.specular
                    * main.color * main.distanceAttenuation * main.shadowAttenuation * saturate(dot(normalWS, main.direction));
                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint i = 0; i < count; i++)
                {
                    Light light = GetAdditionalLight(i, positionWS, half4(1, 1, 1, 1));
                    specular += DirectBRDFSpecular(brdf, normalWS, light.direction, viewWS) * brdf.specular
                        * light.color * light.distanceAttenuation * light.shadowAttenuation * saturate(dot(normalWS, light.direction));
                }
                #endif
                return min(specular, _SpecularClamp);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = input.positionWS;
                bool top = input.top > 0.5;
                float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(positionWS);
                float surfaceY = top ? positionWS.y : SurfaceHeight(positionWS.xz);
                half3 ambient = SampleSH(float3(0, 1, 0));

                half gust = saturate(WindGustAt(positionWS.xz) * _WindDirection.w);
                float3 splash = top ? SplashRings(positionWS.xz) : 0;
                float3 normalWS = top ? SurfaceNormal(positionWS, gust, splash.xy) : float3(0, 1, 0);

                // The ground behind, refracted by the ripples; a ground in front of the surface (above the water) is not
                // refracted, or the rocks around would bleed into it
                float3 ground = SceneAt(uv);
                float2 refractedUV = uv + normalWS.xz * _Refraction * top * saturate((surfaceY - ground.y) * 2);
                float3 refractedGround = SceneAt(refractedUV);
                if (dot(refractedGround - positionWS, -viewWS) < 0)
                {
                    refractedUV = uv;
                    refractedGround = ground;
                }
                half3 behind = SampleSceneColor(refractedUV);
                // The deeper the ground, the blurrier: four more taps around, each kept only if it lies under the water
                float blurRadius = _DepthBlur * saturate((surfaceY - refractedGround.y) / 1.5) * 0.004 * top;
                if (blurRadius > 0.0002)
                {
                    half3 sum = behind;
                    half taps = 1;
                    const float2 offsets[4] = { float2(1, 0.3), float2(-0.3, 1), float2(-1, -0.3), float2(0.3, -1) };
                    for (int tap = 0; tap < 4; tap++)
                    {
                        float2 tapUV = refractedUV + offsets[tap] * blurRadius * float2(_ScreenParams.y / _ScreenParams.x, 1);
                        if (SceneAt(tapUV).y < surfaceY)
                        {
                            sum += SAMPLE_TEXTURE2D_X_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UnityStereoTransformScreenSpaceTex(tapUV), 0).rgb;
                            taps++;
                        }
                    }
                    behind = sum / taps;
                }

                // The path of the view through the water: to the ground, or out of the box and down to a floor below it
                float3 into = -viewWS;
                float toGround = max(dot(refractedGround - positionWS, into), 0);
                float exit = BoxExit(positionWS, into);
                float path = toGround <= exit ? toGround : exit + _FloorDepth;
                float groundDepth = max(surfaceY - refractedGround.y, 0);

                // Caustics on the ground, where a light reaches it
                if (_Caustics > 0 && toGround <= exit)
                {
                    half caustics = CausticsPattern(refractedGround.xz - _Flow.xz * _Time.y * 0.8) * exp(-_CausticsFade * groundDepth) * _Caustics;
                    behind += _CausticsColor.rgb * caustics * LightAt(refractedGround, float3(0, 1, 0));
                }

                // Absorption: the ground fades into the deep color, red first
                half3 transmittance = exp(-_Absorption.rgb * path);
                half3 surfaceLight = LightAt(positionWS, float3(0, 1, 0));
                half3 deep = _DeepColor.rgb * (1 + (surfaceLight + ambient) * _Turbidity) + _ScatterEmission.rgb;
                half3 color = behind * transmittance + deep * (1 - transmittance);

                // The line around what crosses the surface: where the ground rises close to it, cut by a slow noise
                float closeness = min(max(surfaceY - ground.y, 0), max(dot(ground - positionWS, into), 0) * 0.7);
                float2 flowing = positionWS.xz - _Flow.xz * _Time.y;
                float noise = WaterNoise(flowing * _EdgeNoiseScale + _Time.y * 0.15) * 0.65 + WaterNoise(flowing * _EdgeNoiseScale * 2.9 - _Time.y * 0.1) * 0.35;
                float width = _EdgeWidth * (0.75 + 0.5 * noise);
                half edge = 1 - smoothstep(width * 0.85, width, closeness);
                float secondAt = _EdgeWidth * (1.7 + 0.25 * sin(_Time.y * 0.7 + noise * 6));
                half second = (1 - smoothstep(0, _EdgeWidth * 0.12, abs(closeness - secondAt))) * step(0.45, noise) * _EdgeSecond;
                half outline = saturate(edge + second + splash.z * _SplashGlow * 0.5);
                if (!top) outline = 1 - smoothstep(0.03, 0.05, surfaceY - positionWS.y);
                half3 edgeColor = _EdgeColor.rgb * (0.35 + saturate(Luminance(surfaceLight + ambient)));

                if (top)
                {
                    // The room mirrored: a pool lower or higher than the mirror plane sees the same image shifted on screen
                    half reflection = _ReflectionStrength * _WaterReflectionPlane.y * (1 - outline);
                    if (reflection > 0)
                    {
                        float3 shifted = positionWS + float3(0, 2 * (surfaceY - _WaterReflectionPlane.x), 0);
                        float2 reflectionUV = uv - (ScreenUV(shifted) - ScreenUV(positionWS)) + normalWS.xz * _ReflectionDistortion;
                        // The mirror camera sees the mirror image flipped left to right
                        reflectionUV.x = 1 - reflectionUV.x;
                        half blur = saturate(length(normalWS.xz) / max(_NormalStrength, 0.01) - 0.3) * _ReflectionBlur * (0.4 + gust);
                        half3 mirrored = SAMPLE_TEXTURE2D_LOD(_WaterReflectionTex, sampler_WaterReflectionTex, reflectionUV, blur).rgb;
                        color = lerp(color, mirrored, reflection);
                    }
                    half3 specular = SpecularAt(positionWS, normalWS, viewWS);
                    color += specular * (1 - outline);
                    // Glints: tiny points of the ripples flashing where a light strikes them, each for a moment
                    float2 glintCell = floor(positionWS.xz * 16);
                    half glint = step(0.985, WaterHash(glintCell + floor(_Time.y * 5 + WaterHash(glintCell) * 7) * 3.1));
                    glint *= saturate(length(normalWS.xz) * 6) * saturate(Luminance(specular) * 3 + Luminance(surfaceLight) * 0.15);
                    color += _GlintColor.rgb * glint * _Glints * (1 - outline);
                }

                color = lerp(color, edgeColor, outline);
                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
