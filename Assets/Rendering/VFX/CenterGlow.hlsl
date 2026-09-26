// The center glow particles (ported from the Amplify "CenterGlow" shaders): a texture panning, bent by a flow texture
// through a mask, times a panning noise, tinted by the color and the particle's; the center glow keeps only the mask's
// middle, shrinking with the custom data (uv0.z). CENTERGLOW_ADDITIVE draws it added (its alpha folded into the color),
// else blended with its alpha; CENTERGLOW_FRESNEL fades it towards the faces turned to the view
#ifndef CENTER_GLOW_INCLUDED
#define CENTER_GLOW_INCLUDED

#include "Assets/Rendering/VFX/VFXParticles.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
TEXTURE2D(_Flow); SAMPLER(sampler_Flow);
TEXTURE2D(_Mask); SAMPLER(sampler_Mask);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _Noise_ST;
    float4 _Flow_ST;
    float4 _Mask_ST;
    float4 _SpeedMainTexUVNoiseZW;
    float4 _DistortionSpeedXYPowerZ;
    float _Emission;
    half4 _Color;
    half _Opacity;
    float _Usecenterglow;
CBUFFER_END

half4 CenterGlowFrag(VFXVaryings input) : SV_Target
{
    float2 uv = input.uv0.xy;
    half4 mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv * _Mask_ST.xy + _Mask_ST.zw);
    half4 flow = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, VFXPan(uv, _Flow_ST, _DistortionSpeedXYPowerZ.xy));
    float2 mainUV = VFXPan(uv, _MainTex_ST, _SpeedMainTexUVNoiseZW.xy) - (flow * mask).rg * _DistortionSpeedXYPowerZ.z;
    half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
    half4 noise = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, VFXPan(uv, _Noise_ST, _SpeedMainTexUVNoiseZW.zw));
    half4 color = input.color;

    #if defined(CENTERGLOW_ADDITIVE)
        half4 glowed = main * noise * _Color * color * main.a * noise.a * _Color.a * color.a;
        half4 center = saturate(mask * saturate(mask - (1 - input.uv0.z)));
        half4 result = lerp(glowed, glowed * center, _Usecenterglow) * _Emission;
    #else
        half3 glowed = (main * noise * _Color * color).rgb;
        half4 center = saturate(mask * (mask - (1 - input.uv0.z)));
        half4 result = half4(lerp(glowed, glowed * center.rgb, _Usecenterglow) * _Emission, main.a * noise.a * _Color.a * color.a * _Opacity);
    #endif

    #if defined(CENTERGLOW_FRESNEL)
        result *= VFXFresnel(input, 1, 5);
    #endif

    #if defined(CENTERGLOW_ADDITIVE)
        return VFXFog(result, input, true);
    #else
        return VFXFog(result, input, false);
    #endif
}

#endif
