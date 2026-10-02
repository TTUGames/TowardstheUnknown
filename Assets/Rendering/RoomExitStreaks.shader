// The light rising from the rim of an open exit (RoomExit.prefab), drawn on an open cylinder standing on the rim
// (Art/VFX/RoomExit/RoomExitCylinder.asset: U around it, V up it), so that it leaves the rim itself, in front of and
// behind the exit: thin streaks climbing in lanes around it, motes floating up (_Motes), over a faint curtain of light at
// the foot. Tinted by _Color. _Reveal (set per renderer by ExitPortal) sends a band of light up the cylinder as the exit
// opens, before the streaks start; _Hover (the pointer on the exit) brightens it
Shader "Towards the Unknown/Room Exit Streaks"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.35, 0.6, 2, 1)
        _Lanes ("Lanes", Range(4, 64)) = 28
        _Density ("Density", Range(0, 1)) = 0.6
        _Speed ("Speed", Range(0, 2)) = 0.3
        _Length ("Streak Length", Range(0.02, 0.6)) = 0.18
        _Thickness ("Streak Width (pixels)", Range(0.5, 4)) = 1.5
        _Brightness ("Streak Brightness", Range(0, 4)) = 1.2
        _Motes ("Motes", Range(0, 3)) = 0
        _MoteSize ("Mote Size (pixels)", Range(1, 8)) = 2.5
        _MoteSpeed ("Mote Speed", Range(0, 1)) = 0.15
        _Curtain ("Curtain", Range(0, 1)) = 0.3
        _CurtainFalloff ("Curtain Falloff", Range(0.5, 12)) = 5
        _HoverBoost ("Hover Boost", Range(0, 3)) = 1.2
        [Header(Set per renderer)]
        _Reveal ("Reveal", Range(0, 1)) = 1
        _Hover ("Hovered", Range(0, 1)) = 0
        _Fade ("Fade", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "RoomExitStreaks"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            // Both the front and the back of the cylinder
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Noise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Lanes;
                half _Density;
                half _Speed;
                half _Length;
                half _Thickness;
                half _Brightness;
                half _Motes;
                half _MoteSize;
                half _MoteSpeed;
                half _Curtain;
                half _CurtainFalloff;
                half _HoverBoost;
                half _Reveal;
                half _Hover;
                half _Fade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float seed : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                // How much the surface faces the camera: the curtain fades out on the cylinder's silhouette
                half facing : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                output.seed = Hash(float3(floor(origin.xz * 4) + 0.5, 0.5)) * 100;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(input.positionOS.xyz));
                output.facing = abs(dot(normalWS, viewWS));
                return output;
            }

            // The streak of one lane at this point: a short vertical line climbing at its own pace, absent from some cycles
            half Streak(float2 uv, float lanePosition, float lane, float seed, float t, float pixelU)
            {
                float h1 = Hash(float3(lane, seed, 1.3));
                float h2 = Hash(float3(lane, seed, 7.1));
                float speed = _Speed * (0.6 + 0.8 * h1);
                float cycle = t * speed + h2;
                float phase = frac(cycle);
                // Skipped on some cycles, so that the lanes don't all burn at once
                half present = step(Hash(float3(lane, floor(cycle), seed)), _Density);
                float head = phase * (1 + _Length);
                half along = saturate((uv.y - (head - _Length)) / _Length) * step(uv.y, head);
                half across = saturate(_Thickness * 0.5 - abs(lanePosition - 0.5 - (h2 - 0.5) * 0.4) / pixelU + 0.5);
                // Faded in leaving the rim, out before the top
                half life = saturate(head * 8) * saturate((1 - head) * 3);
                return along * along * across * life * present;
            }

            // A mote of one lane: a round dot floating up, wavering
            half Mote(float2 uv, float lanePosition, float lane, float seed, float t, float pixelU, float pixelV)
            {
                float h1 = Hash(float3(lane, seed, 4.9));
                float h2 = Hash(float3(lane, seed, 2.3));
                float cycle = t * _MoteSpeed * (0.6 + 0.8 * h1) + h2;
                float phase = frac(cycle);
                half present = step(Hash(float3(lane, floor(cycle), seed + 3)), 0.5);
                float x = 0.5 + (h2 - 0.5) * 0.5 + sin(t * (0.8 + h1) + lane) * 0.15;
                float2 delta = float2((lanePosition - x) / pixelU, (uv.y - phase) / pixelV);
                float size = _MoteSize * (0.6 + 0.6 * h1) * (1 - phase * 0.5);
                half disc = saturate(1 - length(delta) / size);
                return disc * disc * sin(phase * PI) * present;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _Time.y + input.seed;
                float pixelV = max(fwidth(uv.y), 1e-5);
                // Two offset layers of lanes, denser without lining up
                float lanes = floor(_Lanes);
                float u = uv.x * lanes;
                float pixelU = max(fwidth(u), 1e-5);
                half streaks = Streak(uv, frac(u), floor(u), input.seed, t, pixelU);
                float u2 = u + 0.5;
                streaks += Streak(uv, frac(u2), fmod(floor(u2), lanes) + 101, input.seed, t * 0.8, pixelU) * 0.6;
                // The motes, in lanes of their own, wider apart
                float moteLanes = max(floor(lanes * 0.4), 1);
                float um = uv.x * moteLanes;
                half motes = Mote(uv, frac(um), floor(um), input.seed, t, max(fwidth(um), 1e-5), pixelV) * _Motes;
                // The curtain at the foot, wavering a little
                half curtain = pow(saturate(1 - uv.y), _CurtainFalloff) * (0.75 + 0.25 * ValueNoise(float3(uv.x * 12, t * 0.5, input.seed))) * _Curtain;
                curtain *= input.facing * input.facing;

                // The reveal: a band of light climbing the cylinder, then the streaks and motes fading in
                float bandY = saturate((_Reveal - 0.25) / 0.6) * 1.15;
                half band = (exp(-pow((uv.y - bandY) / 0.14, 2)) * 2.2
                    + pow(saturate(1 - uv.y / max(bandY, 1e-3)), 2) * step(uv.y, bandY) * 0.5)
                    * step(0.25, _Reveal) * (1 - _Reveal) * input.facing;
                half settle = smoothstep(0.55, 1, _Reveal);

                half boost = 1 + smoothstep(0, 1, _Hover) * _HoverBoost;
                half3 result = _Color.rgb * boost * ((streaks * _Brightness + motes) * settle + curtain * smoothstep(0.3, 0.8, _Reveal) + band * 0.5) * _Fade;
                result = MixFogColor(result, half3(0, 0, 0), input.fogFactor);
                return half4(result, 0);
            }
            ENDHLSL
        }
    }
}
