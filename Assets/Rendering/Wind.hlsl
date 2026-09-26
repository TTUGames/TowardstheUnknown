// The wind of the vegetation, shared by Nature Lit and Snow Lit: moves a vertex in world space, in meters, so that it
// looks the same whatever the scale of the instance. Wind (on Environment/Snow) sets the global values: the breeze and the
// gusts crossing the room, the local drafts of WindDraft, the entities walking through (the grass and the small plants
// bend away from them) and the waves of the heavy hits
#ifndef WIND_INCLUDED
#define WIND_INCLUDED

#include "Water.hlsl"

// xz the direction the wind blows to, w its strength (0 still)
float4 _WindDirection;
// The gust crossing the room now and then: x how much it adds to the lean, y where its front is along the wind (in meters,
// as dot(position.xz, direction)), z its width in meters, w the speed of the sway (radians per second)
float4 _WindGust;
// x the speed of the flutter (radians per second)
float4 _WindFlutterParams;

// The entities: xyz their feet, w the radius they bend the plants in
#define WIND_MAX_PUSHERS 16
float4 _WindPushers[WIND_MAX_PUSHERS];
int _WindPusherCount;

// The local drafts: xyz center, w radius; and xyz the direction scaled by the strength
#define WIND_MAX_DRAFTS 8
float4 _WindDrafts[WIND_MAX_DRAFTS];
float4 _WindDraftForces[WIND_MAX_DRAFTS];
int _WindDraftCount;

// The hit waves: xyz center, w start time (_Time.y); and x strength, y speed in meters per second, z duration, w width
#define WIND_MAX_WAVES 4
float4 _WindWaves[WIND_MAX_WAVES];
float4 _WindWaveParams[WIND_MAX_WAVES];
int _WindWaveCount;

// The masks: which part of the mesh moves
#define WIND_MASK_HEIGHT 0   // bends from its base: nothing at the pivot, the most at _WindHeight above it
#define WIND_MASK_HANGING 1  // swings from its top, a vine or a strand hanging under its pivot
#define WIND_MASK_RIGID 2    // tilts as a whole around its pivot, a mushroom
#define WIND_MASK_FLOATING 3 // floats on the water, a lily pad: rises and tilts with the waves, drifts a little

struct WindSettings
{
    half bend;      // how far the top leans in a full gust, in meters
    half flutter;   // how far the tips shiver, in meters
    float height;   // the height of the mesh above its pivot (below for a hanging one), in meters
    half mask;      // WIND_MASK_*
    half push;      // how far the entities bend it, in meters (0: they don't)
    float3 wave;    // floating: the water's wave, as Water.shadergraph's _WaveSpeed, _WaveFrequency, _WaveScale
    half weight;    // below 0: from the mask; else the vertex's own share of the lean (the grass: its height along the blade)
};

float WindHash(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// From 0 to 1: the gust front passing over the point, uneven across the wind
float WindGustAt(float2 positionXZ)
{
    float2 direction = _WindDirection.xz;
    float along = dot(positionXZ, direction) - _WindGust.y;
    float across = dot(positionXZ, float2(-direction.y, direction.x));
    float gust = exp(-along * along / max(_WindGust.z * _WindGust.z, 0.01));
    return gust * (0.7 + 0.3 * sin(across * 0.45 + _Time.y * 0.8));
}

// How much of the lean a vertex takes, from its place against the pivot
float WindWeight(float3 positionWS, float3 pivotWS, WindSettings settings)
{
    if (settings.weight >= 0) return settings.weight;
    float height = max(settings.height, 0.01);
    if (settings.mask == WIND_MASK_HANGING) return saturate((pivotWS.y - positionWS.y) / height);
    if (settings.mask == WIND_MASK_RIGID) return (positionWS.y - pivotWS.y) / height;
    float weight = saturate((positionWS.y - pivotWS.y) / height);
    return weight * weight;
}

// The push of the entities, the drafts and the hit waves on a vertex whose pivot is at pivotWS, in meters on xz
float2 WindLocalPush(float3 positionWS, float3 pivotWS, WindSettings settings, float phase)
{
    float2 push = 0;
    // The entities bend the plants away from their feet
    if (settings.push > 0)
    {
        for (int i = 0; i < _WindPusherCount; i++)
        {
            float4 pusher = _WindPushers[i];
            float2 away = positionWS.xz - pusher.xz;
            float distance = length(away);
            float near = 1 - smoothstep(pusher.w * 0.3, pusher.w, distance);
            near *= 1 - smoothstep(0.5, 1.5, abs(positionWS.y - pusher.y));
            push += away / max(distance, 0.001) * near * settings.push;
        }
    }
    // The drafts blow harder where they are, with a flutter of their own
    for (int j = 0; j < _WindDraftCount; j++)
    {
        float4 draft = _WindDrafts[j];
        float inside = 1 - smoothstep(draft.w * 0.4, draft.w, distance(pivotWS, draft.xyz));
        push += _WindDraftForces[j].xz * inside * (0.75 + 0.25 * sin(_Time.y * 2.7 + phase)) * settings.bend;
    }
    // A heavy hit sends a ring that lays the plants down as it passes
    for (int k = 0; k < _WindWaveCount; k++)
    {
        float4 wave = _WindWaves[k];
        float4 parameters = _WindWaveParams[k];
        float age = _Time.y - wave.w;
        if (age < 0 || age > parameters.z) continue;
        float2 away = pivotWS.xz - wave.xz;
        float distance = length(away);
        float ring = age * parameters.y;
        float band = exp(-(distance - ring) * (distance - ring) / max(parameters.w * parameters.w, 0.001));
        float fade = 1 - age / parameters.z;
        push += away / max(distance, 0.001) * band * fade * fade * parameters.x * max(settings.bend, settings.push);
    }
    return push;
}

// Moves a vertex of a plant whose pivot is at pivotWS
float3 ApplyWind(float3 positionWS, float3 pivotWS, WindSettings settings)
{
    float phase = WindHash(pivotWS.xz) * 6.2831853;
    float strength = _WindDirection.w;
    float2 direction = _WindDirection.xz;
    float time = _Time.y;

    if (settings.mask == WIND_MASK_FLOATING)
    {
        // Rides the water's wave at its pivot, tilted by its slope, and drifts a little with the wind
        float3 slope;
        float rise = WaterWave(pivotWS.xz, settings.wave, slope);
        float2 offset = positionWS.xz - pivotWS.xz;
        positionWS.y += rise + dot(offset, slope.xz);
        positionWS.xz += direction * strength * settings.bend * (0.5 + 0.5 * sin(time * _WindGust.w * 0.35 + phase));
        return positionWS;
    }

    float weight = WindWeight(positionWS, pivotWS, settings);
    float gust = WindGustAt(pivotWS.xz);

    // The lean: with the breeze, much stronger as a gust passes, and a slow sway around it, partly across the wind
    float lean = strength * (0.3 + gust * _WindGust.x);
    float sway = sin(time * _WindGust.w + phase) * 0.6 + sin(time * _WindGust.w * 2.3 + phase * 1.7) * 0.25;
    float2 across = float2(-direction.y, direction.x);
    float2 offset = direction * (lean + sway * strength * 0.35) + across * sin(time * _WindGust.w * 0.7 + phase * 2.1) * strength * 0.15;
    offset = offset * settings.bend + WindLocalPush(positionWS, pivotWS, settings, phase);
    offset *= weight;
    positionWS.xz += offset;

    // Keeps the length: a bent stem lowers its tip, a swinging strand raises its end
    float drop = dot(offset, offset) * 0.5 / max(settings.height, 0.01);
    positionWS.y += settings.mask == WIND_MASK_HANGING ? drop : -drop;

    // The flutter: the tips shiver, faster and each on its own
    float seed = dot(positionWS, float3(1.7, 2.3, 1.3)) * 4;
    float speed = _WindFlutterParams.x;
    float3 shiver = float3(sin(time * speed + seed), sin(time * speed * 1.3 + seed * 1.1) * 0.5, cos(time * speed * 0.9 + seed * 0.9));
    positionWS += shiver * settings.flutter * strength * (0.4 + gust) * saturate(abs(weight));
    return positionWS;
}

// The point the plant moves around, in world space: the object's pivot, or the anchor WindAnchor passes (xyz, with the
// height to use in w) to the parts of a plant made of several meshes and to the strands hanging from their top
float3 WindPivot(float4 anchor, inout WindSettings settings)
{
    if (anchor.w > 0)
    {
        settings.height = anchor.w;
        return anchor.xyz;
    }
    return GetObjectToWorldMatrix()._m03_m13_m23;
}

#endif
