// The mark on the floor of an open exit (RoomExit.prefab), the original's look drawn procedurally on one quad: a bright
// rim fading inwards over a faint fill, wisps of energy running along the rim, a soft glow outside, breathing slowly;
// optionally energy swirling inside (_Swirl) and ripples drawn towards the center (_Ripples). Tinted by _Color. Its UVs
// span -1 to 1 across the quad; the animation is offset by the exit's position, so that two exits don't move in step.
// _Reveal (set per renderer by ExitPortal) opens it: the rim springs out from the center with a flash and a shockwave,
// then the inside fills. _Hover (set by ExitPortal while the pointer is on the exit) brightens it, thickens the rim and
// draws ripples to its center, the way in. The streaks rising from the rim are RoomExitStreaks.shader
Shader "Towards the Unknown/Room Exit"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.35, 0.6, 2, 1)
        _Radius ("Rim Radius", Range(0.3, 1)) = 0.75
        _Width ("Rim Width (pixels)", Range(0.5, 10)) = 3
        _Core ("Rim Core Whiteness", Range(0, 1)) = 0.5
        _Inner ("Inner Gradient", Range(0, 2)) = 0.8
        _InnerFalloff ("Inner Gradient Falloff", Range(0.5, 8)) = 2.5
        _Fill ("Fill", Range(0, 1)) = 0.15
        _Cover ("Inner Cover", Range(0, 1)) = 0.35
        _Wisps ("Wisps", Range(0, 3)) = 1.5
        _WispWidth ("Wisp Width (pixels)", Range(0.5, 4)) = 1.6
        _WispSpeed ("Wisp Speed", Range(0, 3)) = 0.6
        _Swirl ("Inner Swirl", Range(0, 2)) = 0
        _SwirlScale ("Inner Swirl Scale", Range(1, 12)) = 4
        _SwirlSpeed ("Inner Swirl Speed", Range(0, 3)) = 0.5
        _Ripples ("Ripples", Range(0, 2)) = 0
        _RippleCount ("Ripple Count", Range(1, 4)) = 2
        _RippleSpeed ("Ripple Speed", Range(0, 2)) = 0.35
        _Shimmer ("Inner Shimmer", Range(0, 1)) = 0.5
        _Glow ("Outer Glow", Range(0, 2)) = 0.5
        _GlowWidth ("Outer Glow Width", Range(0.02, 0.4)) = 0.18
        _Breath ("Breath", Range(0, 1)) = 0.15
        _BreathSpeed ("Breath Speed", Range(0, 6)) = 1.6
        _HoverBoost ("Hover Boost", Range(0, 2)) = 0.6
        [Header(Set per renderer)]
        _Reveal ("Reveal", Range(0, 1)) = 1
        _Hover ("Hovered", Range(0, 1)) = 0
        _Fade ("Fade", Range(0, 1)) = 1
    }

    SubShader
    {
        // Over the grid (Transparent-1), like the entity rings
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "RoomExit"
            Tags { "LightMode" = "UniversalForward" }
            // Premultiplied: the rim covers the floor, the inside darkens it a little, the rest adds to it
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Noise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Radius;
                half _Width;
                half _Core;
                half _Inner;
                half _InnerFalloff;
                half _Fill;
                half _Cover;
                half _Wisps;
                half _WispWidth;
                half _WispSpeed;
                half _Swirl;
                half _SwirlScale;
                half _SwirlSpeed;
                half _Ripples;
                half _RippleCount;
                half _RippleSpeed;
                half _Shimmer;
                half _Glow;
                half _GlowWidth;
                half _Breath;
                half _BreathSpeed;
                half _HoverBoost;
                half _Reveal;
                half _Hover;
                half _Fade;
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
                float seed : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv * 2 - 1;
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                output.seed = Hash(float3(floor(origin.xz * 4) + 0.5, 0.5)) * 100;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            // A line of constant width in pixels around a distance
            half Line(float d, float target, float pixel, half widthPixels)
            {
                return saturate(widthPixels * 0.5 - abs(d - target) / pixel + 0.5);
            }

            // Overshoots a little before settling on 1
            float EaseOutBack(float x)
            {
                float c = 1.9;
                float y = x - 1;
                return 1 + (c + 1) * y * y * y + c * y * y;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float r = length(uv);
                float pixel = max(fwidth(r), 1e-5);
                float t = _Time.y + input.seed;
                // The direction around the rim, seamless for the noise
                float2 around = uv / max(r, 1e-4);

                // The reveal: the rim springs out over its first half, a flash and a shockwave at its peak, the inside
                // filling over the rest
                float open = saturate(_Reveal * 2.2);
                float radius = _Radius * max(EaseOutBack(open), 0.001);
                half flash = saturate(1 - abs(_Reveal - 0.42) / 0.3);
                flash *= flash;
                half settle = smoothstep(0.3, 1, _Reveal);
                half breath = 1 + _Breath * sin(t * _BreathSpeed) * settle;

                // The rim: a sharp line, whiter at its core
                half hover = smoothstep(0, 1, _Hover);
                half rim = Line(r, radius, pixel, _Width * (1 + flash) + hover * 1.5);
                half rimCore = Line(r, radius, pixel, max(_Width * 0.5, 0.75));
                half inside = step(r, radius);
                float rn = saturate(r / radius);
                // Inside: a gradient from the rim inwards over a faint fill
                half gradient = pow(rn, _InnerFalloff) * _Inner;
                half fill = _Fill;
                // Energy swirling inside, turning towards the center
                float swirlAngle = atan2(uv.y, uv.x) + (1 - rn) * 2.2 + t * _SwirlSpeed * 0.5;
                float2 swirlUV = float2(cos(swirlAngle), sin(swirlAngle)) * rn;
                half swirlNoise = ValueNoise(float3(swirlUV * _SwirlScale, t * _SwirlSpeed * 0.6));
                half swirlNoise2 = ValueNoise(float3(swirlUV * _SwirlScale * 2.1 + 5, t * _SwirlSpeed * 0.9));
                half swirl = smoothstep(0.45, 0.95, swirlNoise * 0.65 + swirlNoise2 * 0.35) * (0.3 + 0.7 * rn) * _Swirl;
                // Ripples drawn towards the center: thin lines leaving the rim, brightest midway
                half ripples = 0;
                [unroll]
                for (int k = 0; k < 4; k++)
                {
                    half active = step(k, _RippleCount - 0.5);
                    float rippleT = frac(t * _RippleSpeed + k / max(_RippleCount, 1));
                    float rippleR = radius * (1 - rippleT);
                    half rippleLine = Line(r, rippleR, pixel, 1.4) + saturate(1 - abs(r - rippleR) / 0.06) * 0.35;
                    ripples += rippleLine * sin(rippleT * PI) * active;
                }
                ripples *= _Ripples;
                // The hover's ripples, quicker, drawing the eye to the center
                [unroll]
                for (int h = 0; h < 2; h++)
                {
                    float hoverT = frac(_Time.y * 0.9 + h * 0.5);
                    float hoverR = radius * (1 - hoverT);
                    half hoverLine = Line(r, hoverR, pixel, 1.6) + saturate(1 - abs(r - hoverR) / 0.07) * 0.4;
                    ripples += hoverLine * sin(hoverT * PI) * hover * 0.9;
                }
                // A slow shimmer through the inside, so that it never reads flat
                half shimmer = lerp(1, 0.6 + 0.8 * ValueNoise(float3(uv * 3.5, t * 0.3)), _Shimmer);
                // Wisps of energy along the rim: two thin wavering lines, brightening in patches running around it
                half wisps = 0;
                [unroll]
                for (int i = 0; i < 2; i++)
                {
                    float speed = _WispSpeed * (i == 0 ? 1 : -0.7);
                    float wave = ValueNoise(float3(around * 2.5, t * 0.35 * (1 + i) + i * 13)) - 0.5;
                    float offset = radius * (0.95 - i * 0.04) + wave * 0.07;
                    half patch = smoothstep(0.35, 0.8, ValueNoise(float3(around * 3 + float2(t * speed, -t * speed), i * 7 + t * 0.2)));
                    wisps += Line(r, offset, pixel, _WispWidth) * patch * (1 - 0.35 * i);
                }
                wisps *= _Wisps;
                // A soft glow on both sides of the rim, wider outside
                half glowOut = saturate(1 - (r - radius) / _GlowWidth) * step(radius, r);
                half glowIn = saturate(1 - (radius - r) / (_GlowWidth * 0.6)) * inside;
                half glow = (glowOut * glowOut + glowIn * glowIn * 0.7) * _Glow * (1 + flash * 1.2);
                // The shockwave of the reveal, leaving the rim as it settles
                float waveRadius = _Radius * (0.8 + _Reveal * 0.55);
                half shock = Line(r, waveRadius, pixel, 2) * smoothstep(0.25, 0.45, _Reveal) * (1 - _Reveal) * (1 - _Reveal) * 3
                    + saturate(1 - abs(r - waveRadius) / 0.08) * smoothstep(0.25, 0.45, _Reveal) * (1 - _Reveal) * 0.6;
                // Faded out before the quad's edge
                half edge = saturate((1 - r) * 10);

                half shown = step(0.001, _Reveal) * _Fade;
                half rimAlpha = saturate(rim) * edge * shown;
                // Inside, the floor darkened a little under the light: the color stays its own over a bluish floor
                half alpha = max(rimAlpha, _Cover * inside * settle * (0.6 + 0.4 * rn) * edge * shown);
                half3 color = _Color.rgb * breath * (1 + flash * 0.7 + hover * _HoverBoost);
                half3 white = max(color, dot(color, 0.33).xxx * 1.8);
                half3 rimColor = lerp(color, white, saturate(rimCore * _Core + flash * 0.5));
                half insideAmount = ((gradient + fill) * shimmer + swirl + ripples) * inside * settle + flash * inside * (1 - rn * 0.5) * 0.35;
                half3 added = (color * (insideAmount + glow) + lerp(color, white, 0.4) * wisps * settle + color * shock) * edge * shown;
                half3 result = rimColor * rimAlpha + added * (1 - rimAlpha);
                result = MixFogColor(result, half3(0, 0, 0), input.fogFactor);
                return half4(result, alpha);
            }
            ENDHLSL
        }
    }
}
