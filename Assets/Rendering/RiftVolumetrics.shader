// The air of the rift, drawn on a box around the room: a ray march through it, from where the view enters the box
// to the first visible surface, gathers the light of the room's lights (with their cookies and shadows: the light
// comes through the rift's cracks) scattered by drifting dust, and a mist that thickens below the tiles.
// The air and the mist also absorb: what lies far behind them, the back of the cave and the void, sinks into the dark
Shader "Towards the Unknown/Rift Volumetrics"
{
    Properties
    {
        _Density ("Air Density", Range(0, 0.05)) = 0.004
        _NoiseScale ("Dust Noise Scale", Float) = 0.35
        _NoiseStrength ("Dust Noise Strength", Range(0, 1)) = 0.8
        _Wind ("Wind", Vector) = (0.12, 0.03, 0.05, 0)
        [HDR] _MistColor ("Mist Color", Color) = (0.018, 0.03, 0.055, 1)
        _MistTop ("Mist Top Height", Float) = -0.7
        _MistFade ("Mist Fade Height", Float) = 2.5
        _MistDensity ("Mist Density", Range(0, 2)) = 0.35
        _MistLight ("Mist Light Scattering", Range(0, 4)) = 1.5
        _ShaftKnee ("Shaft Knee: dim light scatters less, the rays stand out", Range(0, 8)) = 1.5
        _Extinction ("Air Absorption, per meter", Range(0, 0.3)) = 0.04
        _MistExtinction ("Mist Absorption", Range(0, 4)) = 0.6
        _Steps ("Steps", Range(4, 48)) = 20
        _Intensity ("Intensity", Range(0, 4)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-50" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "RiftVolumetrics"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite Off
            ZTest Always
            // Premultiplied: the scattered light over what remains of the scene behind the air (its transmittance)
            Blend One SrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Density;
                float _NoiseScale;
                half _NoiseStrength;
                float4 _Wind;
                half4 _MistColor;
                float _MistTop;
                float _MistFade;
                half _MistDensity;
                half _MistLight;
                half _ShaftKnee;
                half _Extinction;
                half _MistExtinction;
                float _Steps;
                half _Intensity;
            CBUFFER_END

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings Vert(float4 positionOS : POSITION)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
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

            // The mist lies in the void under the tiles, thinning out upwards
            half Mist(float y)
            {
                return _MistDensity * saturate((_MistTop - y) / _MistFade + 0.15);
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
                // The ray of the view through this pixel, back from the box's far side (its back faces are drawn)
                bool orthographic = unity_OrthoParams.w > 0.5;
                float3 direction = orthographic ? -UNITY_MATRIX_V[2].xyz : normalize(input.positionWS - _WorldSpaceCameraPos);
                float3 origin = input.positionWS - direction * 200;

                // Where the ray enters and leaves the box, in world units along the ray (the box is a unit cube)
                float3 originOS = TransformWorldToObject(origin);
                float3 directionOS = TransformWorldToObjectDir(direction, false);
                float3 inverse = 1.0 / directionOS;
                float3 t0 = (-0.5 - originOS) * inverse;
                float3 t1 = (0.5 - originOS) * inverse;
                float3 tMin = min(t0, t1), tMax = max(t0, t1);
                float enter = max(max(tMin.x, tMin.y), tMin.z);
                float leave = min(min(tMax.x, tMax.y), tMax.z);

                // The first visible surface stops it
                float2 screenUV = input.positionCS.xy / _ScaledScreenParams.xy;
                float rawDepth = SampleSceneDepth(screenUV);
                float3 surfaceWS = ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP);
                float surface = dot(surfaceWS - origin, direction);
                if (!orthographic) enter = max(enter, dot(_WorldSpaceCameraPos - origin, direction));
                float end = min(leave, surface);
                if (end <= enter) return half4(0, 0, 0, 1);

                int steps = (int)_Steps;
                float stepLength = (end - enter) / steps;
                // A per pixel offset hides the steps' banding
                float jitter = InterleavedGradientNoise(input.positionCS.xy, 0);
                half3 scattered = 0;
                half transmittance = 1;
                float3 wind = _Wind.xyz * _Time.y;
                for (int s = 0; s < steps; s++)
                {
                    float3 position = origin + direction * (enter + (s + jitter) * stepLength);
                    half dust = 1 - _NoiseStrength + _NoiseStrength * 2 * ValueNoise(position * _NoiseScale + wind) * ValueNoise(position * _NoiseScale * 2.3 - wind * 1.7);
                    half mist = Mist(position.y);
                    half3 light = LightAt(position);
                    half luminance = Luminance(light);
                    light *= luminance / (luminance + _ShaftKnee);
                    scattered += (light * (_Density * dust + mist * _MistLight * 0.02) + _MistColor.rgb * mist) * stepLength * transmittance;
                    transmittance *= exp(-(_Extinction * dust + mist * _MistExtinction) * stepLength);
                }
                return half4(scattered * _Intensity, transmittance);
            }
            ENDHLSL
        }
    }
}
