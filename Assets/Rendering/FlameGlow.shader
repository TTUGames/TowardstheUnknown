// The glow around a flame: a soft disc facing the camera, additive, breathing with noise at the flame's own pace.
// On a quad (the object's scale sets the disc's size); the glow's center is the object's origin
Shader "Towards the Unknown/Flame Glow"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.7, 0.24, 0.05, 1)
        _Falloff ("Falloff", Range(0.5, 8)) = 3.2
        _Flicker ("Flicker", Range(0, 1)) = 0.3
        _Speed ("Speed", Range(0, 12)) = 6
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "FlameGlow"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Noise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Falloff;
                half _Flicker;
                half _Speed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half intensity : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                float size = length(TransformObjectToWorldDir(float3(1, 0, 0), false));
                // Billboard: the quad's corners laid along the camera's axes
                float2 corner = input.uv * 2 - 1;
                float3 positionWS = origin + (UNITY_MATRIX_V[0].xyz * corner.x + UNITY_MATRIX_V[1].xyz * corner.y) * size;
                // Pulled toward the camera so that the flame's mesh doesn't cut it
                positionWS += normalize(GetWorldSpaceViewDir(origin)) * size * 0.5;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = corner;
                float seed = Hash(floor(origin * 7.31) + 0.5) * 50;
                float time = _Time.y * _Speed;
                float noise = ValueNoise(float3(seed, time * 0.35, 0)) * 0.7 + ValueNoise(float3(seed, time, 5)) * 0.3;
                output.intensity = 1 + (noise * 2 - 1) * _Flicker;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half distance = saturate(1 - length(input.uv));
                half glow = pow(distance, _Falloff) * input.intensity;
                half3 color = _Color.rgb * glow;
                color = MixFogColor(color, half3(0, 0, 0), input.fogFactor);
                return half4(color, 0);
            }
            ENDHLSL
        }
    }
}
