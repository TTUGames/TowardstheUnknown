using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The corners of a slanted shape, cut at 45°
/// </summary>
[Flags]
public enum Corners
{
    None = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomRight = 4,
    BottomLeft = 8,
    All = TopLeft | TopRight | BottomRight | BottomLeft,
}

/// <summary>
/// Draws the game's slanted shape (a rectangle with corners cut at 45°, up to parallelograms and diamonds) as the
/// background of an element, under its text and children, and blurs what is behind it following the cut corners.
/// The components (SlantedPanel, SlantedButton...) set its corners and dots; USS sets the rest with custom properties:
/// --cut-size, --fill-color, --line-color, --line-width, --backdrop-blur (48 at most), --shadow-offset and --shadow-color.
/// A dot (child element) ends the top line at the dotted corners, the line breaking in dashes next to it, as in the original design
/// </summary>
public class CutShape
{
    // Dashes of the top line from a dot, in points: a dash, a gap, a dash, a gap, then the line
    private static readonly float[] dashPattern = { 12, 8, 34, 12 };

    private static readonly CustomStyleProperty<float> cutSizeProperty = new("--cut-size");
    private static readonly CustomStyleProperty<Color> fillColorProperty = new("--fill-color");
    private static readonly CustomStyleProperty<Color> lineColorProperty = new("--line-color");
    private static readonly CustomStyleProperty<float> lineWidthProperty = new("--line-width");
    private static readonly CustomStyleProperty<float> backdropBlurProperty = new("--backdrop-blur");
    private static readonly CustomStyleProperty<float> shadowOffsetProperty = new("--shadow-offset");
    private static readonly CustomStyleProperty<Color> shadowColorProperty = new("--shadow-color");

    private readonly VisualElement element;
    private Corners corners;
    private Corners dots;
    private float cutSize;
    private Color fillColor;
    private Color lineColor;
    private float lineWidth;
    private float backdropBlur;
    private float shadowOffset;
    private Color shadowColor;
    private VectorImage image;
    // The dots are children: the background of an element is clipped to its bounds
    private VisualElement leftDot, rightDot;

    public CutShape(VisualElement element)
    {
        this.element = element;
        element.RegisterCallback<CustomStyleResolvedEvent>(_ => ReadStyle());
        element.RegisterCallback<GeometryChangedEvent>(_ => Redraw());
        element.RegisterCallback<DetachFromPanelEvent>(_ => {
            DestroyImage();
            image = null;
        });
    }

    public Corners Corners
    {
        get => corners;
        set { corners = value; Redraw(); }
    }

    /// <summary>
    /// The top corners ending the top line with a dot
    /// </summary>
    public Corners Dots
    {
        get => dots;
        set
        {
            dots = value;
            SetDot(ref leftDot, (dots & Corners.TopLeft) != 0, "dot--left");
            SetDot(ref rightDot, (dots & Corners.TopRight) != 0, "dot--right");
            Redraw();
        }
    }

    private void SetDot(ref VisualElement dot, bool shown, string side)
    {
        if (shown && dot == null)
        {
            dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.AddToClassList("dot");
            dot.AddToClassList(side);
            element.hierarchy.Add(dot);
        }
        else if (!shown && dot != null)
        {
            dot.RemoveFromHierarchy();
            dot = null;
        }
    }

    private void ReadStyle()
    {
        ICustomStyle style = element.customStyle;
        cutSize = style.TryGetValue(cutSizeProperty, out float size) ? size : 0;
        fillColor = style.TryGetValue(fillColorProperty, out Color fill) ? fill : Color.clear;
        lineColor = style.TryGetValue(lineColorProperty, out Color line) ? line : Color.clear;
        lineWidth = style.TryGetValue(lineWidthProperty, out float width) ? width : 0;
        backdropBlur = style.TryGetValue(backdropBlurProperty, out float blur) ? blur : 0;
        shadowOffset = style.TryGetValue(shadowOffsetProperty, out float offset) ? offset : 0;
        shadowColor = style.TryGetValue(shadowColorProperty, out Color shadow) ? shadow : new Color(0, 0, 0, 0.5f);
        Redraw();
    }

    private void Redraw()
    {
        UpdateBackdrop();
        UpdateImage();
    }

    private bool Has(Corners corner) => (corners & corner) != 0;

    private float CutSize(Vector2 size) => corners == Corners.None ? 0 : Mathf.Min(cutSize, size.x, size.y);

    private void UpdateBackdrop()
    {
        Vector2 size = element.layout.size;
        FilterFunctionDefinition slantedBlur = GameAssets.Instance.slantedBlur;
        if (backdropBlur <= 0 || size.x <= 0 || size.y <= 0 || slantedBlur == null)
        {
            element.style.backdropFilter = StyleKeyword.Null;
            return;
        }
        var blur = new FilterFunction(slantedBlur);
        // A filter function takes 4 parameters at most: the corner flags are packed with the cut size
        blur.AddParameter(new FilterParameter(backdropBlur));
        blur.AddParameter(new FilterParameter(CutSize(size) + 4096 * (int)corners));
        blur.AddParameter(new FilterParameter(size.x));
        blur.AddParameter(new FilterParameter(size.y));
        element.style.backdropFilter = new StyleList<FilterFunction>(new List<FilterFunction> { blur });
    }

    /// <summary>
    /// Draws the shape in a vector image set as the element's background, drawn under its text and children
    /// </summary>
    private void UpdateImage()
    {
        // The shadow is the shape moved down right: the shape leaves it room in the element, whose background is clipped
        Rect bounds = new Rect(Vector2.zero, element.layout.size);
        Rect rect = new Rect(Vector2.zero, bounds.size - Vector2.one * shadowOffset);
        bool hasLine = lineColor.a > 0 && lineWidth > 0;
        if (rect.width <= 0 || rect.height <= 0 || (fillColor.a <= 0 && !hasLine))
        {
            element.style.backgroundImage = StyleKeyword.Null;
            return;
        }
        List<Vector2> outline = Outline(rect);
        if (outline.Count < 3)
        {
            element.style.backgroundImage = StyleKeyword.Null;
            return;
        }
        using var painter = new Painter2D();
        // The image bounds are those of its content: a nearly invisible rectangle sets them to the element's
        Fill(painter, new List<Vector2> { bounds.min, new(bounds.xMax, 0), bounds.max, new(0, bounds.yMax) }, new Color(0, 0, 0, 1 / 255f));
        // The shadow is drawn under the shape: it is meant for opaque shapes
        if (shadowOffset > 0)
            Fill(painter, Offset(outline, new Vector2(shadowOffset, shadowOffset)), shadowColor);
        if (fillColor.a > 0)
            Fill(painter, outline, fillColor);
        if (hasLine)
            DrawLine(painter, outline);

        // A new image: the element would keep drawing the previous content of a modified one
        DestroyImage();
        image = ScriptableObject.CreateInstance<VectorImage>();
        painter.SaveToVectorImage(image);
        element.style.backgroundImage = new StyleBackground(image);
        element.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
    }

    // The components also draw in the editor's previews (UI Builder, inspector)
    private void DestroyImage()
    {
        if (image == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(image);
        else UnityEngine.Object.DestroyImmediate(image);
    }

    /// <summary>
    /// The corners of the shape, clockwise from the start of the top line
    /// </summary>
    private List<Vector2> Outline(Rect rect)
    {
        float c = CutSize(rect.size);
        float l = rect.xMin, r = rect.xMax, t = rect.yMin, b = rect.yMax;
        var points = new List<Vector2>();
        AddPoint(points, new Vector2(l + (Has(Corners.TopLeft) ? c : 0), t));
        AddPoint(points, new Vector2(r - (Has(Corners.TopRight) ? c : 0), t));
        if (Has(Corners.TopRight)) AddPoint(points, new Vector2(r, t + c));
        AddPoint(points, new Vector2(r, b - (Has(Corners.BottomRight) ? c : 0)));
        if (Has(Corners.BottomRight)) AddPoint(points, new Vector2(r - c, b));
        AddPoint(points, new Vector2(l + (Has(Corners.BottomLeft) ? c : 0), b));
        if (Has(Corners.BottomLeft)) AddPoint(points, new Vector2(l, b - c));
        AddPoint(points, new Vector2(l, t + (Has(Corners.TopLeft) ? c : 0)));
        if (points.Count > 1 && (points[^1] - points[0]).sqrMagnitude < 0.01f)
            points.RemoveAt(points.Count - 1);
        return points;
    }

    // A cut as long as a side makes two corners meet: a duplicate point would have no direction
    private static void AddPoint(List<Vector2> points, Vector2 point)
    {
        if (points.Count == 0 || (point - points[^1]).sqrMagnitude > 0.01f)
            points.Add(point);
    }

    /// <summary>
    /// The line is a ring between the outline and the outline inset by its width, so that its corners stay sharp.
    /// With dots, the top line breaks in dashes next to them: the ring is then cut in pieces around the gaps
    /// </summary>
    private void DrawLine(Painter2D painter, List<Vector2> outline)
    {
        List<Vector2> inner = Inset(outline, lineWidth);
        List<(float from, float to)> gaps = TopGaps(outline, inner);
        if (gaps.Count == 0)
        {
            painter.BeginPath();
            TracePolygon(painter, outline);
            TracePolygon(painter, inner);
            painter.fillColor = lineColor;
            painter.Fill(FillRule.OddEven);
        }
        else
        {
            float top = outline[0].y, innerTop = inner[0].y;
            // The piece going around the shape, from the last gap to the first one
            var ring = new List<Vector2> { new(gaps[^1].to, top) };
            for (int i = 1; i < outline.Count; i++) ring.Add(outline[i]);
            ring.Add(outline[0]);
            ring.Add(new Vector2(gaps[0].from, top));
            ring.Add(new Vector2(gaps[0].from, innerTop));
            ring.Add(inner[0]);
            for (int i = inner.Count - 1; i >= 1; i--) ring.Add(inner[i]);
            ring.Add(new Vector2(gaps[^1].to, innerTop));
            Fill(painter, ring, lineColor);
            // The dashes between two gaps
            for (int i = 0; i < gaps.Count - 1; i++)
                Fill(painter, new List<Vector2> {
                    new(gaps[i].to, top), new(gaps[i + 1].from, top), new(gaps[i + 1].from, innerTop), new(gaps[i].to, innerTop),
                }, lineColor);
        }
    }

    /// <summary>
    /// The gaps of the top line, as x ranges from left to right, next to the dotted corners
    /// </summary>
    private List<(float from, float to)> TopGaps(List<Vector2> outline, List<Vector2> inner)
    {
        var gaps = new List<(float, float)>();
        // Only a horizontal top line breaks
        if (outline.Count < 3 || Mathf.Abs(outline[0].y - outline[1].y) > 0.01f) return gaps;
        float left = Mathf.Max(outline[0].x, inner[0].x), right = Mathf.Min(outline[1].x, inner[1].x);
        float length = dashPattern[0] + dashPattern[1] + dashPattern[2] + dashPattern[3];
        if (right - left < 3 * length) return gaps;
        if ((dots & Corners.TopLeft) != 0)
        {
            gaps.Add((left + dashPattern[0], left + dashPattern[0] + dashPattern[1]));
            gaps.Add((left + length - dashPattern[3], left + length));
        }
        if ((dots & Corners.TopRight) != 0)
        {
            gaps.Add((right - length, right - length + dashPattern[3]));
            gaps.Add((right - dashPattern[0] - dashPattern[1], right - dashPattern[0]));
        }
        return gaps;
    }

    private static void Fill(Painter2D painter, List<Vector2> polygon, Color color)
    {
        painter.BeginPath();
        TracePolygon(painter, polygon);
        painter.fillColor = color;
        painter.Fill();
    }

    private static List<Vector2> Offset(List<Vector2> polygon, Vector2 offset)
    {
        var moved = new List<Vector2>(polygon.Count);
        foreach (Vector2 point in polygon) moved.Add(point + offset);
        return moved;
    }

    /// <summary>
    /// Moves every edge of a convex clockwise polygon inwards by the given distance
    /// </summary>
    private static List<Vector2> Inset(List<Vector2> polygon, float distance)
    {
        var inset = new List<Vector2>(polygon.Count);
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 previous = polygon[(i + polygon.Count - 1) % polygon.Count], point = polygon[i], next = polygon[(i + 1) % polygon.Count];
            Vector2 n1 = InwardNormal(previous, point), n2 = InwardNormal(point, next);
            // The corner moves along the bisector, far enough for both edges to move by the distance
            inset.Add(point + (n1 + n2) * (distance / (1 + Vector2.Dot(n1, n2))));
        }
        return inset;
    }

    // Clockwise on screen (y down), the inside is on the right of an edge
    private static Vector2 InwardNormal(Vector2 from, Vector2 to)
    {
        Vector2 direction = (to - from).normalized;
        return new Vector2(-direction.y, direction.x);
    }

    private static void TracePolygon(Painter2D painter, List<Vector2> polygon)
    {
        painter.MoveTo(polygon[0]);
        for (int i = 1; i < polygon.Count; i++)
            painter.LineTo(polygon[i]);
        painter.ClosePath();
    }
}
