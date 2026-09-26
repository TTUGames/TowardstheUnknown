// WithoutFear's glow: a panning texture, glowing color in the middle turning to the tint towards the edges (fresnel),
// eaten by a noise (read with the second uv) as the custom data grows (uv0.z). Ported from Amplify, same properties
Shader "Towards the Unknown/VFX/Glow Cutout"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _FresnelScale("Fresnel Scale", Range(0, 1)) = 0.510905
        _FresnelPower("Fresnel Power", Range(0, 5)) = 2
        _NoiseTex("Noise Tex", 2D) = "white" {}
        _AlphaCutout("Alpha Cutout", Float) = 0
        _MaskClipValue("Mask Clip Value", Float) = 0.5
        _NoiseAdjust("Noise Adjust", Range(0, 1)) = 1
        [HDR]_GlowColor("Glow Color", Color) = (0,0,0,0)
        _MainSpeed("Main Speed", Vector) = (0,0,0,0)
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

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half _FresnelScale;
                half _FresnelPower;
                float4 _NoiseTex_ST;
                float _AlphaCutout;
                float _MaskClipValue;
                half _NoiseAdjust;
                half4 _GlowColor;
                float2 _MainSpeed;
            CBUFFER_END

            half4 Frag(VFXVaryings input) : SV_Target
            {
                float4 uv = input.uv0;
                half fresnel = saturate(VFXFresnel(input, _FresnelScale, _FresnelPower));
                half4 tint = lerp(_GlowColor, _TintColor * input.color * fresnel, fresnel);
                half alpha = lerp(_GlowColor.a, _TintColor.a, fresnel);

                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.uv1.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw).r;
                half kept = smoothstep(0, _NoiseAdjust, noise - (_AlphaCutout + uv.z));
                clip(kept - _MaskClipValue);

                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy + _Time.y * _MainSpeed);
                return VFXFog(half4((main * tint).rgb, input.color.a * alpha * kept), input, false);
            }
            ENDHLSL
        }
    }
}
