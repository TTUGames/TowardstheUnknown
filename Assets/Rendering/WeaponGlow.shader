// A weapon of the player: the outfit's surface and glow (Glow.hlsl: a lit PBR surface whose glow mask emits the glow color,
// washed out by the cold like the outfit, with its rim and the magic running through its bands), drawn up to a height of
// its mesh that Dissolving moves (Dissolve.hlsl): the weapon grows out of the hand, its cut glowing, and casts its shadow
// and writes its depth only where it is drawn. Its meshes carry no weather channel: no snow settles on it
// Used by Art/Models/Weapons/Materials
Shader "Towards the Unknown/Weapon Glow"
{
    Properties
    {
        [Header(Surface)]
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor ("Albedo Tint", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.12
        _AlbedoBoost ("Albedo Brightness", Range(0, 16)) = 1

        [Header(Dissolve)]
        _DissolvePosition ("Height Drawn, set at runtime (whole at 5, none at -2)", Float) = 5
        _DissolveGlow ("Glow of the Cut", Range(0, 8)) = 2

        [Header(Glow)]
        [NoScaleOffset] _GlowMask ("Glow Mask (brightest channel)", 2D) = "black" {}
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

        [Header(Weather)]
        _Weathering ("Washed Out", Range(0, 1)) = 0.3

        // The outfit's snow and hem, which Glow.hlsl computes: left at values that draw none
        [HideInInspector] _SnowThreshold ("Snow Threshold", Range(0, 1)) = 0.3
        [HideInInspector] _SnowSoftness ("Snow Edge Softness", Range(0.01, 1)) = 0.35
        [HideInInspector] _SnowScale ("Snow Patches per Meter", Float) = 11
        [HideInInspector] _HemHeight ("Hem Glow Height", Range(0.01, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #define GLOW_MASK
        #define GLOW_DISSOLVE
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
            #pragma vertex DissolveShadowVert
            #pragma fragment DissolveDepthFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            #include "Dissolve.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DissolveDepthVert
            #pragma fragment DissolveDepthFrag
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            #include "Dissolve.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex DissolveDepthVert
            #pragma fragment DissolveDepthNormalsFrag
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            #include "Dissolve.hlsl"
            ENDHLSL
        }
    }
}
