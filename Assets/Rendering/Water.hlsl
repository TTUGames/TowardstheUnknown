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

#endif
