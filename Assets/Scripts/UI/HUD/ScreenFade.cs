using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Covers the screen with a <see cref="SlantedWipe"/> and reveals it again when the player changes room.
/// <see cref="FadeIn"/> ends once the screen is fully covered, <see cref="FadeOut"/> once it is revealed and the wipe hidden.
/// The wipe blocks the pointer while it is shown
/// </summary>
public class ScreenFade
{
    // The frame that loads a room is long: cap its duration so that the reveal doesn't skip half its sweep
    private const float MaxFrameDuration = 0.1f;

    private SlantedWipe wipe;

    public SlantedWipe Wipe => wipe;

    public void Bind(SlantedWipe wipe)
    {
        this.wipe = wipe;
        wipe?.Hide();
    }

    public IEnumerator FadeIn()
    {
        if (wipe == null) yield break;
        yield return wipe.Cover(FrameDuration);
    }

    public IEnumerator FadeOut()
    {
        if (wipe == null) yield break;
        yield return wipe.Reveal(FrameDuration);
    }

    private static float FrameDuration() => Mathf.Min(Time.deltaTime, MaxFrameDuration);
}
