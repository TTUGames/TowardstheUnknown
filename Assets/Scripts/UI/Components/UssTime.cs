using System.Globalization;
using UnityEngine.UIElements;

/// <summary>
/// Reads a time from a USS custom property ("0.25s", "250ms", or a duration token through var()): USS gives a custom
/// property holding a time as text only
/// </summary>
public static class UssTime
{
    /// <summary>
    /// The time <paramref name="property"/> holds in <paramref name="style"/>, in seconds, if set and valid
    /// </summary>
    public static bool TryGetSeconds(this ICustomStyle style, CustomStyleProperty<string> property, out float seconds)
    {
        seconds = 0;
        return style.TryGetValue(property, out string time) && TryParseSeconds(time, out seconds);
    }

    /// <summary>
    /// The time <paramref name="property"/> holds in <paramref name="style"/>, in milliseconds, or
    /// <paramref name="fallback"/>
    /// </summary>
    public static long Milliseconds(this ICustomStyle style, CustomStyleProperty<string> property, long fallback) =>
        style.TryGetSeconds(property, out float seconds) ? (long)(seconds * 1000 + 0.5f) : fallback;

    private static bool TryParseSeconds(string time, out float seconds)
    {
        time = time.Trim();
        float scale = 1;
        if (time.EndsWith("ms")) { scale = 0.001f; time = time[..^2]; }
        else if (time.EndsWith("s")) time = time[..^1];
        bool parsed = float.TryParse(time, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
        seconds *= scale;
        return parsed && seconds >= 0;
    }
}
