// The ribbon a blade leaves behind during a swing (WeaponTrail): additive, brightest at the blade's edge and its tip,
// fading with the age of each slice. The mesh carries the age in uv.x (0 at the blade, 1 at the oldest slice) and the
// position across the blade in uv.y (0 at the hilt, 1 at the tip); WeaponTrail sets the color and its fade in _Color
Shader "Towards the Unknown/VFX/Weapon Trail"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (2, 2, 4, 1)
        _Core ("Core whiteness", Range(0, 1)) = 0.55
        _EdgeSharpness ("Edge sharpness", Range(1, 60)) = 18
        _Body ("Body", Range(0, 1)) = 0.6
        _Streaks ("Streaks", Range(0, 1)) = 0.35
        _StreakDensity ("Streak density", Float) = 14
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Trail"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Core;
                half _EdgeSharpness;
                half _Body;
                half _Streaks;
                half _StreakDensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fog : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half age = saturate(input.uv.x);
                half across = saturate(input.uv.y);
                // Fades with the slice's age, and towards the hilt; the tip, which moves fastest, carries the most
                half body = (1 - age) * (1 - age) * smoothstep(0, 0.45, across) * (0.35 + 0.65 * across);
                // The edge along the blade, where the steel just passed
                half edge = exp(-age * _EdgeSharpness) * smoothstep(0.1, 0.6, across);
                // Thin streaks along the swing
                half streak = 1 - _Streaks + _Streaks * (0.5 + 0.5 * sin(across * _StreakDensity * 6.2832 + age * 3));
                // The edge whitens towards the color's brightest channel: a hot core in the artifact's color
                half peak = max(max(_Color.r, _Color.g), _Color.b);
                half3 color = lerp(_Color.rgb, peak.xxx, saturate(edge * _Core));
                half amount = saturate(body * streak * _Body + edge) * _Color.a;
                color *= amount;
                color = MixFogColor(color, half3(0, 0, 0), input.fog);
                return half4(color, 0);
            }
            ENDHLSL
        }
    }
}
