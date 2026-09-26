// The wave of the water pools, shared by Water.shader (the top vertices of the pool's cube) and the
// plants floating on them (Nature Lit's floating wind mask): both rise and fall together
#ifndef WATER_INCLUDED
#define WATER_INCLUDED

// The height the wave adds at a point, and its slope (x and z, y unused). wave: x speed, y frequency, z scale
float WaterWave(float2 positionXZ, float3 wave, out float3 slope)
{
    float2 angle = wave.x * _Time.y + wave.y * positionXZ;
    slope = float3(cos(angle.x), 0, -sin(angle.y)) * wave.y * wave.z;
    return (sin(angle.x) + cos(angle.y)) * wave.z;
}

// The pools of the room (WaterSurface.PassPools): xy the center, zw the half size (x and z of the box), and xy the box's
// x axis on the ground, z the surface's height
#define WATER_MAX_POOLS 8
float4 _WaterPools[WATER_MAX_POOLS];
float4 _WaterPoolAxes[WATER_MAX_POOLS];
int _WaterPoolCount;

// From 0 to 1: how wet a point is, damp in a band just over a pool's waterline (band: its height, in meters)
half WaterWetness(float3 positionWS, float band)
{
    half wet = 0;
    for (int i = 0; i < _WaterPoolCount; i++)
    {
        float4 pool = _WaterPools[i];
        float4 axes = _WaterPoolAxes[i];
        float2 offset = positionWS.xz - pool.xy;
        float2 local = float2(dot(offset, axes.xy), dot(offset, float2(-axes.y, axes.x)));
        // A little past the box's sides, where the banks meet the water
        float2 outside = max(abs(local) - pool.zw - 0.3, 0);
        if (outside.x + outside.y > 0) continue;
        float height = positionWS.y - axes.z;
        wet = max(wet, saturate(1 - height / band) * step(-0.05, height));
    }
    return wet;
}

#endif
