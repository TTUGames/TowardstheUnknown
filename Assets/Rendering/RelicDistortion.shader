// The space around a relic floating above the floor (a collectable), torn by its instability: drawn on a quad that faces
// the camera, centered on the orb, it reads the scene behind (the opaque texture) and bends it. A soft ripple always,
// a ring of void around the orb (darker, colder, drained), and when the relic glitches, in step with the orb: thin slices
// of the view slip sideways, the colors split and small blocks of void flash. The orb itself is spared (found by its
// depth), so that it all happens behind it. The color and the glitch clock come from RelicAura
Shader "Towards the Unknown/Relic Distortion"
{
    Properties
    {
        [HDR] _Color ("Rarity Color", Color) = (0.1, 0.8, 1, 1)
        _Radius ("Radius, in meters", Float) = 0.9
        _OrbRadius ("Orb Radius, share of the radius spared by the void", Range(0, 1)) = 0.3
        _OrbWorldRadius ("Orb Radius, in meters: the pixels this close to the orb's depth are the orb", Float) = 0.25
        _Ripple ("Ripple, in screen share", Range(0, 0.02)) = 0.004
        _Void ("Void, how dark and drained the ring gets", Range(0, 1)) = 0.35
        _VoidTint ("Void Tint, the rarity's share in it", Range(0, 1)) = 0.25
        _GlitchRate ("Glitch Slots per Second", Float) = 9
        _GlitchChance ("Glitch Chance per Slot", Range(0, 1)) = 0.18
        _SliceShift ("Glitch Slice Shift, in screen share", Range(0, 0.1)) = 0.012
        _Split ("Glitch Color Split, in screen share", Range(0, 0.02)) = 0.004
        _Blocks ("Glitch Void Blocks", Range(0, 1)) = 0.12
    }

    SubShader
    {
        // Before the other transparents (snow, air), which draw over it
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-60" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "RelicDistortion"
            Tags { "LightMode" = "UniversalForward" }
            Blend Off
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Radius;
                half _OrbRadius;
                float _OrbWorldRadius;
                float _Ripple;
                half _Void;
                half _VoidTint;
                float _GlitchRate;
                half _GlitchChance;
                float _SliceShift;
                float _Split;
                half _Blocks;
            CBUFFER_END

            #include "Relic.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 local : TEXCOORD0;
                float4 screen : TEXCOORD1;
                float2 seedGlitch : TEXCOORD2;
                float3 centerWS : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                // A quad facing the camera, around the object's center, of the radius in meters whatever the scale
                float2 local = input.uv * 2 - 1;
                float3 centerWS = GetObjectToWorldMatrix()._m03_m13_m23;
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;
                float3 positionWS = centerWS + (right * local.x + up * local.y) * _Radius;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.local = local;
                output.centerWS = centerWS;
                output.screen = ComputeScreenPos(output.positionCS);
                float seed = RelicSeed();
                output.seedGlitch = float2(seed, RelicGlitch(seed, _GlitchRate, _GlitchChance));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float seed = input.seedGlitch.x;
                half glitch = input.seedGlitch.y;
                float time = _Time.y;
                float2 uv = input.screen.xy / input.screen.w;
                float r = length(input.local);
                // The effect fades out to nothing at the quad's edge
                half falloff = 1 - smoothstep(0.35, 1, r);
                if (falloff <= 0) return half4(SampleSceneColor(uv), 1);

                // The orb's own pixels: inside its disc on screen and at its depth. Left untouched, it all happens behind it
                float3 forward = -UNITY_MATRIX_V[2].xyz;
                float3 surfaceWS = ComputeWorldSpacePosition(uv, SampleSceneDepth(uv), UNITY_MATRIX_I_VP);
                float depthGap = abs(dot(surfaceWS - input.centerWS, forward));
                if (r < _OrbRadius * 1.15 && depthGap < _OrbWorldRadius * 1.4) return half4(SampleSceneColor(uv), 1);

                // A soft ripple, always
                float2 warp = float2(ValueNoise(float3(input.local * 3, time * 0.9 + seed)), ValueNoise(float3(input.local * 3 + 5.2, time * 0.9 + seed))) - 0.5;
                uv += warp * _Ripple * falloff;

                // Glitch: horizontal slices of the view slip sideways
                float slot = floor(time * _GlitchRate);
                float slice = floor(input.local.y * 30 + Hash1(slot + seed) * 5);
                float slips = step(0.75, Hash1(slice * 7.3 + slot + seed));
                uv.x += (Hash1(slice * 3.1 + slot * 1.9 + seed) - 0.5) * 2 * _SliceShift * slips * glitch * falloff;

                // The colors split apart
                float split = (_Split * glitch + _Split * 0.15) * falloff;
                half3 color;
                color.r = SampleSceneColor(uv + float2(split, 0)).r;
                color.g = SampleSceneColor(uv).g;
                color.b = SampleSceneColor(uv - float2(split, 0)).b;

                // The void around the orb: darker, drained, a little of the rarity's color, breathing
                half ring = falloff * smoothstep(_OrbRadius * 0.9, _OrbRadius * 1.3, r);
                half breath = 0.85 + 0.15 * sin(time * 1.7 + seed);
                half luminance = dot(color, half3(0.299, 0.587, 0.114));
                half3 drained = lerp(luminance.xxx, luminance * _Color.rgb / max(max(_Color.r, _Color.g), max(_Color.b, 0.001)), _VoidTint);
                color = lerp(color, drained * (1 - _Void * 0.6), ring * _Void * breath);

                // Blocks of void flash while it glitches
                float2 block = floor(input.local * float2(22, 40));
                half flash = step(1 - _Blocks * 0.25, Hash1(dot(block, float2(1, 57)) + slot * 3.7 + seed)) * glitch * falloff;
                color = lerp(color, color * 0.35 + _Color.rgb * 0.04, flash * 0.7);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
