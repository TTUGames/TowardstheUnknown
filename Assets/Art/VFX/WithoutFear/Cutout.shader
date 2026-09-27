// WithoutFear's aligned cutout: a texture to a power, tinted, cut along one axis of the uv by a panning noise, the cut
// moving with the custom data (uv0.z). Ported from Amplify, same properties and keywords
Shader "Towards the Unknown/VFX/Cutout Alignment"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _NoiseTex("Noise Tex", 2D) = "white" {}
        _AlphaCutout("Alpha Cutout", Float) = 0
        _TexPower("Tex Power", Float) = 1
        _NoiseSpeed("Noise Speed", Vector) = (0,0,0,0)
        _MaskClipValue("Mask Clip Value", Float) = 0.5
        _NoiseMin("Noise Min", Float) = 0
        _NoiseMax("Noise Max", Float) = 1
        [KeywordEnum(X,Y)] _Alignment("Alignment", Float) = 0
        _Size("Size", Float) = 1
        _NoisePower("Noise Power", Float) = 1
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
            #pragma shader_feature_local _ALIGNMENT_X _ALIGNMENT_Y

            #include "Assets/Rendering/VFX/VFXParticles.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float2 _NoiseSpeed;
                float _AlphaCutout;
                float _TexPower;
                float _MaskClipValue;
                float _NoiseMin;
                float _NoiseMax;
                float _Size;
                float _NoisePower;
            CBUFFER_END

            half4 Frag(VFXVaryings input) : SV_Target
            {
                float4 uv = input.uv0;
                #if defined(_ALIGNMENT_Y)
                    float along = uv.y;
                #else
                    float along = uv.x;
                #endif
                float distance = abs(along + (_AlphaCutout + uv.z)) - _Size;
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, VFXPan(uv.xy, _NoiseTex_ST, _NoiseSpeed)).r;
                half mask = (noise * _NoisePower - smoothstep(_NoiseMin, _NoiseMax, distance)) * saturate(1 - distance);
                clip(saturate(mask) - _MaskClipValue);

                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                half4 color = pow(abs(main), _TexPower) * _TintColor * input.color;
                return VFXFog(color, input, false);
            }
            ENDHLSL
        }
    }
}
