// The ring under an entity during a combat (EntityRing): a thin sharp ring over a soft halo, blue for the player and red
// for the enemies. It pulses on the entity's turn, brightens while hovered, turns to the target color while an artifact
// aims at it. _Fade, _Active, _Hover and _Targeted are set per renderer
Shader "Towards the Unknown/Entity Ring"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.35, 0.7, 1.6, 1)
        [HDR] _TargetColor ("Targeted Color", Color) = (2.4, 0.35, 0.25, 1)
        _Radius ("Ring Radius", Range(0.3, 1)) = 0.82
        _Width ("Ring Width (pixels)", Range(0.5, 8)) = 2
        _Opacity ("Ring Opacity", Range(0, 1)) = 0.9
        _Halo ("Inner Halo", Range(0, 1)) = 0.3
        _HaloFalloff ("Inner Halo Falloff", Range(0.5, 8)) = 2.5
        _Glow ("Outer Glow", Range(0, 1)) = 0.25
        _GlowWidth ("Outer Glow Width", Range(0.01, 0.5)) = 0.14
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.5
        _PulseStrength ("Pulse Strength", Range(0, 2)) = 0.7
        _HoverBoost ("Hover Boost", Range(0, 2)) = 0.8
        [Header(Set per renderer)]
        _Fade ("Fade", Range(0, 1)) = 1
        _Active ("Active Turn", Range(0, 1)) = 0
        _Hover ("Hovered", Range(0, 1)) = 0
        _Targeted ("Targeted", Range(0, 1)) = 0
    }

    SubShader
    {
        // Over the grid (Transparent-1) and the selection overlays (Transparent)
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "EntityRing"
            Tags { "LightMode" = "UniversalForward" }
            // Premultiplied: the ring covers the ground, the glow adds to it
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _TargetColor;
                half _Radius;
                half _Width;
                half _Opacity;
                half _Halo;
                half _HaloFalloff;
                half _Glow;
                half _GlowWidth;
                half _PulseSpeed;
                half _PulseStrength;
                half _HoverBoost;
                half _Fade;
                half _Active;
                half _Hover;
                half _Targeted;
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
                half fogFactor : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv * 2 - 1;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float r = length(input.uv);
                float pixel = max(fwidth(r), 1e-5);

                // 0 to 1 and back, only on the entity's turn
                half pulse = _Active * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));
                half intensity = 1 + _Active * 0.35 + pulse * _PulseStrength + _Hover * _HoverBoost;
                half3 color = lerp(_Color.rgb, _TargetColor.rgb, _Targeted) * intensity;

                // The sharp ring, a constant width in pixels
                half ring = saturate(_Width * 0.5 - abs(r - _Radius) / pixel + 0.5);
                // The soft halo inside, brightest against the ring
                half inside = saturate(r / _Radius);
                half halo = pow(inside, _HaloFalloff) * step(r, _Radius) * _Halo;
                // A faint glow outside, wider while the ring pulses
                half glowWidth = _GlowWidth * (1 + pulse * 0.6);
                half glow = saturate(1 - (r - _Radius) / glowWidth) * step(_Radius, r) * _Glow;
                // Faded out before the edge of the quad
                glow *= glow * saturate((1 - r) * 20);

                half ringAlpha = ring * _Opacity * _Fade;
                half3 added = color * (halo + glow) * (1 + _Hover * 0.5) * _Fade;
                half3 result = color * ringAlpha + added * (1 - ringAlpha);
                result = MixFogColor(result, half3(0, 0, 0), input.fogFactor);
                return half4(result, ringAlpha);
            }
            ENDHLSL
        }
    }
}
