// Glowing outfit of a character: a lit PBR surface (albedo, metallic, smoothness) whose glow mask emits, in a color and an
// intensity in exposure stops, with a rim of the glow color and a slow pulse (Glow.hlsl, GLOW_MASK)
// Used by the materials of GlowClothes, variants of CharacterGlow.mat
Shader "Towards the Unknown/Character Glow"
{
    Properties
    {
        [Header(Surface)]
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor ("Albedo Tint", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.3
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5

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
