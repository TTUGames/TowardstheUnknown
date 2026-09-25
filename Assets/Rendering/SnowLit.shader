// Lit surface covered with snow where it faces the sky: the snow settles on the faces turned upwards, with a noisy edge,
// and stays off the surfaces sheltered by something above them (the height map of SnowCover, seen from the top).
// The snow glints with sparkles and a soft rim. Used by the rocks, the props and the tiles of the rooms
Shader "Towards the Unknown/Snow Lit"
{
    Properties
    {
        [Header(Surface)]
        _BaseMap ("Base Map", 2D) = "white" {}
        [HDR] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2

        [Header(Snow)]
        [HDR] _SnowColor ("Snow Color", Color) = (0.92, 0.96, 1.05, 1)
        _SnowAmount ("Slope: how far from facing up it still settles", Range(0, 1)) = 0.35
        _SnowCoverage ("Coverage: how much of those faces it covers", Range(0, 1)) = 0.9
        _SnowOpacity ("Opacity", Range(0, 1)) = 1
        _SnowSharpness ("Edge Sharpness", Range(1, 30)) = 8
        _SnowNoiseScale ("Edge Noise Scale", Float) = 1.6
        _SnowSmoothness ("Snow Smoothness", Range(0, 1)) = 0.35
        [HDR] _SnowShadowTint ("Snow Shadow Tint, the blue of its unlit side", Color) = (0.55, 0.68, 0.95, 1)

        [Header(Glints)]
        _SparkleStrength ("Sparkle Strength", Range(0, 4)) = 1.2
        _SparkleScale ("Sparkle Density", Float) = 38
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Smoothness;
            half4 _SnowColor;
            half _SnowAmount;
            half _SnowCoverage;
            half _SnowOpacity;
            half _SnowSharpness;
            float _SnowNoiseScale;
            half _SnowSmoothness;
            half4 _SnowShadowTint;
            half _SparkleStrength;
            float _SparkleScale;
            half _RimStrength;
        CBUFFER_END

        // Set by SnowCover: the highest point of the room above each spot, and the area it covers
        TEXTURE2D(_SnowHeightMap);
        float4 _SnowHeightBounds; // x min, z min, size, 1 when the map is set
        float _SnowHeightBias;

        float Hash(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }

        float ValueNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x),
                             lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                        lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                             lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
        }

        // 1 where the sky is open above the point, 0 under an overhang
        half SkyExposure(float3 positionWS)
        {
            if (_SnowHeightBounds.w < 0.5) return 1;
            float2 uv = (positionWS.xz - _SnowHeightBounds.xy) / _SnowHeightBounds.z;
            if (any(uv < 0) || any(uv > 1)) return 1;
            float highest = SAMPLE_TEXTURE2D_LOD(_SnowHeightMap, sampler_LinearClamp, uv, 0).r;
            // A float map may hold garbage where nothing was drawn on some platforms: count it as open sky
            if (!(highest > -500 && highest < 500)) return 1;
            return smoothstep(-0.35, 0.0, positionWS.y - highest + _SnowHeightBias);
        }

        // How much snow covers the point, from 0 to 1: the faces turned up enough, in noisy patches whose share is the coverage
        half SnowCoverage(float3 positionWS, float3 normalWS)
        {
            half up = saturate(normalWS.y);
            half facing = smoothstep(1 - _SnowAmount, 1 - _SnowAmount + 0.12, up);
            float noise = ValueNoise(positionWS * _SnowNoiseScale) * 0.6 + ValueNoise(positionWS * _SnowNoiseScale * 2.7) * 0.28 + ValueNoise(positionWS * _SnowNoiseScale * 7.3) * 0.12;
            float threshold = 1 - _SnowCoverage;
            float softness = 0.5 / _SnowSharpness;
            half patches = _SnowCoverage >= 0.999 ? 1 : smoothstep(threshold - softness, threshold + softness, noise);
            return facing * patches * SkyExposure(positionWS) * _SnowOpacity;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half snow = SnowCoverage(input.positionWS, normalWS);

                half3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = lerp(baseColor, _SnowColor.rgb, snow);
                surface.smoothness = lerp(_Smoothness, _SnowSmoothness, snow);
                surface.occlusion = 1;
                surface.alpha = 1;
                surface.normalTS = half3(0, 0, 1);

                // Snow is never black: its unlit side takes the cold blue of the sky it scatters
                half3 scatter = _SnowShadowTint.rgb * 0.18 * snow;
                // Sparkles: tiny crystals catching the light, twinkling as the view moves
                float3 cell = floor(input.positionWS * _SparkleScale);
                half sparkle = step(0.992, Hash(cell)) * pow(saturate(dot(normalWS, viewWS)), 2);
                sparkle *= 0.5 + 0.5 * sin(_Time.y * 3 + Hash(cell + 7) * 20);
                half rim = pow(1 - saturate(dot(normalWS, viewWS)), 4) * _RimStrength;
                surface.emission = scatter + (sparkle * _SparkleStrength + rim) * snow * _SnowColor.rgb;

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirection = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirection = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
