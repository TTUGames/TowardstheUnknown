using System.Collections;
using UnityEngine;

/// <summary>
/// Owns <c>Time.timeScale</c>, which the pause, the combat feedback and the game speed setting share:
/// the pause stops the time, a hit stop freezes it for an instant, a slow motion slows it, and the speed multiplies the rest
/// </summary>
public static class GameTime
{
    private const float HitStopScale = 0;

    private static bool paused;
    private static float speed = 1;
    // In unscaled time
    private static float hitStopEnd;
    private static float slowMotionEnd;
    private static float slowMotionScale = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        paused = false;
        speed = 1;
        Clear();
    }

    /// <summary>
    /// Stops the time behind the pause menu
    /// </summary>
    public static bool Paused
    {
        get => paused;
        set { paused = value; Apply(); }
    }

    /// <summary>
    /// The game speed setting, multiplying the time outside the pause and the combat feedback
    /// </summary>
    public static float Speed
    {
        get => speed;
        set { speed = Mathf.Max(0.1f, value); Apply(); }
    }

    /// <summary>
    /// Freezes the time for an instant, in real seconds
    /// </summary>
    public static void HitStop(float seconds) => Extend(ref hitStopEnd, seconds);

    /// <summary>
    /// Slows the time down to the scale, for real seconds
    /// </summary>
    public static void SlowMotion(float scale, float seconds)
    {
        slowMotionScale = Time.unscaledTime < slowMotionEnd ? Mathf.Min(slowMotionScale, scale) : scale;
        Extend(ref slowMotionEnd, seconds);
    }

    /// <summary>
    /// Ends the hit stops and slow motions, when a scene changes
    /// </summary>
    public static void Clear()
    {
        hitStopEnd = slowMotionEnd = 0;
        slowMotionScale = 1;
        Apply();
    }

    private static void Extend(ref float end, float seconds)
    {
        if (seconds <= 0) return;
        end = Mathf.Max(end, Time.unscaledTime + seconds);
        Apply();
        ActionManager.Run(ApplyAfter(seconds));
    }

    private static IEnumerator ApplyAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Apply();
    }

    private static void Apply()
    {
        float now = Time.unscaledTime;
        if (paused) Time.timeScale = 0;
        else if (now < hitStopEnd) Time.timeScale = HitStopScale;
        else Time.timeScale = (now < slowMotionEnd ? slowMotionScale : 1) * speed;
    }
}
