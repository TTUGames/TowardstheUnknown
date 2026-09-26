// The wisps of mist drifting in the void under the tiles: large soft particles whose shape is a warped noise that keeps
// churning, so that a wisp changes shape as it drifts instead of sliding like a picture. Blended over the scene, cold,
// lit where a light of the room (with its cookie and shadows) crosses it, and faded where it meets the rock.
// The particle system passes its streams as TEXCOORD0: the UV, then Stable Random X and Age Percent
Shader "Towards the Unknown/Mist"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.08, 0.11, 0.16, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.35
        _NoiseScale ("Noise Scale, across a particle", Float) = 2.2
        _Warp ("Warp", Range(0, 3)) = 1.4
        _Churn ("Churn: how fast the shape changes", Range(0, 1)) = 0.06
        _Threshold ("Threshold: higher leaves thinner wisps", Range(0, 1)) = 0.38
        _Softness ("Edge Softness", Range(0.01, 1)) = 0.45
        _LightScatter ("Light Scattering", Range(0, 2)) = 0.5
        _SoftDistance ("Depth Fade Distance", Float) = 1.2
        _FadeTop ("Fade Top Height: none above", Float) = -0.6
        _FadeRange ("Fade Range", Float) = 1
    }

    SubShader
    {
        // After the water (Transparent-60), so that it drifts over the pools, and before the rift's air (Transparent-50),
        // which darkens it with the depth
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-55" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Mist"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Opacity;
                float _NoiseScale;
                half _Warp;
                float _Churn;
                half _Threshold;
                half _Softness;
                half _LightScatter;
                float _SoftDistance;
                float _FadeTop;
                float _FadeRange;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                // xy the UV, z Stable Random X, w Age Percent
                float4 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float4 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.color = input.color;
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float ValueNoise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x),
                                 lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                                 lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            float Fbm(float3 p)
            {
                return ValueNoise(p) * 0.55 + ValueNoise(p * 2.03 + 17.1) * 0.3 + ValueNoise(p * 4.07 + 41.3) * 0.15;
            }

            // The eye depth of the scene behind the pixel, in orthographic projection too
            float SceneEyeDepth(float2 screenUV)
            {
                float rawDepth = SampleSceneDepth(screenUV);
                if (unity_OrthoParams.w > 0.5)
                {
                    #if UNITY_REVERSED_Z
                    rawDepth = 1 - rawDepth;
                    #endif
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
                }
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            half3 LightAt(float3 positionWS)
            {
                half3 light = 0;
                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint i = 0; i < count; i++)
                {
                    Light additional = GetAdditionalLight(i, positionWS, half4(1, 1, 1, 1));
                    light += additional.color * additional.distanceAttenuation * additional.shadowAttenuation;
                }
                #endif
                return light;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 centered = input.uv.xy * 2 - 1;
                float random = input.uv.z;
                // Each particle reads its own part of the noise, and the noise keeps moving through its third axis
                float churn = _Time.y * _Churn + random * 13.7;
                float3 p = float3(centered * _NoiseScale + random * 71.3, churn);
                float3 warp = float3(Fbm(p * 0.6 + float3(5.2, 1.3, churn * 0.5)), Fbm(p * 0.6 + float3(9.2, 2.8, -churn * 0.4)), 0) * 2 - 1;
                half n = Fbm(p + warp * _Warp);

                // A round falloff, eaten by the noise: the wisp's edge is the noise's, never the quad's
                half radial = saturate(1 - dot(centered, centered));
                half density = saturate((n * radial - _Threshold * (1 - radial * 0.5)) / _Softness);
                density *= density;

                // Faded where it meets the rock and the tiles, and above the void
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy;
                float eyeDepth = -TransformWorldToView(input.positionWS).z;
                density *= saturate((SceneEyeDepth(screenUV) - eyeDepth) / _SoftDistance);
                density *= saturate((_FadeTop - input.positionWS.y) / _FadeRange);

                half alpha = saturate(density * _Opacity * input.color.a);
                half3 light = LightAt(input.positionWS);
                half3 color = _Color.rgb * input.color.rgb + light * _LightScatter * 0.1;
                color = MixFog(color, input.fogFactor);
                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
