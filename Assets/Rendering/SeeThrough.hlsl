// The decor in front of the hovered tile, dithered away so that the tile shows through (SeeThrough.cs): what stands
// within a radius of the line from the tile to the game camera, nearer the camera than the tile and above its top. The
// tiles beside it, at its height, stay whole. Only for the game camera's view: the mirror camera of the water and the
// shadows keep the whole decor. Snow Lit and Nature Lit call it in their forward, depth and depth normals passes alike,
// so that the depth prepass doesn't hide the tile under the decor it cut
#ifndef SEE_THROUGH_INCLUDED
#define SEE_THROUGH_INCLUDED

// xyz: the hovered tile's top, w: how much of the decor in front of it is cut (0 when none is hovered)
float4 _SeeThroughTile;
// xyz: towards the game camera, w: the radius around the line from the tile to the camera, in meters
float4 _SeeThroughView;

// Discards the pixel at a world position, by an ordered dither of its screen position
void SeeThroughClip(float3 positionWS, float4 positionCS)
{
    half amount = _SeeThroughTile.w;
    // The game camera's view only: an orthographic camera looks along the opposite of its view matrix's third row
    if (amount <= 0 || dot(UNITY_MATRIX_V[2].xyz, _SeeThroughView.xyz) < 0.999) return;

    float3 offset = positionWS - _SeeThroughTile.xyz;
    float along = dot(offset, _SeeThroughView.xyz);
    float across = length(offset - along * _SeeThroughView.xyz);
    float radius = _SeeThroughView.w;
    half cut = (1 - smoothstep(radius * 0.5, radius, across)) * smoothstep(0.2, 0.6, along) * smoothstep(0.1, 0.3, offset.y);

    // 4x4 Bayer matrix: the share of the pixels cut grows with the amount, evenly spread
    uint2 pixel = (uint2)positionCS.xy % 4;
    const float bayer[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
    float threshold = (bayer[pixel.y * 4 + pixel.x] + 0.5) / 16;
    clip(threshold - cut * amount);
}

#endif
