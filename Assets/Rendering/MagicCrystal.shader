// Glowing magic crystal: a dark, glossy facetted shell lit by the scene (sharp highlights on the facets) over an inner glow
// that brightens towards the tips, with veins of light seen through the surface at a depth (a parallax), a rim of light,
// facets that shimmer in turn and a slow pulse, each crystal at its own pace. The emission is HDR, for the bloom
// Used by the LowPolyCavePack crystals
Shader "Towards the Unknown/Magic Crystal"
{
    Properties
    {
        [Header(Shell)]
        [HDR] _ShellColor ("Shell Color", Color) = (0.02, 0.05, 0.16, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.92

        [Header(Inner Glow)]
        [HDR] _DeepColor ("Deep Color, at the base", Color) = (0.05, 0.12, 0.9, 1)
        [HDR] _CoreColor ("Core Color, at the tips", Color) = (0.35, 1.6, 3.2, 1)
        _GradientHeight ("Base to tip height, in object units", Float) = 3
        _GlowStrength ("Glow Strength", Range(0, 4)) = 1

        [Header(Veins)]
        [HDR] _VeinColor ("Vein Color", Color) = (0.6, 1.8, 4, 1)
        _VeinScale ("Vein Scale", Float) = 1.8
        _VeinDepth ("Vein Depth, in object units", Range(0, 1)) = 0.35
        _VeinSpeed ("Vein Drift Speed", Float) = 0.12

        [Header(Rim and Facets)]
        [HDR] _RimColor ("Rim Color", Color) = (0.7, 0.55, 3, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _FacetShimmer ("Facet Shimmer", Range(0, 1)) = 0.45
        _FacetSpeed ("Facet Shimmer Speed", Float) = 1.3

        [Header(Pulse)]
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.25
        _PulseSpeed ("Pulse Speed", Float) = 0.8
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _ShellColor;
            half _Smoothness;
            half4 _DeepColor;
            half4 _CoreColor;
            float _GradientHeight;
            half _GlowStrength;
            half4 _VeinColor;
            float _VeinScale;
            float _VeinDepth;
            float _VeinSpeed;
            half4 _RimColor;
            half _RimPower;
            half _FacetShimmer;
            float _FacetSpeed;
            half _PulseAmount;
            float _PulseSpeed;
        CBUFFER_END

        #include "Noise.hlsl"

        // Thin bright lines where the noise crosses its middle: the veins of light inside the crystal
        float Veins(float3 p)
        {
            float n = ValueNoise(p) * 0.65 + ValueNoise(p * 2.3 + 11.7) * 0.35;
            return pow(saturate(1 - abs(n - 0.5) * 9), 3);
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                // The facets are flat: their normal is the same over the face, a stable seed for each of them
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(normalWS, viewWS));

                // Each crystal pulses at its own pace, seeded by its position
                float3 objectWS = GetObjectToWorldMatrix()._m03_m13_m23;
                float seed = Hash(floor(objectWS * 3.7) + 0.5) * 6.2831;
                half pulse = 1 + sin(_Time.y * _PulseSpeed + seed) * _PulseAmount;

                // Inner glow: deep at the base, bright at the tips, stronger where the crystal is seen through (facing the view)
                half height = saturate(input.positionOS.y / max(_GradientHeight, 0.01));
                half3 glow = lerp(_DeepColor.rgb, _CoreColor.rgb, height * height) * (0.35 + 0.65 * facing);

                // Veins seen at a depth under the surface: the lookup moves against the view, in object space so they follow the crystal
                float3 viewOS = TransformWorldToObjectDir(viewWS);
                float3 veinPosition = input.positionOS * _VeinScale - viewOS * _VeinDepth * _VeinScale + float3(0, _Time.y * _VeinSpeed, 0);
                half veins = Veins(veinPosition) * (0.4 + 0.6 * height);

                // Facets shimmer in turn, and a rim of light outlines the crystal
                float facet = Hash(floor(normalWS * 8) + seed);
                half shimmer = 1 + (sin(_Time.y * _FacetSpeed + facet * 40) * 0.5 + 0.5) * _FacetShimmer * (facet - 0.3);
                half3 rim = _RimColor.rgb * pow(1 - facing, _RimPower);

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
                surface.emission = ((glow * shimmer + _VeinColor.rgb * veins) * _GlowStrength + rim) * pulse;

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
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
