// The lit surfaces that glow in HDR, for the bloom, whatever the lights, with a slow pulse: Spectral Glow (the silhouette
// glows, a Fresnel rim) and Character Glow (GLOW_MASK: an albedo map, a glow mask and a rim of the glow color).
// _GlowMultiplier can be driven at runtime (0 turns the glow off, above 1 flashes it)
#ifndef GLOW_INCLUDED
#define GLOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#if defined(GLOW_MASK)
    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);
    TEXTURE2D(_GlowMask);
#endif

CBUFFER_START(UnityPerMaterial)
    #if defined(GLOW_MASK)
        float4 _BaseMap_ST;
    #endif
    half4 _BaseColor;
    half _Metallic;
    half _Smoothness;
    half4 _GlowColor;
    half _GlowIntensity;
    half _GlowMultiplier;
    #if defined(GLOW_MASK)
        half _RimStrength;
    #endif
    half _RimPower;
    half _PulseAmount;
    float _PulseSpeed;
CBUFFER_END

#endif

// The forward pass, once Lighting.hlsl is included
#if defined(UNIVERSAL_LIGHTING_INCLUDED) && !defined(GLOW_FORWARD_INCLUDED)
#define GLOW_FORWARD_INCLUDED

struct GlowAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GlowVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    half fogFactor : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

GlowVaryings GlowVert(GlowAttributes input)
{
    GlowVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = position.positionCS;
    #if defined(GLOW_MASK)
        output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #else
        output.uv = input.uv;
    #endif
    output.positionWS = position.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.fogFactor = ComputeFogFactor(position.positionCS.z);
    return output;
}

half4 GlowFrag(GlowVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    float3 normalWS = normalize(input.normalWS);
    float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half facing = saturate(dot(normalWS, viewWS));
    half pulse = 1 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

    #if defined(GLOW_MASK)
        // The mask glows and breathes slowly; the rim outlines the character in the glow color, in front of the board
        half3 albedo = (SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor).rgb;
        half mask = SAMPLE_TEXTURE2D(_GlowMask, sampler_BaseMap, input.uv).r;
        half3 glow = _GlowColor.rgb * exp2(_GlowIntensity) * mask * pulse + _GlowColor.rgb * pow(1 - facing, _RimPower) * _RimStrength;
    #else
        // The silhouette glows and breathes slowly
        half3 albedo = _BaseColor.rgb;
        half3 glow = _GlowColor.rgb * exp2(_GlowIntensity) * pow(max(1 - facing, 1e-4), _RimPower) * pulse;
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
    surface.metallic = _Metallic;
    surface.smoothness = _Smoothness;
    surface.occlusion = 1;
    surface.alpha = 1;
    surface.normalTS = half3(0, 0, 1);
    surface.emission = glow * max(_GlowMultiplier, 0);

    half4 color = UniversalFragmentPBR(inputData, surface);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

#endif
