using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Keeps the game in an area between 16:10 and 16:9, with black bars around it on a wider or taller screen: the camera and
/// the UI Toolkit documents. A screen between the two ratios is filled whole; the level design is made for no wider than 16:9
/// </summary>
public static class Letterbox
{
    public const float WideAspect = 16f / 9f;
    public const float NarrowAspect = 16f / 10f;

    private static readonly HashSet<IPanel> fittedPanels = new();

    // Play mode starts without a domain reload: forget the panels of the previous session
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => fittedPanels.Clear();

    /// <summary>
    /// The aspect ratio of the game's area on a screen of this size
    /// </summary>
    public static float Aspect(float width, float height) => Mathf.Clamp(width / height, NarrowAspect, WideAspect);

    /// <summary>
    /// The game's area on a screen of this size, in viewport coordinates (from the bottom left corner, 0 to 1)
    /// </summary>
    public static Rect Viewport(float width, float height)
    {
        float scaleHeight = width / height / Aspect(width, height);
        return scaleHeight < 1f
            ? new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight)
            : new Rect((1f - 1f / scaleHeight) / 2f, 0, 1f / scaleHeight, 1f);
    }

    /// <summary>
    /// The factor of an orthographic camera's size that shows as wide a view as in 16:9, taller on a narrower area: the board
    /// keeps its width whole
    /// </summary>
    public static float CameraSizeScale => WideAspect / Aspect(Screen.width, Screen.height);

    /// <summary>
    /// Places every document of the element's panel in the game's area, now and whenever the screen size changes
    /// </summary>
    public static void Fit(VisualElement element)
    {
        IPanel panel = element.panel;
        if (panel == null) return;
        if (fittedPanels.Add(panel))
            panel.visualTree.RegisterCallback<GeometryChangedEvent>(_ => FitDocuments(panel.visualTree));
        FitDocuments(panel.visualTree);
    }

    private static void FitDocuments(VisualElement visualTree)
    {
        Vector2 size = visualTree.layout.size;
        if (size.x <= 0 || size.y <= 0) return;
        Rect area = Viewport(size.x, size.y);
        foreach (VisualElement document in visualTree.Children())
        {
            document.style.position = Position.Absolute;
            document.style.left = area.x * size.x;
            document.style.top = (1f - area.yMax) * size.y;
            document.style.width = area.width * size.x;
            document.style.height = area.height * size.y;
        }
    }
}
