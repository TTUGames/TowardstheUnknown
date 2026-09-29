// The cold on the characters, shared by the player's outfit (Glow.hlsl) and the creatures (EnemyEnergy.hlsl): colors
// washed out towards a pale cold gray, and snow lying in grainy patches. The snow is drawn on the mesh's weather channel
// (UV set 2, a float4 written once per mesh: xyz the vertex at the bind pose in meters, w how much snow settles there),
// so that it stays fixed on the body whatever the animation, continuous from one part to the next; a mesh without the
// channel gets none
#ifndef WEATHER_INCLUDED
#define WEATHER_INCLUDED

#include "Noise.hlsl"

// Towards a pale cold gray of its own brightness
half3 WashOut(half3 albedo, half amount)
{
    half luminance = dot(albedo, half3(0.299, 0.587, 0.114));
    return lerp(albedo, luminance * half3(0.85, 0.92, 1.05) + 0.06, amount);
}

struct SnowCover
{
    half amount; // 0 to 1, how much the surface is snow
    half field;  // the smooth field under the patches, for their relief
    half grain;  // a fine noise breaking the patches, for their brightness
};

// The snow at a point of the weather channel: patches of a noise, per meter of scale, where it settles, from the threshold
// over the softness; a fine grain breaks the edges and the full patches (never a flat white), and a few flakes are caught
// anywhere with the weathering
SnowCover SnowAt(float4 weather, float scale, half threshold, half softness, half flakes)
{
    SnowCover snow;
    float3 patches = weather.xyz * scale;
    half field = ValueNoise(patches) * 0.52 + ValueNoise(patches * 2.4 + 17.3) * 0.26 + ValueNoise(patches * 5.3 + 34.6) * 0.13;
    snow.grain = ValueNoise(patches * 13 + 51.2);
    field += snow.grain * 0.09;
    half settles = weather.w;
    snow.field = saturate((field - (1 - settles)) * 2.4 + settles * 0.15);
    snow.amount = smoothstep(threshold, threshold + softness, snow.field) * (0.7 + 0.3 * snow.grain);
    snow.amount = max(snow.amount, smoothstep(0.84, 0.92, ValueNoise(weather.xyz * 140 + 3.1)) * flakes * step(0.001, settles));
    return snow;
}

// The snow's color on the surface: slightly uneven with its grain
half3 SnowAlbedo(half3 albedo, half3 snowColor, SnowCover snow)
{
    return lerp(albedo, snowColor * (0.8 + 0.2 * snow.grain), snow.amount);
}

// Bends a normal by the slope of a height (in meters) across the screen: a bump without tangents (Mikkelsen's surface
// gradient), for the snow's thickness and the fabric's threads
float3 BumpNormal(float3 normalWS, float3 positionWS, float height)
{
    float3 dpdx = ddx(positionWS), dpdy = ddy(positionWS);
    float3 r1 = cross(dpdy, normalWS), r2 = cross(normalWS, dpdx);
    float det = dot(dpdx, r1);
    float3 gradient = sign(det) * (ddx(height) * r1 + ddy(height) * r2);
    return normalize(abs(det) * normalWS - gradient);
}

#endif
