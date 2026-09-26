// Dissolve Noise Paw: a dissolving particle (Rendering/VFX/DissolveNoise.hlsl), for NanukoPaw
Shader "Towards the Unknown/VFX/Dissolve Noise Paw"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _TextureNoise("Texture Noise", 2D) = "white" {}
        _Dissolvenoise("Dissolve noise", 2D) = "white" {}
        _NoisespeedXYEmissonZPowerW("Noise speed XY / Emisson Z / Power W", Vector) = (0.5,0,2,1)
        _DissolvespeedXY("Dissolve speed XY", Vector) = (0,0,0,0)
        _Maincolor("Main color", Color) = (0.7609469,0.8547776,0.9433962,1)
        _Noisecolor("Noise color", Color) = (0.2470588,0.3012382,0.3607843,1)
        _Dissolvecolor("Dissolve color", Color) = (1,1,1,1)
        [Toggle]_Usetexturecolor("Use texture color", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VFXVert
            #pragma fragment DissolveNoiseFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #include "Assets/Rendering/VFX/DissolveNoise.hlsl"
            ENDHLSL
        }
    }
}
