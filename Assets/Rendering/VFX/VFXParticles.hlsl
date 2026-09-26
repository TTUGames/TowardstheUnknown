// Shared by the spells' particle shaders (Rendering/VFX and the shaders next to their effects in Art/VFX): the vertex
// streams of the particle systems (the color, uv0 with the custom data in zw, uv1, uv3), the fog, and the helpers they
// use. Unlit, drawn in the transparent queue (SRPDefaultUnlit). Soft particles are left out: the shaders ported from
// Amplify only faded near the surfaces with a built-in keyword URP never set, and their effects were tuned without it
#ifndef VFX_PARTICLES_INCLUDED
#define VFX_PARTICLES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct VFXAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    half4 color : COLOR;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv3 : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VFXVaryings
{
    float4 positionCS : SV_POSITION;
    half4 color : COLOR;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 uv3 : TEXCOORD2;
    float3 positionWS : TEXCOORD3;
    float3 normalWS : TEXCOORD4;
    float4 screenPos : TEXCOORD5;
    half fogFactor : TEXCOORD6;
    UNITY_VERTEX_OUTPUT_STEREO
};

VFXVaryings VFXVert(VFXAttributes input)
{
    VFXVaryings output = (VFXVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = position.positionCS;
    output.positionWS = position.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.screenPos = ComputeScreenPos(position.positionCS);
    output.color = input.color;
    output.uv0 = input.uv0;
    output.uv1 = input.uv1;
    output.uv3 = input.uv3;
    output.fogFactor = ComputeFogFactor(position.positionCS.z);
    return output;
}

// A texture's coordinates with its tiling and offset, moving at a speed (units per second)
float2 VFXPan(float2 uv, float4 tilingOffset, float2 speed)
{
    return uv * tilingOffset.xy + tilingOffset.zw + _Time.y * speed;
}

// The fresnel term of the built-in node: scale times (1 - N.V) to the power
float VFXFresnel(VFXVaryings input, float scale, float power)
{
    float3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
    return scale * pow(max(0.0, 1.0 - dot(normalize(input.normalWS), view)), power);
}

// The fog, towards black for the additive shaders
half4 VFXFog(half4 color, VFXVaryings input, bool additive)
{
    color.rgb = additive ? MixFogColor(color.rgb, half3(0, 0, 0), input.fogFactor) : MixFog(color.rgb, input.fogFactor);
    return color;
}

#endif
