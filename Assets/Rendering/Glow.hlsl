// The lit surfaces that glow in HDR, for the bloom, whatever the lights, with a slow pulse: Spectral Glow (the silhouette
// glows, a Fresnel rim) and Character Glow (GLOW_MASK: an albedo map, a glow mask and a rim of the glow color).
// _GlowMultiplier can be driven at runtime (0 turns the glow off, above 1 flashes it)
#ifndef GLOW_INCLUDED
#define GLOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(GLOW_MASK)
    #include "Noise.hlsl"
#endif

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
        half _GradientShade;
        half _HueShift;
        float _FlowScale;
        float _FlowSpeed;
        half _SurgeStrength;
        half _SparkleStrength;
        half _HaloStrength;
        half _HaloBlur;
        half _HemGlow;
        half _HemHeight;
        half _HemTint;
        half _Weathering;
        half4 _SnowColor;
        half _SnowThreshold;
        half _SnowSoftness;
        half _SnowSmoothness;
        float _SnowBump;
        float _SnowScale;
        float _FabricScale;
        half _FabricGrain;
        float _FabricBump;
    #endif
    half _RimPower;
    half _PulseAmount;
    float _PulseSpeed;
CBUFFER_END

#endif

// The forward pass, once Lighting.hlsl is included
#if defined(UNIVERSAL_LIGHTING_INCLUDED) && !defined(GLOW_FORWARD_INCLUDED)
#define GLOW_FORWARD_INCLUDED

// Bends a normal by the slope of a height (in meters) across the screen: a bump without tangents (Mikkelsen's surface gradient)
float3 BumpNormal(float3 normalWS, float3 positionWS, float height)
{
    float3 dpdx = ddx(positionWS), dpdy = ddy(positionWS);
    float3 r1 = cross(dpdy, normalWS), r2 = cross(normalWS, dpdx);
    float det = dot(dpdx, r1);
    float3 gradient = sign(det) * (ddx(height) * r1 + ddy(height) * r2);
    return normalize(abs(det) * normalWS - gradient);
}

// Turns a color's hue by a share of a turn, keeping its brightness (a rotation around the gray axis)
half3 HueShift(half3 color, half turns)
{
    half angle = turns * TWO_PI;
    half3 k = half3(0.57735, 0.57735, 0.57735);
    half c = cos(angle);
    return color * c + cross(k, color) * sin(angle) + k * dot(k, color) * (1 - c);
}

struct GlowAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    // The hem gradient of a garment (0 at the belt, 1 at the hem); 0 on the meshes without it
    float2 uv2 : TEXCOORD1;
    // The weather of a garment (OutfitWeather): xyz its point at the bind pose, in meters, fixed on the fabric; w how much
    // snow settles there (the head and shoulders, the back, the hem). 0 on the meshes without it
    float4 weather : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GlowVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    half fogFactor : TEXCOORD3;
    float hem : TEXCOORD4;
    float4 weather : TEXCOORD5;
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
    output.hem = input.uv2.x;
    output.weather = input.weather;
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
        // The mask glows and breathes slowly; the rim outlines the character in the glow color, in front of the board.
        // Waves run up the body in world space: the glow goes from its color to a deeper shade of it (and a shifted hue,
        // none by default), and the crest of each wave surges through the bands; a finer noise sparkles in them
        half3 albedo = (SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor).rgb;
        half mask = SAMPLE_TEXTURE2D(_GlowMask, sampler_BaseMap, input.uv).r;
        // Washed out by the weather: towards a pale cold gray of its own brightness
        half luminance = dot(albedo, half3(0.299, 0.587, 0.114));
        albedo = lerp(albedo, luminance * half3(0.85, 0.92, 1.05) + 0.06, _Weathering);
        // Fixed on the fabric: its point at the bind pose, which the animation doesn't move
        float3 fabric = input.weather.xyz;
        // The grain: vertical threads, faded out where they would be finer than the pixels
        float3 threads = fabric * _FabricScale * float3(1, 3, 1);
        float grain = ValueNoise(threads) * 0.6 + ValueNoise(threads * 2.3 + 4.1) * 0.4;
        half fineness = saturate(1.5 - length(fwidth(threads)));
        albedo *= 1 + (grain - 0.5) * _FabricGrain * fineness;
        // The snow: patches of a noise on the fabric where it settles, and a few flakes caught anywhere
        float3 patches = fabric * _SnowScale;
        half snowField = ValueNoise(patches) * 0.52 + ValueNoise(patches * 2.4 + 17.3) * 0.26 + ValueNoise(patches * 5.3 + 34.6) * 0.13;
        // A fine grain breaks the edges and the full patches: never a flat white
        half snowGrain = ValueNoise(patches * 13 + 51.2);
        snowField += snowGrain * 0.09;
        half settles = input.weather.w;
        snowField = saturate((snowField - (1 - settles)) * 2.4 + settles * 0.15);
        half snowMap = snowField;
        half snow = smoothstep(_SnowThreshold, _SnowThreshold + _SnowSoftness, snowField) * (0.7 + 0.3 * snowGrain);
        snow = max(snow, smoothstep(0.84, 0.92, ValueNoise(fabric * 140 + 3.1)) * _Weathering * 0.5 * step(0.001, settles));
        mask *= 1 - snow * 0.75;
        // The threads and the thickness of the snow catch the light
        normalWS = BumpNormal(normalWS, input.positionWS, grain * _FabricBump * fineness + snowMap * _SnowBump);
        float flow = (input.positionWS.y + dot(input.positionWS.xz, float2(0.35, 0.2))) * _FlowScale - _Time.y * _FlowSpeed;
        half wave = 0.5 + 0.5 * sin(flow * TWO_PI);
        half3 deep = HueShift(_GlowColor.rgb, _HueShift) * _GradientShade;
        half3 magic = lerp(deep, _GlowColor.rgb, smoothstep(0.15, 0.85, wave));
        half surge = 1 + _SurgeStrength * pow(saturate(sin(flow * PI)), 6);
        half sparkle = 1 + _SparkleStrength * (ValueNoise(input.positionWS * 40 + _Time.y * float3(0, 3, 0)) * 2 - 1);
        // The mask's blurred mips spread a halo around the bands, on the fabric
        half halo = saturate(SAMPLE_TEXTURE2D_BIAS(_GlowMask, sampler_BaseMap, input.uv, _HaloBlur).r * 3 - mask) * _HaloStrength;
        // The hem lights up towards its edge, flickering with the waves
        half hem = smoothstep(1 - _HemHeight, 1, input.hem);
        albedo = lerp(albedo, albedo * 0.4 + magic * 0.25, hem * _HemTint);
        albedo = lerp(albedo, _SnowColor.rgb * (0.8 + 0.2 * snowGrain), snow);
        half3 glow = magic * exp2(_GlowIntensity) * pulse * (mask * surge * sparkle + halo)
            + _GlowColor.rgb * hem * hem * _HemGlow * (0.6 + 0.4 * wave) * pulse
            + magic * pow(1 - facing, _RimPower) * _RimStrength;
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
    #if defined(GLOW_MASK)
        surface.smoothness = lerp(_Smoothness, _SnowSmoothness, snow);
    #else
        surface.smoothness = _Smoothness;
    #endif
    surface.occlusion = 1;
    surface.alpha = 1;
    surface.normalTS = half3(0, 0, 1);
    surface.emission = glow * max(_GlowMultiplier, 0);

    half4 color = UniversalFragmentPBR(inputData, surface);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

#endif
