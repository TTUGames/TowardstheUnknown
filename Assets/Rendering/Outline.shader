// Used by OutlineFeature: pass 0 draws the outlined renderers in the mask, pass 1 draws the outline around the mask
Shader "Hidden/Outline"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "Mask"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half frag() : SV_Target
            {
                return 1;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #define DIRECTIONS 16

            half4 _OutlineColor;
            float2 _OutlineStep; // outline width in UV

            half Mask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).r;
            }

            // Covered when a point of the mask lies within the outline width: samples two rings around the pixel
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half coverage = 0;
                [unroll]
                for (int i = 0; i < DIRECTIONS; i++)
                {
                    float angle = i * (TWO_PI / DIRECTIONS);
                    float2 offset = float2(cos(angle), sin(angle)) * _OutlineStep;
                    coverage = max(coverage, max(Mask(uv + offset), Mask(uv + offset * 0.5)));
                }
                return half4(_OutlineColor.rgb, _OutlineColor.a * coverage * (1 - Mask(uv)));
            }
            ENDHLSL
        }
    }
}
