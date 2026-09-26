// The orb of a relic floating above the floor (a collectable), unstable, as if torn between worlds: a dark, glassy sphere
// lit by the scene, holding a swirling nebula of its rarity's color seen at a depth and a bright core, split by rifts
// of light. Now and then it glitches: slices of it slip sideways and its colors fringe. An echo of it from another
// world, a faint ghost, trembles next to it and jumps when it glitches. It floats up and down slowly.
// The color comes from RelicAura (a property block)
Shader "Towards the Unknown/Relic Orb"
{
    Properties
    {
        [HDR] _Color ("Rarity Color", Color) = (0.1, 0.8, 1, 1)
        _Glow ("Glow", Range(0, 4)) = 1

        [Header(Shell)]
        [HDR] _ShellColor ("Shell Color", Color) = (0.015, 0.015, 0.025, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95

        [Header(Nebula)]
        _SwirlScale ("Swirl Scale", Float) = 2.4
        _SwirlSpeed ("Swirl Speed", Float) = 0.35
        _SwirlDepth ("Swirl Depth", Range(0, 1)) = 0.35
        _CorePower ("Core Tightness", Range(1, 16)) = 5
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5

        [Header(Rifts)]
        _RiftScale ("Rift Scale", Float) = 3.2
        _RiftWidth ("Rift Width", Range(0.005, 0.2)) = 0.05
        _RiftShare ("Rift Share, how much of the surface is torn", Range(0, 1)) = 0.45
        _RiftGlow ("Rift Glow", Range(0, 6)) = 2.2

        [Header(Instability)]
        _GlitchRate ("Glitch Slots per Second", Float) = 9
        _GlitchChance ("Glitch Chance per Slot", Range(0, 1)) = 0.12
        _GlitchShift ("Glitch Slice Shift, in object units", Float) = 0.18
        _SliceCount ("Glitch Slices", Float) = 9
        _Tremor ("Tremor, the surface's constant shiver", Range(0, 0.1)) = 0.025
        _GhostOffset ("Ghost Offset, in object units", Float) = 0.12
        _GhostStrength ("Ghost Strength", Range(0, 2)) = 0.35

        [Header(Float)]
        _BobHeight ("Bob Height, in object units", Float) = 0.08
        _BobSpeed ("Bob Speed", Float) = 1.4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Glow;
            half4 _ShellColor;
            half _Smoothness;
            float _SwirlScale;
            float _SwirlSpeed;
            float _SwirlDepth;
            half _CorePower;
            half _RimPower;
            float _RiftScale;
            float _RiftWidth;
            half _RiftShare;
            half _RiftGlow;
            float _GlitchRate;
            half _GlitchChance;
            float _GlitchShift;
            float _SliceCount;
            float _Tremor;
            float _GhostOffset;
            half _GhostStrength;
            float _BobHeight;
            float _BobSpeed;
        CBUFFER_END

        #include "Relic.hlsl"

        // The orb's surface, unstable: it shivers, its slices slip sideways when it glitches, and it floats
        float3 Unstable(float3 positionOS, float3 normalOS, float seed, float glitch)
        {
            float time = _Time.y;
            positionOS += normalOS * (ValueNoise(positionOS * 3 + time * 1.7 + seed) - 0.5) * _Tremor * 2;
            float slot = floor(time * _GlitchRate);
            float slice = floor((positionOS.y + 1) * _SliceCount * 0.5 + Hash1(slot + seed) * 3);
            float slips = step(0.5, Hash1(slice * 3.3 + slot + seed));
            float shift = (Hash1(slice * 13.1 + slot * 7.7 + seed) - 0.5) * 2;
            positionOS.x += shift * slips * glitch * _GlitchShift;
            positionOS.y += sin(time * _BobSpeed + seed) * _BobHeight;
            return positionOS;
        }

        // The echo from another world: the orb, drawn again beside itself, trembling, jumping when it glitches
        float3 GhostOffset(float seed, float glitch)
        {
            float slot = floor(_Time.y * _GlitchRate * 0.25);
            float3 direction = normalize(float3(Hash1(slot + seed) - 0.5, (Hash1(slot * 2.3 + seed) - 0.5) * 0.6, Hash1(slot * 5.1 + seed) - 0.5) + 0.0001);
            float drift = 0.5 + 0.5 * sin(_Time.y * 2.3 + seed);
            return direction * _GhostOffset * (0.4 + 0.6 * drift + glitch * 2);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                float2 seedGlitch : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Rifts: the edges of a 3D cell pattern, only over part of the surface
            half Rifts(float3 p, float seed)
            {
                float3 cell = floor(p);
                float3 f = frac(p);
                float first = 8, second = 8;
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    float3 offset = float3(x, y, z);
                    float3 site = offset + float3(Hash(cell + offset + seed), Hash(cell + offset + seed + 17.1), Hash(cell + offset + seed + 31.7)) - f;
                    float d = dot(site, site);
                    if (d < first) { second = first; first = d; }
                    else if (d < second) second = d;
                }
                return 1 - smoothstep(0, _RiftWidth, sqrt(second) - sqrt(first));
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float seed = RelicSeed();
                float glitch = RelicGlitch(seed, _GlitchRate, _GlitchChance);
                VertexPositionInputs position = GetVertexPositionInputs(Unstable(input.positionOS.xyz, input.normalOS, seed, glitch));
                output.positionCS = position.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                output.seedGlitch = float2(seed, glitch);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float seed = input.seedGlitch.x;
                half glitch = input.seedGlitch.y;
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(normalWS, viewWS));
                float time = _Time.y;

                // The nebula, looked up under the surface (towards the center, against the view) and turning slowly
                float3 viewOS = TransformWorldToObjectDir(viewWS);
                float angle = time * _SwirlSpeed;
                float2x2 turn = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
                float3 inner = input.positionOS - viewOS * _SwirlDepth;
                inner.xz = mul(turn, inner.xz);
                float3 p = inner * _SwirlScale + float3(0, time * _SwirlSpeed * 0.6, 0);
                float swirl = ValueNoise(p) * 0.6 + ValueNoise(p * 2.2 + 7.3) * 0.4;
                half filaments = pow(saturate(1 - abs(swirl - 0.5) * 5), 3);
                half haze = smoothstep(0.35, 0.8, swirl) * 0.35;

                // Rifts torn in the shell, slowly shifting, flaring when it glitches
                float3 riftPosition = input.positionOS * _RiftScale + float3(0, time * 0.05, 0);
                half torn = smoothstep(1 - _RiftShare - 0.08, 1 - _RiftShare + 0.08, ValueNoise(input.positionOS * 1.6 + seed + time * 0.04));
                half rifts = Rifts(riftPosition, seed) * torn;
                half flicker = 0.75 + 0.25 * sin(time * 7 + seed + input.positionOS.y * 5);

                half core = pow(facing, _CorePower);
                half rim = pow(1 - facing, _RimPower);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = _ShellColor.rgb;
                surface.smoothness = _Smoothness;
                surface.occlusion = 1;
                surface.alpha = 1;
                surface.normalTS = half3(0, 0, 1);

                half inside = (filaments * 0.9 + haze) * (0.25 + 0.75 * facing) + core * 0.45;
                half3 emission = _Color.rgb * (inside + rim * 0.8 + rifts * _RiftGlow * flicker * (1 + glitch * 2));
                // When it glitches, its colors fringe: the rim splits into shifted hues, and screen bands flash
                half3 fringe = _Color.gbr * pow(1 - facing, _RimPower * 0.6) * glitch * 0.5;
                half band = step(0.6, Hash1(floor(inputData.normalizedScreenSpaceUV.y * 60) + floor(time * _GlitchRate) + seed)) * glitch;
                surface.emission = (emission * (1 + band * 0.8) + fringe) * _Glow;

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            // The echo from another world, drawn after the orb: only its glowing rim, faint and flickering
            Name "Ghost"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One One
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                float2 seedGlitch : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float seed = RelicSeed();
                float glitch = RelicGlitch(seed, _GlitchRate, _GlitchChance);
                float3 positionOS = Unstable(input.positionOS.xyz, input.normalOS, seed + 5, glitch) + GhostOffset(seed, glitch);
                VertexPositionInputs position = GetVertexPositionInputs(positionOS);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                output.seedGlitch = float2(seed, glitch);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float seed = input.seedGlitch.x;
                half glitch = input.seedGlitch.y;
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half rim = pow(1 - saturate(dot(normalWS, viewWS)), 2);
                half flicker = step(0.3, Hash1(floor(_Time.y * 14) + seed)) * (0.6 + 0.4 * sin(_Time.y * 31 + seed));
                half3 color = _Color.gbr * 0.35 + _Color.rgb * 0.65;
                color *= rim * _GhostStrength * _Glow * flicker * (1 + glitch * 2);
                color = MixFogColor(color, half3(0, 0, 0), input.fogFactor);
                return half4(color, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float seed = RelicSeed();
                float3 positionWS = TransformObjectToWorld(Unstable(input.positionOS.xyz, input.normalOS, seed, RelicGlitch(seed, _GlitchRate, _GlitchChance)));
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirection = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirection = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float seed = RelicSeed();
                return TransformObjectToHClip(Unstable(input.positionOS.xyz, input.normalOS, seed, RelicGlitch(seed, _GlitchRate, _GlitchChance)));
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                float seed = RelicSeed();
                output.positionCS = TransformObjectToHClip(Unstable(input.positionOS.xyz, input.normalOS, seed, RelicGlitch(seed, _GlitchRate, _GlitchChance)));
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
