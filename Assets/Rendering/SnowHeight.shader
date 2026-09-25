// Used by SnowCover: draws the world height of the room's meshes seen from the top, keeping the highest one,
// into the height map that tells the Snow Lit surfaces whether something shelters them
Shader "Hidden/Snow Height"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always
        BlendOp Max
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _SnowHeightBounds; // x min, z min, size

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float height : TEXCOORD0;
            };

            Varyings Vert(float4 positionOS : POSITION)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(positionOS.xyz);
                // The map covers the room's square seen from the top: x and z become the screen
                float2 uv = (positionWS.xz - _SnowHeightBounds.xy) / _SnowHeightBounds.z;
                float2 clip = uv * 2 - 1;
                #if UNITY_UV_STARTS_AT_TOP
                    clip.y = -clip.y;
                #endif
                output.positionCS = float4(clip, 0.5, 1);
                output.height = positionWS.y;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                return float4(input.height, 0, 0, 1);
            }
            ENDHLSL
        }
    }
}
