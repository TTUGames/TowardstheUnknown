// A low poly flame: faceted tongues whose vertices lick upward with noise, lit from inside. The facets facing the
// camera burn white-yellow, the grazing ones orange then red, so that the facets read as cut gems of fire.
// On a flame mesh (uv.y: height in its tongue, 0 at the base, 1 at the tip; uv.x: the tongue's seed), or, with
// Particles on, on mesh particles tinted by their color (embers and shards: no lick, the particle moves them).
Shader "Towards the Unknown/Stylized Flame"
{
    Properties
    {
        [HDR] _CoreColor ("Core", Color) = (4, 3.1, 1.6, 1)
        [HDR] _MidColor ("Middle", Color) = (3.2, 1.25, 0.25, 1)
        [HDR] _EdgeColor ("Edge", Color) = (1.4, 0.22, 0.04, 1)
        _Bands ("Bands (core, middle)", Vector) = (0.72, 0.42, 0, 0)
        _BandSoftness ("Band Softness", Range(0.001, 0.2)) = 0.04
        _TipRedness ("Tip Redness", Range(0, 1)) = 0.35
        _FacingWeight ("Facing Weight", Range(0, 1)) = 0.5
        _Glint ("Glint", Range(0, 4)) = 1.5
        _GlintPower ("Glint Sharpness", Range(2, 64)) = 20
        _Flicker ("Flicker", Range(0, 1)) = 0.25
        _EdgeAlpha ("Edge Opacity", Range(0, 1)) = 0.55
        _Lick ("Lick", Range(0, 0.6)) = 0.22
        _Sway ("Sway", Range(0, 0.6)) = 0.12
        _Speed ("Speed", Range(0, 6)) = 2.4
        _NoiseScale ("Noise Scale", Range(0.5, 12)) = 3.2
        _Breath ("Breath", Range(0, 0.5)) = 0.16
        [Toggle(_PARTICLES)] _Particles ("Particles", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "StylizedFlame"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma shader_feature_local _PARTICLES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Noise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _MidColor;
                half4 _EdgeColor;
                half4 _Bands;
                half _BandSoftness;
                half _TipRedness;
                half _FacingWeight;
                half _Glint;
                half _GlintPower;
                half _Flicker;
                half _EdgeAlpha;
                half _Lick;
                half _Sway;
                half _Speed;
                half _NoiseScale;
                half _Breath;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half4 color : COLOR;
                float2 uv : TEXCOORD1;
                float seed : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                #if defined(_PARTICLES)
                    output.seed = 0;
                #else
                    // Each flame and each of its tongues at its own pace
                    float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                    float seed = Hash(floor(origin * 7.31) + 0.5) * 50 + input.uv.x * 13.7;
                    float scale = length(TransformObjectToWorldDir(float3(0, 1, 0), false));
                    float height = input.uv.y;
                    float time = _Time.y * _Speed;
                    // Breath: the tongue grows and shrinks, its tip most
                    float breath = (ValueNoise(float3(seed, time * 0.7, 0)) * 2 - 1) * _Breath;
                    positionWS.y += (positionWS.y - origin.y) * breath * height;
                    // Lick: noise rising along the tongue bends it, more toward the tip; sway: the whole flame leans
                    float3 p = float3(positionWS.x, positionWS.y - time * 0.45 * scale, positionWS.z) * _NoiseScale / max(scale, 0.001);
                    float2 lick = float2(ValueNoise(p + seed), ValueNoise(p + seed + 31.7)) * 2 - 1;
                    float2 sway = float2(ValueNoise(float3(seed, time * 0.3, 3)), ValueNoise(float3(seed, time * 0.3, 9))) * 2 - 1;
                    float bend = height * height;
                    positionWS.xz += (lick * _Lick * bend + sway * _Sway * height) * scale;
                    output.seed = seed;
                #endif
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.color = input.color;
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Flat facets: the normal of the triangle, from the screen derivatives
                float3 normal = normalize(cross(ddy(input.positionWS), ddx(input.positionWS)));
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                if (dot(normal, view) < 0) normal = -normal;
                half facing = saturate(dot(normal, view));
                // A facet's own flicker, steady over the facet: hashed from its normal
                half facetHash = Hash(floor(normal * 9.0) + input.seed);
                half flicker = (ValueNoise(float3(facetHash * 40, _Time.y * _Speed * 2.2, input.seed)) * 2 - 1) * _Flicker;

                #if defined(_PARTICLES)
                    half heat = facing * 0.9 + flicker * 0.5;
                #else
                    half height = input.uv.y;
                    // Hot at the heart (low and facing the camera), cooling toward the edges and the tip
                    half heat = lerp(1 - height, facing, _FacingWeight) - height * _TipRedness + flicker;
                #endif

                half core = smoothstep(_Bands.x - _BandSoftness, _Bands.x + _BandSoftness, heat);
                half middle = smoothstep(_Bands.y - _BandSoftness, _Bands.y + _BandSoftness, heat);
                half3 color = lerp(lerp(_EdgeColor.rgb, _MidColor.rgb, middle), _CoreColor.rgb, core);
                half alpha = lerp(_EdgeAlpha, 1, middle);
                // A glint on the facets turned right at the camera: the cut gem catching the light
                color += _CoreColor.rgb * pow(facing, _GlintPower) * _Glint * middle;

                #if defined(_PARTICLES)
                    color *= input.color.rgb;
                    alpha *= input.color.a;
                #endif

                color = MixFog(color, input.fogFactor);
                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
