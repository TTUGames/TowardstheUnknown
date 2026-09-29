// Glowing outfit of a character: a lit PBR surface (albedo, metallic, smoothness) whose glow mask emits, in a color and an
// intensity in exposure stops, with a rim of the glow color and a slow pulse (Glow.hlsl, GLOW_MASK). The glow runs like
// magic: waves of it run up the body, from its color to a deeper shade of it, bright surges travel through the bands and
// a soft halo spreads around them; the hem of a garment whose mesh carries it (UV2.x, 0 at the belt, 1 at the hem) glows up in a gradient.
// Matte fabric with a grain of threads, worn in the cold: its colors are washed out, and snow lies on it in thick patches
// where the mesh's weather channel (UV set 2, OutfitWeather: the fabric's point at the bind pose and how much snow settles
// there) lets it: the head and shoulders, the back, the hem. The patches dim the bands under them
// Used by the materials of GlowClothes, variants of CharacterGlow.mat
Shader "Towards the Unknown/Character Glow"
{
    Properties
    {
        [Header(Surface)]
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor ("Albedo Tint", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.12

        [Header(Glow)]
        [NoScaleOffset] _GlowMask ("Glow Mask (R)", 2D) = "black" {}
        _GlowColor ("Glow Color", Color) = (0.345, 0.267, 0.8, 1)
        _GlowIntensity ("Glow Intensity, in stops", Range(-4, 8)) = 2
        _GlowMultiplier ("Glow Multiplier, set at runtime", Float) = 1

        [Header(Rim)]
        _RimStrength ("Rim Strength", Range(0, 4)) = 0.25
        _RimPower ("Rim Power", Range(0.5, 8)) = 3

        [Header(Pulse)]
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.15
        _PulseSpeed ("Pulse Speed", Float) = 1.2

        [Header(Magic)]
        _GradientShade ("Gradient Deep Shade, share of the glow", Range(0, 1)) = 0.4
        _HueShift ("Hue Shift of the Deep Shade, in turns", Range(-0.5, 0.5)) = 0
        _FlowScale ("Flow Scale, waves per meter", Float) = 1.5
        _FlowSpeed ("Flow Speed, waves per second", Float) = 0.3
        _SurgeStrength ("Surge Strength", Range(0, 4)) = 0.45
        _SparkleStrength ("Sparkle Strength", Range(0, 2)) = 0.08
        _HaloStrength ("Halo Strength", Range(0, 2)) = 0.12
        _HaloBlur ("Halo Blur, in mip levels", Range(0, 8)) = 4
        _HemGlow ("Hem Glow", Range(0, 4)) = 0
        _HemHeight ("Hem Glow Height", Range(0.01, 1)) = 0.35
        _HemTint ("Hem Albedo Tint", Range(0, 1)) = 0

        [Header(Weather)]
        _Weathering ("Washed Out", Range(0, 1)) = 0.3
        [HDR] _SnowColor ("Snow Color", Color) = (0.9, 0.95, 1.05, 1)
        _SnowThreshold ("Snow Threshold: the map's value where the snow starts", Range(0, 1)) = 0.3
        _SnowSoftness ("Snow Edge Softness", Range(0.01, 1)) = 0.35
        _SnowSmoothness ("Snow Smoothness", Range(0, 1)) = 0.3
        _SnowBump ("Snow Thickness, in meters", Range(0, 0.02)) = 0.002
        _SnowScale ("Snow Patches per Meter", Float) = 11

        [Header(Fabric)]
        _FabricScale ("Threads per Meter", Float) = 260
        _FabricGrain ("Grain in the Albedo", Range(0, 1)) = 0.25
        _FabricBump ("Grain Relief, in meters", Range(0, 0.005)) = 0.0006
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #define GLOW_MASK
        #include "Glow.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex GlowVert
            #pragma fragment GlowFrag
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
            #include "Glow.hlsl"
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
            #pragma vertex ShadowVert
            #pragma fragment DepthFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            ENDHLSL
        }
    }
}
