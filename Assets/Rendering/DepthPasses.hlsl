// The shadow caster, depth and depth normals passes of the lit shaders whose vertices stay still (Character Glow, Spectral
// Glow, Magic Crystal): each pass includes this file and picks its functions
#ifndef DEPTH_PASSES_INCLUDED
#define DEPTH_PASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;
float3 _LightPosition;

struct DepthAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct DepthNormalsVaryings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD0;
};

float4 ShadowVert(DepthAttributes input) : SV_POSITION
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

float4 DepthVert(DepthAttributes input) : SV_POSITION
{
    UNITY_SETUP_INSTANCE_ID(input);
    return TransformObjectToHClip(input.positionOS.xyz);
}

half4 DepthFrag() : SV_Target { return 0; }

DepthNormalsVaryings DepthNormalsVert(DepthAttributes input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    DepthNormalsVaryings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    return output;
}

half4 DepthNormalsFrag(DepthNormalsVaryings input) : SV_Target
{
    return half4(NormalizeNormalPerPixel(input.normalWS), 0);
}

#endif
