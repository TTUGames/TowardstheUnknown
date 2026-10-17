// Used by RiftVolumetricsFeature: lays the rift's air, drawn at a fraction of the screen's resolution, over the camera color.
// Each pixel takes the four nearest texels of the small image, weighted by how close their scene depth is to its own, so
// that the air doesn't bleed over the edges of what stands in front of it
Shader "Hidden/RiftVolumetricsComposite"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "RiftVolumetricsComposite"
            // Premultiplied, as the air was drawn: the scattered light over what remains of the scene behind it
            Blend One SrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Size of the small image: width, height, 1 / width, 1 / height
            float4 _RiftTargetSize;
            // Depth difference in meters from which a texel counts for little, over a flat surface: more on a slanted one, whose
            // depth changes from one texel to the next (else the weights alternate with the texels and draw a grid)
            #define DEPTH_TOLERANCE 0.1
            #define SLOPE_TOLERANCE 4

            float EyeDepth(float rawDepth)
            {
                return unity_OrthoParams.w > 0.5 ? LinearDepthToEyeDepth(rawDepth) : LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 position = uv * _RiftTargetSize.xy - 0.5;
                int2 origin = (int2)floor(position);
                float2 fraction = position - origin;
                float depth = EyeDepth(SampleSceneDepth(uv));
                float tolerance = DEPTH_TOLERANCE + SLOPE_TOLERANCE * (abs(ddx(depth)) + abs(ddy(depth)));

                half4 sum = 0;
                half weights = 0;
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    int2 offset = int2(i & 1, i >> 1);
                    int2 texel = clamp(origin + offset, 0, (int2)_RiftTargetSize.xy - 1);
                    // The depth the air was marched to at this texel
                    float tapDepth = EyeDepth(SampleSceneDepth((texel + 0.5) * _RiftTargetSize.zw));
                    half bilinear = (offset.x > 0 ? fraction.x : 1 - fraction.x) * (offset.y > 0 ? fraction.y : 1 - fraction.y);
                    half weight = bilinear / (tolerance + abs(tapDepth - depth));
                    sum += LOAD_TEXTURE2D_X(_BlitTexture, texel) * weight;
                    weights += weight;
                }
                return sum / weights;
            }
            ENDHLSL
        }
    }
}
