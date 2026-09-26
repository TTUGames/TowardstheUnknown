// Blend Center Glow Fresnel: a center glow particle (Rendering/VFX/CenterGlow.hlsl), blended with its alpha, fading towards the faces turned to the view (NanukoPaw)
Shader "Towards the Unknown/VFX/Blend Center Glow Fresnel"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Flow("Flow", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        _SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
        _DistortionSpeedXYPowerZ("Distortion Speed XY Power Z", Vector) = (0,0,0,0)
        _Emission("Emission", Float) = 2
        _Color("Color", Color) = (0.5,0.5,0.5,1)
        _Opacity("Opacity", Range(0, 1)) = 1
        [Toggle]_Usecenterglow("Use center glow?", Float) = 0
        [Enum(Cull Off,0, Cull Front,1, Cull Back,2)] _CullMode("Culling", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull [_CullMode]
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VFXVert
            #pragma fragment CenterGlowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #define CENTERGLOW_FRESNEL
            #include "Assets/Rendering/VFX/CenterGlow.hlsl"
            ENDHLSL
        }
    }
}
