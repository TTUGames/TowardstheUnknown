// Burning logs, low poly: flat-shaded wood lit by the main light and the ambient, whose charred parts glow like embers.
// The mesh's vertex color red is how burnt the wood is there (0 bare wood, 1 ember); the embers breathe with noise
Shader "Towards the Unknown/Ember Wood"
{
    Properties
    {
        _WoodColor ("Wood", Color) = (0.32, 0.19, 0.11, 1)
        _CharColor ("Char", Color) = (0.06, 0.04, 0.035, 1)
        [HDR] _EmberColor ("Ember", Color) = (3.2, 0.75, 0.12, 1)
        _EmberStart ("Ember Start", Range(0, 1)) = 0.55
        _Pulse ("Pulse", Range(0, 1)) = 0.45
        _PulseSpeed ("Pulse Speed", Range(0, 6)) = 1.6
        _PulseScale ("Pulse Scale", Range(1, 40)) = 14
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "EmberWood"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Noise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _WoodColor;
                half4 _CharColor;
                half4 _EmberColor;
                half _EmberStart;
                half _Pulse;
                half _PulseSpeed;
                half _PulseScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
                half burn : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.positionOS = input.positionOS.xyz;
                output.burn = input.color.r;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(cross(ddy(input.positionWS), ddx(input.positionWS)));
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = saturate(dot(normal, light.direction)) * light.shadowAttenuation;
                half3 lighting = light.color * diffuse + SampleSH(normal);
                // The flame's own light, and the others around
                #if defined(_ADDITIONAL_LIGHTS)
                    InputData inputData = (InputData)0;
                    inputData.positionWS = input.positionWS;
                    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                    uint count = GetAdditionalLightsCount();
                    #if USE_CLUSTER_LIGHT_LOOP
                    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                    {
                        Light additional = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                        lighting += additional.color * additional.distanceAttenuation * additional.shadowAttenuation * saturate(dot(normal, additional.direction));
                    }
                    #endif
                    LIGHT_LOOP_BEGIN(count)
                        Light additional = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                        lighting += additional.color * additional.distanceAttenuation * additional.shadowAttenuation * saturate(dot(normal, additional.direction));
                    LIGHT_LOOP_END
                #endif
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    lighting *= GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(input.positionCS)).indirectAmbientOcclusion;
                #endif

                half3 albedo = lerp(_WoodColor.rgb, _CharColor.rgb, smoothstep(0.25, 0.7, input.burn));
                half3 color = albedo * lighting;

                // Embers: the most burnt wood glows, in patches that breathe
                half patches = ValueNoise(input.positionOS * _PulseScale + float3(0, _Time.y * _PulseSpeed, 0));
                half ember = smoothstep(_EmberStart, 1, input.burn + (patches - 0.5) * _Pulse);
                color += _EmberColor.rgb * ember;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
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
            #pragma vertex ShadowVert
            #pragma fragment DepthFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_instancing
            #include "DepthPasses.hlsl"
            ENDHLSL
        }
    }
}
