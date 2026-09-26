// DragonStrike's wind: a panning texture cut by the custom data (uv0.z) and the cutout, tinted, faded at both ends of
// the uv's v. Ported from Amplify, same properties
Shader "Towards the Unknown/VFX/Dragon Strike Wind"
{
    Properties
    {
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _Speed("Speed", Vector) = (0,0,0,0)
        _UVGradientOffset("UV Gradient Offset", Float) = 0
        _MainTilingOffset("Main Tiling Offset", Vector) = (1,1,0,0)
        _Cutout("Cutout", Float) = 0
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

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float4 _Speed;
                float _UVGradientOffset;
                float4 _MainTilingOffset;
                float _Cutout;
            CBUFFER_END

            half4 Frag(VFXVaryings input) : SV_Target
            {
                float4 uv = input.uv0;
                half gradient = saturate(lerp(_UVGradientOffset, 1, 1 - uv.y) * lerp(_UVGradientOffset, 1, uv.y) * 4);
                half main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, VFXPan(uv.xy, _MainTilingOffset, _Speed.xy)).r;
                half4 color = _TintColor * input.color * gradient * saturate(main - (uv.z + _Cutout));
                return VFXFog(half4(color.rgb, saturate(color.a)), input, false);
            }
            ENDHLSL
        }
    }
}
