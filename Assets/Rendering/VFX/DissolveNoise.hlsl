// The dissolving particles (ported from the Amplify "DissolveNoise" shaders): a panning noise shades between a main and a
// noise color; a second panning noise dissolves the particle as its custom data grows (uv0.z), with a colored edge,
// and hides it past uv0.w. The texture tints both colors when asked, and gives the alpha
#ifndef DISSOLVE_NOISE_INCLUDED
#define DISSOLVE_NOISE_INCLUDED

#include "Assets/Rendering/VFX/VFXParticles.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
TEXTURE2D(_TextureNoise); SAMPLER(sampler_TextureNoise);
TEXTURE2D(_Dissolvenoise); SAMPLER(sampler_Dissolvenoise);

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _TextureNoise_ST;
    float4 _Dissolvenoise_ST;
    float4 _NoisespeedXYEmissonZPowerW;
    float4 _DissolvespeedXY;
    half4 _Maincolor;
    half4 _Noisecolor;
    half4 _Dissolvecolor;
    float _Usetexturecolor;
CBUFFER_END

half4 DissolveNoiseFrag(VFXVaryings input) : SV_Target
{
    float4 uv = input.uv0;
    float power = _NoisespeedXYEmissonZPowerW.w;
    half4 noise = SAMPLE_TEXTURE2D(_TextureNoise, sampler_TextureNoise, VFXPan(uv.xy, _TextureNoise_ST, _NoisespeedXYEmissonZPowerW.xy));
    half4 shaded = lerp(_Maincolor, _Noisecolor, saturate(pow(abs(noise), power) * power));

    half dissolve = SAMPLE_TEXTURE2D(_Dissolvenoise, sampler_Dissolvenoise, VFXPan(uv.xy, _Dissolvenoise_ST, _DissolvespeedXY.xy)).r;
    half dissolved = step(dissolve, uv.z);
    half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);

    half4 body = shaded * (1 - dissolved);
    body = lerp(body, body * main, _Usetexturecolor);
    half4 edgeColor = lerp(_Dissolvecolor, _Dissolvecolor * main, _Usetexturecolor);
    // The edge: the dissolve noise against the custom data, both remapped around 0
    half edge = saturate((-4 + ((-0.65 + (1 - uv.z) * 1.3) + dissolve) * 11) * 3);
    half4 colored = lerp(body, edgeColor, edge * dissolved);
    half visible = saturate(-15 + (dissolve + (-0.65 + uv.w * 1.3)) * 30);

    half4 result = half4((_NoisespeedXYEmissonZPowerW.z * colored * input.color).rgb, input.color.a * main.a * visible);
    return VFXFog(result, input, false);
}

#endif
