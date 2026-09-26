// DragonStrike's aura: a texture bent by two panning noises (multiplied, mixed or added) along an axis, the bend growing
// with the custom data (uv0.z) and optionally along the texture; colored through a ramp, faded along a gradient of the
// uv and, optionally, eaten by the noise as uv0.w and the opacity grow. Ported from Amplify, same properties and keywords
Shader "Towards the Unknown/VFX/Dragon Strike Aura"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _MainTilingOffset("Main Tiling Offset", Vector) = (1,1,0,0)
        _Noise1Texture("Noise 1 Texture", 2D) = "white" {}
        _Noise2Texture("Noise 2 Texture", 2D) = "white" {}
        _Speed("Speed", Vector) = (0,0,0,0)
        _MaskTextureOffset("Mask Texture Offset", Float) = 0
        [KeywordEnum(Multiply,Lerp,Add)] _NoiseBlend("NoiseBlend", Float) = 0
        _Adjust("Adjust", Range(0, 1)) = 0
        _SmoothstepMin("Smoothstep Min", Range(0, 1)) = 0
        _SmoothstepMax("Smoothstep Max", Range(0, 1)) = 1
        [KeywordEnum(U,V)] _GradientUV("Gradient UV", Float) = 0
        _UVGradientOffset("UV Gradient Offset", Range(-1, 1)) = 0
        _UVGradientAdjust("UV Gradient Adjust", Range(0, 1)) = 0
        [KeywordEnum(Noise1,Noise2,BlendNoise)] _NoiseOpacity("NoiseOpacity", Float) = 0
        _Opacity("Opacity", Range(0, 1)) = 0
        [Toggle(_USENOISEUVGRADIENT_ON)] _UseNoiseUVGradient("Use Noise UV Gradient", Float) = 0
        _RampMap("Ramp Map", 2D) = "white" {}
        [KeywordEnum(U,V)] _MaskTextureOffsetAxis("Mask Texture Offset Axis", Float) = 0
        [Toggle(_USENOISEOPACITY_ON)] _UseNoiseOpacity("Use Noise Opacity", Float) = 1
        [Toggle(_BLENDTEXTURES_ON)] _BlendTextures("Blend Textures", Float) = 0
        [Toggle(_INVERTGRADIENT_ON)] _InvertGradient("Invert Gradient", Float) = 0
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
            #pragma shader_feature_local _BLENDTEXTURES_ON
            #pragma shader_feature_local _NOISEBLEND_MULTIPLY _NOISEBLEND_LERP _NOISEBLEND_ADD
            #pragma shader_feature_local _MASKTEXTUREOFFSETAXIS_U _MASKTEXTUREOFFSETAXIS_V
            #pragma shader_feature_local _USENOISEUVGRADIENT_ON
            #pragma shader_feature_local _INVERTGRADIENT_ON
            #pragma shader_feature_local _GRADIENTUV_U _GRADIENTUV_V
            #pragma shader_feature_local _USENOISEOPACITY_ON
            #pragma shader_feature_local _NOISEOPACITY_NOISE1 _NOISEOPACITY_NOISE2 _NOISEOPACITY_BLENDNOISE

            #include "Assets/Rendering/VFX/VFXParticles.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Noise1Texture); SAMPLER(sampler_Noise1Texture);
            TEXTURE2D(_Noise2Texture); SAMPLER(sampler_Noise2Texture);
            TEXTURE2D(_RampMap); SAMPLER(sampler_RampMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float4 _MainTilingOffset;
                float4 _Noise1Texture_ST;
                float4 _Noise2Texture_ST;
                float4 _Speed;
                float _MaskTextureOffset;
                half _Adjust;
                half _SmoothstepMin;
                half _SmoothstepMax;
                half _UVGradientOffset;
                half _UVGradientAdjust;
                half _Opacity;
            CBUFFER_END

            half4 Frag(VFXVaryings input) : SV_Target
            {
                float4 uv = input.uv0;
                half noise1 = SAMPLE_TEXTURE2D(_Noise1Texture, sampler_Noise1Texture, VFXPan(uv.xy, _Noise1Texture_ST, _Speed.xy)).r;
                half noise2 = SAMPLE_TEXTURE2D(_Noise2Texture, sampler_Noise2Texture, VFXPan(uv.xy, _Noise2Texture_ST, _Speed.zw)).r;
                #if defined(_NOISEBLEND_LERP)
                    half noise = lerp(noise1, noise2, 0.5);
                #elif defined(_NOISEBLEND_ADD)
                    half noise = noise1 + noise2;
                #else
                    half noise = noise1 * noise2;
                #endif

                // The bend, along the chosen axis, growing with the custom data and optionally along the other uv
                float offset = uv.z + _MaskTextureOffset;
                #if defined(_MASKTEXTUREOFFSETAXIS_V)
                    float2 axis = float2(offset, 0);
                    float along = uv.x;
                #else
                    float2 axis = float2(0, offset);
                    float along = uv.y;
                #endif
                #if !defined(_USENOISEUVGRADIENT_ON)
                    along = 1;
                #endif
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy * _MainTilingOffset.xy + _MainTilingOffset.zw + noise * axis * along);
                #if defined(_BLENDTEXTURES_ON)
                    main = (main.r + smoothstep(_SmoothstepMin, _SmoothstepMax, saturate(main.r - _Adjust))).xxxx;
                #endif
                half4 color = input.color * _TintColor * SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(main.r, 0)) * main;

                #if defined(_GRADIENTUV_V)
                    half gradient = uv.y;
                #else
                    half gradient = uv.x;
                #endif
                #if defined(_INVERTGRADIENT_ON)
                    gradient = 1 - gradient;
                #endif
                gradient = smoothstep(0, _UVGradientAdjust, lerp(_UVGradientOffset, 1, gradient));

                #if defined(_USENOISEOPACITY_ON)
                    #if defined(_NOISEOPACITY_NOISE2)
                        half eaten = noise2;
                    #elif defined(_NOISEOPACITY_BLENDNOISE)
                        half eaten = noise;
                    #else
                        half eaten = noise1;
                    #endif
                    half kept = step(saturate(eaten), 1 - (_Opacity + uv.w));
                #else
                    half kept = 1;
                #endif

                return VFXFog(half4(color.rgb, saturate(color.a * gradient * kept)), input, false);
            }
            ENDHLSL
        }
    }
}
