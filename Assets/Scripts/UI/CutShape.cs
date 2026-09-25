using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Draws the slanted shape of the game's UI (a rectangle with corners cut at 45°) on the elements with the cut class.
/// It is configured in USS: --cut-size (px), --cut-corners ("top-left bottom-right"...), --fill-color, --line-color,
/// --line-width (px) and --backdrop-blur (px, 48 at most), the blur following the cut corners
/// </summary>
public class CutShape
{
    public const string ClassName = "cut";
    private const string AttachedClassName = "cut--attached";

    private static readonly CustomStyleProperty<float> cutSizeProperty = new("--cut-size");
    private static readonly CustomStyleProperty<string> cutCornersProperty = new("--cut-corners");
    private static readonly CustomStyleProperty<Color> fillColorProperty = new("--fill-color");
    private static readonly CustomStyleProperty<Color> lineColorProperty = new("--line-color");
    private static readonly CustomStyleProperty<float> lineWidthProperty = new("--line-width");
    private static readonly CustomStyleProperty<float> backdropBlurProperty = new("--backdrop-blur");

    private readonly VisualElement element;
    private readonly FilterFunctionDefinition slantedBlur;
    private float cutSize;
    private bool topLeft, topRight, bottomRight, bottomLeft;
    private Color fillColor;
    private Color lineColor;
    private float lineWidth;
    private float backdropBlur;
    private VectorImage image;

    /// <param name="slantedBlur">The backdrop blur leaving the cut corners out (Assets/UI/Filters/SlantedBlur.asset)</param>
    public static void AttachAll(VisualElement root, FilterFunctionDefinition slantedBlur)
    {
        root.Query(className: ClassName).ForEach(element => {
            if (element.ClassListContains(AttachedClassName)) return;
            element.AddToClassList(AttachedClassName);
            new CutShape(element, slantedBlur);
        });
    }

    private CutShape(VisualElement element, FilterFunctionDefinition slantedBlur)
    {
        this.element = element;
        this.slantedBlur = slantedBlur;
        element.RegisterCallback<CustomStyleResolvedEvent>(_ => ReadStyle());
        element.RegisterCallback<GeometryChangedEvent>(_ => Redraw());
        element.RegisterCallback<DetachFromPanelEvent>(_ => {
            if (image != null) Object.Destroy(image);
            image = null;
        });
        ReadStyle();
    }

    private void ReadStyle()
    {
        ICustomStyle style = element.customStyle;
        cutSize = style.TryGetValue(cutSizeProperty, out float size) ? size : 0;
        string corners = style.TryGetValue(cutCornersProperty, out string value) ? value : "";
        topLeft = corners.Contains("top-left");
        topRight = corners.Contains("top-right");
        bottomRight = corners.Contains("bottom-right");
        bottomLeft = corners.Contains("bottom-left");
        fillColor = style.TryGetValue(fillColorProperty, out Color fill) ? fill : Color.clear;
        lineColor = style.TryGetValue(lineColorProperty, out Color line) ? line : Color.clear;
        lineWidth = style.TryGetValue(lineWidthProperty, out float width) ? width : 0;
        backdropBlur = style.TryGetValue(backdropBlurProperty, out float blur) ? blur : 0;
        Redraw();
    }

    private void Redraw()
    {
        UpdateBackdrop();
        UpdateImage();
    }

    private void UpdateBackdrop()
    {
        Vector2 size = element.layout.size;
        if (backdropBlur <= 0 || size.x <= 0 || size.y <= 0)
        {
            element.style.backdropFilter = StyleKeyword.Null;
            return;
        }
        var blur = new FilterFunction(slantedBlur);
        // A filter function takes 4 parameters at most: the corner flags are packed with the cut size
        int corners = (topLeft ? 1 : 0) | (topRight ? 2 : 0) | (bottomRight ? 4 : 0) | (bottomLeft ? 8 : 0);
        blur.AddParameter(new FilterParameter(backdropBlur));
        blur.AddParameter(new FilterParameter(Mathf.Round(CutSize(size)) + 4096 * corners));
        blur.AddParameter(new FilterParameter(size.x));
        blur.AddParameter(new FilterParameter(size.y));
        element.style.backdropFilter = new StyleList<FilterFunction>(new List<FilterFunction> { blur });
    }

    private float CutSize(Vector2 size) => Mathf.Min(cutSize, size.x, size.y);

    /// <summary>
    /// Draws the shape in a vector image set as the element's background, drawn under its text and children
    /// </summary>
    private void UpdateImage()
    {
        Rect rect = new Rect(Vector2.zero, element.layout.size);
        if (rect.width <= 0 || rect.height <= 0 || (fillColor.a <= 0 && (lineColor.a <= 0 || lineWidth <= 0)))
        {
            element.style.backgroundImage = StyleKeyword.Null;
            return;
        }
        using var painter = new Painter2D();
        Draw(painter, rect);
        // A new image: the element would keep drawing the previous content of a modified one
        if (image != null) Object.Destroy(image);
        image = ScriptableObject.CreateInstance<VectorImage>();
        painter.SaveToVectorImage(image);
        element.style.backgroundImage = new StyleBackground(image);
        element.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
    }

    private void Draw(Painter2D painter, Rect rect)
    {
        List<Vector2> outline = Outline(rect);
        if (fillColor.a > 0)
        {
            painter.BeginPath();
            TracePolygon(painter, outline);
            painter.fillColor = fillColor;
            painter.Fill();
        }
        if (lineColor.a > 0 && lineWidth > 0)
        {
            // The line is a ring between the outline and the outline inset by its width: its corners stay sharp
            painter.BeginPath();
            TracePolygon(painter, outline);
            TracePolygon(painter, Inset(outline, lineWidth));
            painter.fillColor = lineColor;
            painter.Fill(FillRule.OddEven);
        }
    }

    /// <summary>
    /// The corners of the shape, clockwise from the top left one
    /// </summary>
    private List<Vector2> Outline(Rect rect)
    {
        float c = CutSize(rect.size);
        float l = rect.xMin, r = rect.xMax, t = rect.yMin, b = rect.yMax;
        var points = new List<Vector2>();
        AddPoint(points, new Vector2(l + (topLeft ? c : 0), t));
        AddPoint(points, new Vector2(r - (topRight ? c : 0), t));
        if (topRight) AddPoint(points, new Vector2(r, t + c));
        AddPoint(points, new Vector2(r, b - (bottomRight ? c : 0)));
        if (bottomRight) AddPoint(points, new Vector2(r - c, b));
        AddPoint(points, new Vector2(l + (bottomLeft ? c : 0), b));
        if (bottomLeft) AddPoint(points, new Vector2(l, b - c));
        AddPoint(points, new Vector2(l, t + (topLeft ? c : 0)));
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
