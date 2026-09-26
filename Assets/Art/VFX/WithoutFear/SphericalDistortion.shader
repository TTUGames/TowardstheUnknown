// WithoutFear's distortion: a sphere bending the scene seen through it (the opaque texture), the most at its rim, along
// its normal on screen and by a panning noise, tinted; optionally see through only at the rim. Ported from Amplify: its
// GrabPass, which URP does not run, reads the opaque texture instead. Same properties
Shader "Towards the Unknown/VFX/Spherical Distortion"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _FresnelPower("Fresnel Power", Float) = 2.49
        _FresnelScale("Fresnel Scale", Float) = 0.65
        _NoiseMap("Noise Map", 2D) = "white" {}
        _Distortion("Distortion", Range(-1, 1)) = 0
        _NoiseSpeed("Noise Speed", Vector) = (0,0,0,0)
        [Toggle]_UseFresnelOpacity("Use Fresnel Opacity", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VFXVert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Assets/Rendering/VFX/VFXParticles.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_NoiseMap); SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float _FresnelPower;
                float _FresnelScale;
                float4 _NoiseMap_ST;
                float _Distortion;
                float2 _NoiseSpeed;
                float _UseFresnelOpacity;
            CBUFFER_END

            half4 Frag(VFXVaryings input) : SV_Target
            {
                float fresnel = VFXFresnel(input, _FresnelScale, _FresnelPower);
                // The most at the rim, none in the middle nor at the very edge
                float rim = fresnel * (1 - fresnel);
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 bend = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS) - mul((float3x3)UNITY_MATRIX_V, viewWS));
                half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, VFXPan(input.uv0.xy, _NoiseMap_ST, _NoiseSpeed)).r;
                float2 offset = (noise * rim * _Distortion * input.color * float4(bend, 0)).xy;

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                half3 behind = SampleSceneColor(screenUV + offset);
                half alpha = saturate(input.color.a * (_UseFresnelOpacity > 0.5 ? rim : 1) * _TintColor.a);
                return VFXFog(half4(_TintColor.rgb * behind, alpha), input, false);
            }
            ENDHLSL
        }
    }
}
