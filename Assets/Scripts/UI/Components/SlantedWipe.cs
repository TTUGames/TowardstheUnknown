using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A screen wipe in the game's 45° shape language: horizontal bands cut at 45° (parallelograms) sweep across the
/// element from left to right to cover it, one after the other, then sweep out on the right to reveal it again.
/// <see cref="Cover"/> and <see cref="Reveal"/> end on the exact covered and hidden states; the element blocks the
/// pointer while it is shown and is not displayed once revealed.
/// USS sets its look with custom properties: --fill-color (the bands), --wipe-duration (each way, in seconds).
/// The slanted-wipe class of Common.uss fills its parent with them
/// </summary>
[UxmlElement]
public partial class SlantedWipe : VisualElement
{
    private enum Phase { Hidden, Covering, Covered, Revealing }

    // The start delay of each band, as a rank: a comb rather than one straight diagonal
    private static readonly int[] bandOrder = { 2, 0, 3, 1, 4 };
    // Share of the duration spread over the band delays; each band sweeps during the rest
    private const float Stagger = 0.35f;
    // The bands overlap by this much in height, so that no line of the game shows between two covering bands
    private const float BandOverlap = 1;
    private const float DefaultDuration = 0.4f;
    // The frames that load a room or a scene are long: their duration is capped so that the sweep doesn't skip half its way
    private const float MaxFrameDuration = 1f / 30f;

    private static readonly CustomStyleProperty<Color> fillColorProperty = new("--fill-color");
    // A time ("0.4s", "400ms"): USS gives a custom property holding a time as text only
    private static readonly CustomStyleProperty<string> durationProperty = new("--wipe-duration");

    private Color fillColor = Color.black;
    private float duration = DefaultDuration;

    private Phase phase = Phase.Hidden;
    // Progress of the current phase, from 0 to 1
    private float progress;

    public SlantedWipe()
    {
        pickingMode = PickingMode.Position;
        style.display = DisplayStyle.None;
        generateVisualContent += Draw;
        RegisterCallback<CustomStyleResolvedEvent>(_ => ReadStyle());
    }

    /// <summary>
    /// Seconds each way, from --wipe-duration
    /// </summary>
    public float Duration => duration;

    /// <summary>
    /// Sweeps the bands in until they cover the element, in game time or <paramref name="unscaledTime"/>
    /// </summary>
    public IEnumerator Cover(bool unscaledTime = false)
    {
        if (phase == Phase.Covered) yield break;
        SetState(Phase.Covering, 0);
        yield return Run(unscaledTime);
        SetState(Phase.Covered, 1);
    }

    /// <summary>
    /// Sweeps the bands out until the element is revealed and hidden, in game time or <paramref name="unscaledTime"/>
    /// </summary>
    public IEnumerator Reveal(bool unscaledTime = false)
    {
        if (phase == Phase.Hidden) yield break;
        SetState(Phase.Revealing, 0);
        yield return Run(unscaledTime);
        SetState(Phase.Hidden, 0);
    }

    private IEnumerator Run(bool unscaledTime)
    {
        while (progress < 1)
        {
            yield return null;
            float deltaTime = Mathf.Min(unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime, MaxFrameDuration);
            SetState(phase, duration > 0 ? progress + deltaTime / duration : 1);
        }
    }

    private void SetState(Phase newPhase, float newProgress)
    {
        phase = newPhase;
        progress = Mathf.Clamp01(newProgress);
        style.display = phase == Phase.Hidden ? DisplayStyle.None : DisplayStyle.Flex;
        MarkDirtyRepaint();
    }

    private void ReadStyle()
    {
        ICustomStyle custom = customStyle;
        if (custom.TryGetValue(fillColorProperty, out Color fill)) fillColor = fill;
        if (custom.TryGetValue(durationProperty, out string time) && TryParseSeconds(time, out float seconds)) duration = seconds;
        MarkDirtyRepaint();
    }

    private static bool TryParseSeconds(string time, out float seconds)
    {
        time = time.Trim();
        float scale = 1;
        if (time.EndsWith("ms")) { scale = 0.001f; time = time[..^2]; }
        else if (time.EndsWith("s")) time = time[..^1];
        bool parsed = float.TryParse(time, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out seconds);
        seconds *= scale;
        return parsed && seconds >= 0;
    }

    // The sweep of a band, delayed by its rank, eased in and out
    private float BandProgress(int band)
    {
        float delay = Stagger * bandOrder[band] / (bandOrder.Length - 1);
        float t = Mathf.Clamp01((progress - delay) / (1 - Stagger));
        return t * t * (3 - 2 * t);
    }

    private void Draw(MeshGenerationContext context)
    {
        Rect rect = contentRect;
        if (phase == Phase.Hidden || rect.width <= 0 || rect.height <= 0) return;
        Painter2D painter = context.painter2D;
        if (phase == Phase.Covered)
        {
            Fill(painter, fillColor, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax));
            return;
        }

        int count = bandOrder.Length;
        float height = rect.height / count;
        // The edges are cut at 45°: an edge goes back by the band's height from its top to its bottom
        float slant = height + BandOverlap;
        // The distance a top corner travels for the band to go from out on the left to out on the right
        float travel = rect.width + slant;
        for (int i = 0; i < count; i++)
        {
            float top = rect.yMin + i * height;
            float bottom = Mathf.Min(top + slant, rect.yMax);
            float edgeSlant = bottom - top;
            float sweep = BandProgress(i) * travel;
            // The top corners of the band's back and front edges
            float back = phase == Phase.Covering ? rect.xMin : rect.xMin + sweep;
            float front = phase == Phase.Covering ? rect.xMin + sweep : rect.xMin + travel;
            if (front <= back) continue;
            Fill(painter, fillColor, new Vector2(back, top), new Vector2(front, top),
                new Vector2(front - edgeSlant, bottom), new Vector2(back - edgeSlant, bottom));
        }
    }

    private static void Fill(Painter2D painter, Color color, params Vector2[] polygon)
    {
        painter.BeginPath();
        painter.MoveTo(polygon[0]);
        for (int i = 1; i < polygon.Length; i++) painter.LineTo(polygon[i]);
        painter.ClosePath();
        painter.fillColor = color;
        painter.Fill();
    }
}
