using UnityEngine;

/// <summary>
/// The rings on the water, passed to Water.shader: those of the drops (<see cref="WaterDrip"/>: _WaterRipples,
/// _WaterRippleCount) and the small ones of the snowflakes touching it (<see cref="WaterSplash"/>: _WaterSplashes,
/// _WaterSplashCount). The shaders' clock is the time since the scene loaded, scaled
/// </summary>
public static class WaterRipples
{
    // Keep in step with WATER_MAX_RIPPLES and WATER_MAX_SPLASHES in Water.shader
    private const int MaxRipples = 16;
    private const int MaxSplashes = 48;
    private static readonly int RipplesId = Shader.PropertyToID("_WaterRipples");
    private static readonly int RippleCountId = Shader.PropertyToID("_WaterRippleCount");
    private static readonly int SplashesId = Shader.PropertyToID("_WaterSplashes");
    private static readonly int SplashCountId = Shader.PropertyToID("_WaterSplashCount");

    private static readonly Vector4[] ripples = new Vector4[MaxRipples];
    private static readonly Vector4[] splashes = new Vector4[MaxSplashes];
    private static int nextRipple, rippleCount, nextSplash, splashCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        nextRipple = rippleCount = nextSplash = splashCount = 0;
        Shader.SetGlobalInt(RippleCountId, 0);
        Shader.SetGlobalInt(SplashCountId, 0);
    }

    /// <summary>Sends the rings of a drop from a point of the surface</summary>
    public static void Add(Vector3 position)
    {
        ripples[nextRipple] = new Vector4(position.x, position.y, position.z, Time.timeSinceLevelLoad);
        nextRipple = (nextRipple + 1) % MaxRipples;
        rippleCount = Mathf.Min(rippleCount + 1, MaxRipples);
        Shader.SetGlobalVectorArray(RipplesId, ripples);
        Shader.SetGlobalInt(RippleCountId, rippleCount);
    }

    /// <summary>Queues the small ring of a snowflake touching the surface; <see cref="Flush"/> passes them</summary>
    public static void Splash(Vector3 position)
    {
        splashes[nextSplash] = new Vector4(position.x, position.y, position.z, Time.timeSinceLevelLoad);
        nextSplash = (nextSplash + 1) % MaxSplashes;
        splashCount = Mathf.Min(splashCount + 1, MaxSplashes);
    }

    /// <summary>Passes the splashes queued this frame to the shader</summary>
    public static void Flush()
    {
        Shader.SetGlobalVectorArray(SplashesId, splashes);
        Shader.SetGlobalInt(SplashCountId, splashCount);
    }
}
