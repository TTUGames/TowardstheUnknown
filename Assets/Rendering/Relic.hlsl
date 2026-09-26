// Shared by the relic shaders (Relic Orb, Relic Distortion): noise, the relic's seed and its glitch clock, so that
// the orb and the space around it glitch at the same moments. They declare _GlitchRate and _GlitchChance, set by RelicAura
#ifndef RELIC_INCLUDED
#define RELIC_INCLUDED

float Hash(float3 p)
{
    p = frac(p * 0.3183099 + 0.1);
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float Hash1(float n) { return frac(sin(n * 127.1 + 311.7) * 43758.5453); }

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

// Each relic has its own seed, from its position: its float and its glitches don't match the others'
float RelicSeed()
{
    float3 objectWS = GetObjectToWorldMatrix()._m03_m13_m23;
    return Hash(floor(objectWS * 5.3) + 0.5) * 100;
}

// How hard the relic glitches right now, from 0 to 1: the time is cut in short slots, a few of which glitch
float RelicGlitch(float seed, float rate, float chance)
{
    float slot = floor(_Time.y * rate);
    float on = step(1 - chance, Hash1(slot + seed));
    return on * (0.4 + 0.6 * Hash1(slot * 1.7 + seed));
}

#endif
