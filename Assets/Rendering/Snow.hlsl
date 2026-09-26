// The snow shared by Snow Lit and the grass of Nature Lit: where the sky is open above a point (the height map SnowCover
// draws) and how much the heat sources around it melt the snow
#ifndef SNOW_INCLUDED
#define SNOW_INCLUDED

// Set by SnowCover: the highest point of the room above each spot, and the area it covers
TEXTURE2D(_SnowHeightMap);
float4 _SnowHeightBounds; // x min, z min, size, 1 when the map is set
float _SnowHeightBias;
// Set by SnowCover: the room's heat sources, xyz position and w radius
float4 _SnowHeatSources[16];
int _SnowHeatCount;

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

// 1 where the sky is open above the point, 0 under an overhang
half SkyExposure(float3 positionWS)
{
    if (_SnowHeightBounds.w < 0.5) return 1;
    float2 uv = (positionWS.xz - _SnowHeightBounds.xy) / _SnowHeightBounds.z;
    if (any(uv < 0) || any(uv > 1)) return 1;
    float highest = SAMPLE_TEXTURE2D_LOD(_SnowHeightMap, sampler_LinearClamp, uv, 0).r;
    // A float map may hold garbage where nothing was drawn on some platforms: count it as open sky
    if (!(highest > -500 && highest < 500)) return 1;
    return smoothstep(-0.35, 0.0, positionWS.y - highest + _SnowHeightBias);
}

// How much the heat around melts the snow at the point, from 0 to 1: a patch around each source whose edge the noise
// pushes in and out, so that it is not a circle. A source only melts what lies near its height, not a floor far below
half HeatMelt(float3 positionWS)
{
    half melt = 0;
    float wobble = ValueNoise(positionWS * 1.1 + 3.7) * 0.7 + ValueNoise(positionWS * 3.3 + 9.1) * 0.3;
    for (int i = 0; i < _SnowHeatCount; i++)
    {
        float4 source = _SnowHeatSources[i];
        float radius = source.w * (0.6 + 0.8 * wobble);
        float distance = length(positionWS.xz - source.xz);
        float below = source.y - positionWS.y;
        half vertical = 1 - smoothstep(source.w * 1.8, source.w * 2.6, abs(below));
        melt = max(melt, (1 - smoothstep(radius * 0.55, radius, distance)) * vertical);
    }
    return melt;
}

#endif
