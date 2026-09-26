// Creature of the rift: a dark lit flesh crossed by flowing veins of energy, with a rim and an aura of the same energy
// flickering around the silhouette (EnemyEnergy.hlsl)
// Used by the enemies' materials (Art/Materials/Enemies), variants of EnemyEnergy.mat
Shader "Towards the Unknown/Enemy Energy"
{
    Properties
    {
        [Header(Flesh)]
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor ("Albedo Tint", Color) = (0.06, 0.05, 0.06, 1)
        [NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Float) = 1
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.35

        [Header(Energy)]
        _GlowColor ("Energy Color", Color) = (0.91, 0.165, 0.396, 1)
        _GlowIntensity ("Energy Intensity, in stops", Range(-4, 8)) = 2
        _GlowMultiplier ("Energy Multiplier, set at runtime", Float) = 1
        [NoScaleOffset] _GlowMask ("Glow Mask (R), marks lit whole", 2D) = "black" {}
        _MaskStrength ("Mask Strength", Range(0, 2)) = 1

        [Header(Veins)]
        _VeinScale ("Vein Density, per meter", Float) = 4
        _VeinSharpness ("Vein Thinness", Range(1, 128)) = 40
        _VeinAmount ("Vein Brightness", Range(0, 2)) = 1
        _VeinFlow ("Vein Flow, meters per second upwards", Float) = 0.25
        _VeinCoverage ("Vein Coverage, the part of the body they surface on", Range(0, 1)) = 0.4
        _VeinDarken ("Flesh Burnt Around The Veins", Range(0, 1)) = 0.6

        [Header(Rim)]
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.08
        _RimPower ("Rim Power", Range(0.5, 8)) = 4

        [Header(Pulse)]
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.5
        _PulseSpeed ("Pulse Speed", Float) = 3
        _PulseFrequency ("Pulse Waves, per meter of height", Float) = 4

        [Header(Aura)]
        _AuraWidth ("Aura Width, in meters", Range(0, 0.5)) = 0.05
        _AuraIntensity ("Aura Intensity, in stops", Range(-4, 8)) = 0.5
        _AuraNoiseScale ("Aura Flame Size, per meter", Float) = 6
        _AuraSpeed ("Aura Rise, meters per second", Float) = 0.8
        _AuraSoftness ("Aura Edge Softness", Range(0.01, 1)) = 0.35
        _AuraCoverage ("Aura Coverage", Range(0, 1)) = 0.55
    }

    SubShader
    {
        // Drawn after the opaque Geometry queue (still opaque): the aura writes no depth, and the board drawn after it
        // would cover it
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+475" }

        HLSLINCLUDE
        #include "EnemyEnergy.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex EnergyVert
            #pragma fragment EnergyFrag
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
            #include "EnemyEnergy.hlsl"
            ENDHLSL
        }

        // The aura: an extra unlit pass of the same draw, additive, no depth written, its back faces only (EnemyEnergy.hlsl)
        Pass
        {
            Name "Aura"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One One
            ZWrite Off
            Cull Front

            HLSLPROGRAM
            #pragma vertex AuraVert
            #pragma fragment AuraFrag
            #pragma multi_compile_instancing
            #define ENEMY_ENERGY_AURA
            #include "EnemyEnergy.hlsl"
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
