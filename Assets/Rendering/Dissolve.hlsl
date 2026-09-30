// A mesh drawn up to a height of its own space (object Y), which the code moves (_DissolvePosition): the player's weapons
// growing out of the hand (Weapon Glow). The includer declares _DissolvePosition in its UnityPerMaterial buffer. Included
// a second time after DepthPasses.hlsl, it gives the shadow caster, depth and depth normals passes cut at the same height
#ifndef DISSOLVE_INCLUDED
#define DISSOLVE_INCLUDED

// Cuts what is above the height drawn; returns how close the point is to the cut, from 0 (1.5 units under it) to 1
half Dissolve(float height)
{
    float left = _DissolvePosition + 0.5 - height;
    clip(left);
    return saturate(1 - left / 1.5);
}

#endif

#if defined(DEPTH_PASSES_INCLUDED) && !defined(DISSOLVE_DEPTH_INCLUDED)
#define DISSOLVE_DEPTH_INCLUDED

struct DissolveDepthVaryings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD0;
    float height : TEXCOORD1;
};

DissolveDepthVaryings DissolveShadowVert(DepthAttributes input)
{
    DissolveDepthVaryings output;
    output.positionCS = ShadowVert(input);
    output.normalWS = 0;
    output.height = input.positionOS.y;
    return output;
}

DissolveDepthVaryings DissolveDepthVert(DepthAttributes input)
{
    DissolveDepthVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.height = input.positionOS.y;
    return output;
}

half4 DissolveDepthFrag(DissolveDepthVaryings input) : SV_Target
{
    Dissolve(input.height);
    return 0;
}

half4 DissolveDepthNormalsFrag(DissolveDepthVaryings input) : SV_Target
{
    Dissolve(input.height);
    return half4(NormalizeNormalPerPixel(input.normalWS), 0);
}

#endif
