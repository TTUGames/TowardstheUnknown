// DragonStrike's ground wave: a border texture plus a texture scrolling with the custom data (uv0.z), tinted, fading out
// along the uv's v as the second custom data grows (uv3.w). Ported from Amplify, same properties
Shader "Towards the Unknown/VFX/Dragon Strike Ground"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _GradientStep("Gradient Step", Float) = -0.1
        _BorderTexture("Border Texture", 2D) = "white" {}
        _MainTiling("Main Tiling", Vector) = (0,0,0,0)
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
            TEXTURE2D(_BorderTexture); SAMPLER(sampler_BorderTexture);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float4 _BorderTexture_ST;
                float2 _MainTiling;
                float _GradientStep;
            CBUFFER_END

            half4 Frag(VFXVaryings input) : SV_Target
            {
                float4 uv = input.uv0;
                half border = SAMPLE_TEXTURE2D(_BorderTexture, sampler_BorderTexture, uv.xy * _BorderTexture_ST.xy + _BorderTexture_ST.zw).r;
                half main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy * _MainTiling + float2(0, uv.z)).r;
                half4 color = _TintColor * (border + main) * input.color;
                half fade = saturate(smoothstep(0, _GradientStep, 1 - (uv.y + input.uv3.w)));
                return VFXFog(half4(color.rgb, saturate(color.a * fade)), input, false);
            }
            ENDHLSL
        }
    }
}
