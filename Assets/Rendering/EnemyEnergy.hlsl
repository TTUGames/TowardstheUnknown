// The creatures of the rift: a dark lit flesh crossed by veins of energy flowing through it, a rim and an aura of the same
// energy flickering around the silhouette. Lengths are in meters whatever the model's import scale (the noise runs on the
// object position times the object's scale); the veins stick to the body, the aura's flames rise in world space.
// _GlowMultiplier can be driven at runtime (0 turns the energy off, above 1 flares it)
#ifndef ENEMY_ENERGY_INCLUDED
#define ENEMY_ENERGY_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Noise.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);
TEXTURE2D(_GlowMask);

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _BumpScale;
    half _Metallic;
    half _Smoothness;
    half4 _GlowColor;
    half _GlowIntensity;
    half _GlowMultiplier;
    half _MaskStrength;
    float _VeinScale;
    half _VeinSharpness;
    half _VeinAmount;
    float _VeinFlow;
    half _VeinCoverage;
    half _VeinDarken;
    half _RimStrength;
    half _RimPower;
    half _PulseAmount;
    float _PulseSpeed;
    float _PulseFrequency;
    float _AuraWidth;
    half _AuraIntensity;
    float _AuraNoiseScale;
    float _AuraSpeed;
    half _AuraSoftness;
    half _AuraCoverage;
CBUFFER_END

float3 ObjectScale()
{
    return float3(length(UNITY_MATRIX_M._m00_m10_m20), length(UNITY_MATRIX_M._m01_m11_m21), length(UNITY_MATRIX_M._m02_m12_m22));
}

// The object position in meters: the model's frame, at the scale it is drawn
float3 MetricObjectPosition(float3 positionOS)
{
    return positionOS * ObjectScale();
}

// The world's up in that metric frame, whichever axis the model was built along (the Golem stands on Z)
float3 MetricObjectUp()
{
    return normalize(TransformWorldToObjectDir(float3(0, 1, 0), false) * ObjectScale());
}

// Thin lines where the noise crosses its middle: 1 on a line, 0 away from it
half Ridge(float n, half sharpness)
{
    return pow(saturate(1 - abs(n * 2 - 1)), sharpness);
}

// The veins flowing up the body (upM: the world's up in the metric object frame)
half Veins(float3 positionM, float3 upM, float time)
{
    float3 p = positionM * _VeinScale;
    float3 flow = -upM * time * _VeinFlow * _VeinScale;
    // Slow domain warp: the veins twist and crawl instead of scrolling as a block
    float3 warp = float3(ValueNoise(p * 0.5 + time * 0.07), ValueNoise(p * 0.5 + 17.3 - time * 0.05), ValueNoise(p * 0.5 + 41.1)) * 1.5;
    half veins = Ridge(ValueNoise(p + flow + warp), _VeinSharpness);
    veins = max(veins, Ridge(ValueNoise(p * 2.3 + flow * 1.7 + warp + 7.1), _VeinSharpness * 1.5) * 0.6);
    // Only in patches, drifting slowly: the energy surfaces here and there rather than everywhere
    half patches = ValueNoise(positionM * _VeinScale * 0.3 + float3(time * 0.11, time * 0.07, -time * 0.05) + 3.7);
    return veins * smoothstep(1 - _VeinCoverage, 1 - _VeinCoverage + 0.2, patches);
}

// Brighter on the waves travelling up the body, height in meters above the object's pivot
half Pulse(float height, float time)
{
    half wave = 0.5 + 0.5 * sin(height * _PulseFrequency - time * _PulseSpeed);
    return lerp(1 - _PulseAmount, 1 + _PulseAmount, wave);
}

#endif

// The forward pass, once Lighting.hlsl is included
#if defined(UNIVERSAL_LIGHTING_INCLUDED) && !defined(ENEMY_ENERGY_FORWARD_INCLUDED)
#define ENEMY_ENERGY_FORWARD_INCLUDED

struct EnergyAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct EnergyVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float4 tangentWS : TEXCOORD3;
    float3 positionM : TEXCOORD4;
    float3 upM : TEXCOORD5;
    half fogFactor : TEXCOORD6;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

EnergyVaryings EnergyVert(EnergyAttributes input)
{
    EnergyVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.positionCS = position.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.positionWS = position.positionWS;
    output.normalWS = normal.normalWS;
    output.tangentWS = float4(normal.tangentWS, input.tangentOS.w * GetOddNegativeScale());
    output.positionM = MetricObjectPosition(input.positionOS.xyz);
    output.upM = MetricObjectUp();
    output.fogFactor = ComputeFogFactor(position.positionCS.z);
    return output;
}

half4 EnergyFrag(EnergyVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    float time = _Time.y;

    half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BaseMap, input.uv), _BumpScale);
    float3 bitangentWS = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
    float3 normalWS = normalize(TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS)));
    float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half facing = saturate(dot(normalWS, viewWS));

    half veins = Veins(input.positionM, normalize(input.upM), time) * _VeinAmount;
    half mask = SAMPLE_TEXTURE2D(_GlowMask, sampler_BaseMap, input.uv).r * _MaskStrength;
    half pulse = Pulse(input.positionWS.y - UNITY_MATRIX_M._m13, time);
    half rim = pow(max(1 - facing, 1e-4), _RimPower) * _RimStrength;
    half3 energy = _GlowColor.rgb * exp2(_GlowIntensity) * ((veins + mask) * pulse + rim) * max(_GlowMultiplier, 0);

    // The flesh burns dark around the veins, so that they read as cracks
    half3 albedo = (SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor).rgb * (1 - saturate(veins * 4) * _VeinDarken);

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
    surface.normalTS = normalTS;
    surface.emission = energy;

    half4 color = UniversalFragmentPBR(inputData, surface);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

#endif

// The aura pass: a shell pushed out around the body, of which only the back faces are drawn, additively: the body hides them
// wherever it stands in front, so the aura only shows around the silhouette. It is pushed along the normal of an ellipsoid
// fitted to the renderer's bounds rather than along the mesh's normals, which split at the hard edges of the low poly models
// and would crack the shell. Rising flames of noise eat it
#if defined(ENEMY_ENERGY_AURA) && !defined(ENEMY_ENERGY_AURA_INCLUDED)
#define ENEMY_ENERGY_AURA_INCLUDED

struct AuraAttributes
{
    float4 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct AuraVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 directionWS : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float AuraNoise(float3 positionWS, float time)
{
    float3 p = positionWS * _AuraNoiseScale - float3(0, time * _AuraSpeed, 0);
    return ValueNoise(p) * 0.65 + ValueNoise(p * 2.1 + 5.3) * 0.35;
}

// The normal of the ellipsoid of the renderer's bounds through this point: smooth all around the body
float3 AuraDirection(float3 positionWS)
{
    float3 center = (unity_RendererBounds_Min.xyz + unity_RendererBounds_Max.xyz) * 0.5;
    float3 extents = max((unity_RendererBounds_Max.xyz - unity_RendererBounds_Min.xyz) * 0.5, 0.05);
    return normalize((positionWS - center) / (extents * extents));
}

AuraVaryings AuraVert(AuraAttributes input)
{
    AuraVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 direction = AuraDirection(positionWS);
    // The shell breathes with the flames: thicker where they burn
    positionWS += direction * _AuraWidth * (0.5 + AuraNoise(positionWS, _Time.y));
    output.positionCS = TransformWorldToHClip(positionWS);
    output.positionWS = positionWS;
    output.directionWS = direction;
    return output;
}

half4 AuraFrag(AuraVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    // Faded towards the shell's outer edge, where it turns side on
    half side = abs(dot(normalize(input.directionWS), GetWorldSpaceNormalizeViewDir(input.positionWS)));
    half fade = smoothstep(0, _AuraSoftness, side);
    half flames = saturate((AuraNoise(input.positionWS, _Time.y) - (1 - _AuraCoverage)) * 3);
    half3 color = _GlowColor.rgb * exp2(_AuraIntensity) * fade * flames * max(_GlowMultiplier, 0);
    return half4(color, 0);
}

#endif
