// The vegetation: a lit surface that moves in the wind (Wind.hlsl), with the glows of the rift's plants, each behind its
// keyword. Emission: a map times an HDR color (the cave mushrooms). Glow: an HDR color added to the albedo, so that the
// plant burns in its color wherever the light and the ambient reach it (the rift's garden plants). Rim: a Fresnel glow,
// in stops, with a slow pulse (the spectral mushrooms). Ramp: an inner glow read from a ramp by the facing of the surface,
// masked, broken by a noise seen at a depth, tinted by the vertex color (the leaves and the wisteria). Grass: the blades of
// the generated tufts (Tools > Nature > Generate Grass), colored from their foot to their tip by the vertex color (r the
// height along the blade, g a tint per blade, b the petals, which glow), bending from their foot, with snow on their tips
// where the sky is open (Snow.hlsl), melted around the flames. Used by the materials of Art/Materials/Nature
Shader "Towards the Unknown/Nature Lit"
{
    Properties
    {
        [Header(Surface)]
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _MaskMap ("Metallic (R), Smoothness (A)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [Toggle(_NORMALMAP)] _UseNormalMap ("Normal Map", Float) = 0
        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}

        [Header(Emission)]
        [Toggle(_EMISSION)] _UseEmission ("Emission", Float) = 0
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Glow)] // added to the albedo
        [Toggle(_GLOW)] _UseGlow ("Glow", Float) = 0
        _GlowMap ("Glow Map", 2D) = "white" {}
        [HDR] _GlowColor ("Glow Color", Color) = (0, 0, 0, 1)

        [Header(Rim)] // a Fresnel glow
        [Toggle(_RIM)] _UseRim ("Rim", Float) = 0
        _RimColor ("Rim Color", Color) = (0, 0.3, 1, 1)
        _RimIntensity ("Rim Intensity, in stops", Range(-4, 8)) = 1
        _RimPower ("Rim Power, 0 lights the whole surface", Range(0, 8)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.5
        _PulseSpeed ("Pulse Speed", Float) = 1

        [Header(Ramp)] // an inner glow
        [Toggle(_RAMP)] _UseRamp ("Ramp", Float) = 0
        _RampMap ("Ramp, read left to right as the surface faces away", 2D) = "white" {}
        [HDR] _RampTint ("Ramp Tint", Color) = (1, 1, 1, 1)
        _RampPower ("Ramp Power", Float) = 1
        _RampFacingExp ("Facing Exponent", Float) = 0.1
        _RampOffsetExp ("Ramp Offset Exponent", Float) = 1.05
        _RampRemapMax ("Ramp Remap Max", Float) = 1
        _RampMask ("Ramp Mask (R)", 2D) = "white" {}
        _RampMaskExp ("Mask Exponent", Float) = 4
        _RampMaskOffset ("Mask Offset", Float) = 0.6
        [Toggle(_RAMP_NOISE)] _UseRampNoise ("Ramp Noise", Float) = 0
        _RampNoise ("Ramp Noise (R)", 2D) = "white" {}
        _RampNoiseScale ("Noise Scale (UV)", Vector) = (2, 2, 0, 0)
        _RampNoiseDepth ("Noise Depth", Float) = 0.175
        _RampNoiseOffset ("Noise Offset", Float) = 0.05

        [Header(Grass)]
        [Toggle(_GRASS)] _UseGrass ("Grass", Float) = 0
        _GrassBaseColor ("Foot Color", Color) = (0.1, 0.16, 0.16, 1)
        _GrassTipColor ("Tip Color", Color) = (0.42, 0.55, 0.5, 1)
        _FlowerColor ("Petal Color", Color) = (0.75, 0.6, 1, 1)
        [HDR] _FlowerGlow ("Petal Glow", Color) = (0.4, 0.25, 1, 1)
        _GrassSnow ("Snow on the tips", Range(0, 1)) = 0.5
        _GrassSnowStart ("Snow from, along the blade", Range(0, 1)) = 0.55
        [HDR] _SnowColor ("Snow Color", Color) = (0.92, 0.96, 1.05, 1)

        [Header(Wind)]
        [Toggle(_WIND)] _UseWind ("Wind", Float) = 0
        [Enum(Height, 0, Hanging, 1, Rigid, 2, Floating, 3)] _WindMask ("Mask", Float) = 0
        _WindHeight ("Height above the pivot (below when hanging), in meters", Float) = 1
        _WindBend ("Bend: how far the top leans in a gust, in meters", Float) = 0.05
        _WindFlutter ("Flutter: how far the tips shiver, in meters", Float) = 0.005
        _WindPush ("Push: how far the entities bend it, in meters", Float) = 0
        [HideInInspector] _WindAnchor ("Anchor, set by WindAnchor", Vector) = (0, 0, 0, 0)
        [Header(Floating)] // the wave of the water under it
        _WaveSpeed ("Wave Speed", Float) = 0.8
        _WaveFrequency ("Wave Frequency", Float) = 4
        _WaveScale ("Wave Scale", Float) = 0.02
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Wind.hlsl"
        #include "Snow.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Metallic;
            half _Smoothness;
            half4 _EmissionColor;
            half4 _GlowColor;
            half4 _RimColor;
            half _RimIntensity;
            half _RimPower;
            half _PulseAmount;
            float _PulseSpeed;
            half4 _RampTint;
            half _RampPower;
            half _RampFacingExp;
            half _RampOffsetExp;
            half _RampRemapMax;
            half _RampMaskExp;
            half _RampMaskOffset;
            float4 _RampNoiseScale;
            half _RampNoiseDepth;
            half _RampNoiseOffset;
            half _WindMask;
            float _WindHeight;
            half _WindBend;
            half _WindFlutter;
            half _WindPush;
            float4 _WindAnchor;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveScale;
            half4 _GrassBaseColor;
            half4 _GrassTipColor;
            half4 _FlowerColor;
            half4 _FlowerGlow;
            half _GrassSnow;
            half _GrassSnowStart;
            half4 _SnowColor;
        CBUFFER_END

        // color: the vertex color, whose red is the grass blade's height along it
        float3 NatureWind(float3 positionWS, half4 color)
        {
            #if defined(_WIND)
                WindSettings settings;
                settings.bend = _WindBend;
                settings.flutter = _WindFlutter;
                settings.height = _WindHeight;
                settings.mask = _WindMask;
                settings.push = _WindPush;
                settings.wave = float3(_WaveSpeed, _WaveFrequency, _WaveScale);
                #if defined(_GRASS)
                    settings.weight = color.r * color.r;
                #else
                    settings.weight = -1;
                #endif
                float3 pivotWS = WindPivot(_WindAnchor, settings);
                return ApplyWind(positionWS, pivotWS, settings);
            #else
                return positionWS;
            #endif
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _WIND
            #pragma shader_feature_local _GRASS
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _GLOW
            #pragma shader_feature_local_fragment _RIM
            #pragma shader_feature_local _RAMP
            #pragma shader_feature_local_fragment _RAMP_NOISE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MaskMap);
            TEXTURE2D(_NormalMap);
            TEXTURE2D(_EmissionMap);
            TEXTURE2D(_GlowMap);
            TEXTURE2D(_RampMap); SAMPLER(sampler_RampMap);
            TEXTURE2D(_RampMask);
            TEXTURE2D(_RampNoise); SAMPLER(sampler_RampNoise);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float4 tangentWS : TEXCOORD4;
                half4 color : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = NatureWind(TransformObjectToWorld(input.positionOS.xyz), input.color);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normals.normalWS;
                output.tangentWS = float4(normals.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 geometricNormalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 bitangentWS = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);

                float3 normalWS = geometricNormalWS;
                #if defined(_NORMALMAP)
                    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_BaseMap, input.uv));
                    normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld));
                #endif

                half4 mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, input.uv);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 emission = 0;

                #if defined(_GLOW)
                    albedo += SAMPLE_TEXTURE2D(_GlowMap, sampler_BaseMap, input.uv).rgb * _GlowColor.rgb;
                #endif
                #if defined(_GRASS)
                    // From the dark foot to the pale tip, each blade a little lighter or darker, the petals in their color
                    half along = input.color.r;
                    albedo *= lerp(_GrassBaseColor.rgb, _GrassTipColor.rgb, along) * (0.85 + 0.3 * input.color.g);
                    albedo = lerp(albedo, _FlowerColor.rgb, input.color.b);
                    emission += _FlowerGlow.rgb * input.color.b;
                    // Snow on the tips where the sky is open, melted around the flames
                    half snowTip = _GrassSnow * smoothstep(_GrassSnowStart, 1, along) * (1 - input.color.b);
                    snowTip *= SkyExposure(input.positionWS) * (1 - HeatMelt(input.positionWS));
                    albedo = lerp(albedo, _SnowColor.rgb, snowTip);
                #endif
                #if defined(_EMISSION)
                    emission += SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, input.uv).rgb * _EmissionColor.rgb;
                #endif
                #if defined(_RIM)
                    half facingRim = saturate(dot(geometricNormalWS, viewWS));
                    half pulse = 1 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                    emission += _RimColor.rgb * exp2(_RimIntensity) * pow(max(1 - facingRim, 1e-4), _RimPower) * pulse;
                #endif
                #if defined(_RAMP)
                    // The inner glow: strongest where the surface faces the view, masked, broken by a noise seen at a depth
                    half inner = pow(saturate(dot(geometricNormalWS, viewWS)), _RampFacingExp);
                    inner *= saturate(pow(SAMPLE_TEXTURE2D(_RampMask, sampler_BaseMap, input.uv).r, _RampMaskExp) + _RampMaskOffset);
                    #if defined(_RAMP_NOISE)
                        float2 noiseUV = input.uv * _RampNoiseScale.xy;
                        half height = SAMPLE_TEXTURE2D(_RampNoise, sampler_RampNoise, noiseUV).r;
                        float3 viewTS = normalize(mul(tangentToWorld, viewWS));
                        viewTS.z += 0.42;
                        float2 parallax = (height * _RampNoiseDepth - _RampNoiseDepth * 0.5) * (viewTS.xy / viewTS.z);
                        inner *= saturate(SAMPLE_TEXTURE2D(_RampNoise, sampler_RampNoise, noiseUV + parallax).r + _RampNoiseOffset);
                    #endif
                    inner = saturate(inner);
                    half rampU = 1 - pow(1 - saturate(inner / _RampRemapMax), _RampOffsetExp);
                    half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(rampU, 0)).rgb;
                    emission += _RampPower * ramp * _RampTint.rgb * inner * input.color.rgb;
                #endif

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
                surface.albedo = albedo;
                surface.metallic = mask.r * _Metallic;
                surface.smoothness = mask.a * _Smoothness;
                #if defined(_GRASS)
                    // The foot of the tuft is in the shade of the blades around it
                    surface.occlusion = lerp(0.3, 1, saturate(input.color.r * 1.4));
                #else
                    surface.occlusion = 1;
                #endif
                surface.alpha = 1;
                surface.normalTS = half3(0, 0, 1);
                surface.emission = emission;

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
            #pragma shader_feature_local _WIND
            #pragma shader_feature_local _GRASS
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = NatureWind(TransformObjectToWorld(input.positionOS.xyz), input.color);
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
            #pragma shader_feature_local _WIND
            #pragma shader_feature_local _GRASS
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformWorldToHClip(NatureWind(TransformObjectToWorld(input.positionOS.xyz), input.color));
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
            #pragma shader_feature_local _WIND
            #pragma shader_feature_local _GRASS
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
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
                output.positionCS = TransformWorldToHClip(NatureWind(TransformObjectToWorld(input.positionOS.xyz), input.color));
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
