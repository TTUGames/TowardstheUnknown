using UnityEngine;

/// <summary>
/// The rings on the water, passed to Water.shader: those of the drops (<see cref="WaterDrip"/>: _WaterRipples,
/// _WaterRippleCount) and the small ones of the snowflakes touching it (<see cref="WaterSplash"/>: _WaterSplashes,
/// _WaterSplashCount). The shaders' clock is the time since the scene loaded, scaled
/// </summary>
public static class WaterRipples
{
    // Capacities: WATER_MAX_RIPPLES and WATER_MAX_SPLASHES in Water.shader
    private static readonly ShaderRingBuffer ripples = new(16, "_WaterRippleCount", "_WaterRipples");
    private static readonly ShaderRingBuffer splashes = new(48, "_WaterSplashCount", "_WaterSplashes");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ripples.Clear();
        splashes.Clear();
    }

    private static Vector4 Now(Vector3 position) => new(position.x, position.y, position.z, Time.timeSinceLevelLoad);

    /// <summary>Sends the rings of a drop from a point of the surface</summary>
    public static void Add(Vector3 position)
    {
        ripples.Add(Now(position));
        ripples.Pass();
    }

    /// <summary>Queues the small ring of a snowflake touching the surface; <see cref="Flush"/> passes them</summary>
    public static void Splash(Vector3 position) => splashes.Add(Now(position));

    /// <summary>Passes the splashes queued this frame to the shader</summary>
    public static void Flush() => splashes.Pass();
}
