// The thin grid drawn over the walkable tiles during a combat (CombatGrid): a quad per tile, whose borders are drawn at a
// constant width in pixels. UV1 holds each border's share of the line (1 alone, 0.5 shared with the neighbouring tile)
Shader "Towards the Unknown/Combat Grid"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.75, 0.9, 1, 0.35)
        _Thickness ("Line Width (pixels)", Range(0.5, 6)) = 1.5
        _Fade ("Fade", Range(0, 1)) = 1
    }

    SubShader
    {
        // Under the selection overlays (Transparent) but after the rift's air (Transparent-50), which would cover it
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-1" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "CombatGrid"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            // Over the snow dusting the tiles
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Thickness;
                half _Fade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 borders : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 borders : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.borders = input.borders;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            // Coverage of a line of the given width in pixels, starting at the border and going into the tile
            half Line(float distancePixels, float share)
            {
                return saturate(_Thickness * share - distancePixels + 0.5);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = max(fwidth(input.uv), 1e-5);
                float2 fromMin = input.uv / pixel;
                float2 fromMax = (1 - input.uv) / pixel;
                half coverage = max(
                    max(Line(fromMin.x, input.borders.x), Line(fromMax.x, input.borders.y)),
                    max(Line(fromMin.y, input.borders.z), Line(fromMax.y, input.borders.w)));

                half alpha = coverage * _Color.a * _Fade;
                clip(alpha - 0.001);
                half3 color = MixFog(_Color.rgb, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
