// The wave of the water pools, shared by Water.shadergraph (its vertex stage, through a Custom Function node) and the
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

// Shader Graph: moves the top face of the pool (object y at or above 0.5), the positions in object space
void WaterWave_float(float3 PositionOS, float3 PositionWS, float Speed, float Frequency, float Scale, out float3 Out)
{
    float3 slope;
    Out = PositionOS;
    Out.y += step(0.5, PositionOS.y) * WaterWave(PositionWS.xz, float3(Speed, Frequency, Scale), slope);
}

#endif
