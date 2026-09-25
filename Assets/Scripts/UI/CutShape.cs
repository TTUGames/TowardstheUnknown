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

    /// <param name="slantedBlur">The backdrop blur leaving the cut corners out (Assets/UI/Filters/SlantedBlur.asset)</param>
    public static void AttachAll(VisualElement root, FilterFunctionDefinition slantedBlur)
    {
        root.Query(className: ClassName).ForEach(element => new CutShape(element, slantedBlur));
    }

    private CutShape(VisualElement element, FilterFunctionDefinition slantedBlur)
    {
        this.element = element;
        this.slantedBlur = slantedBlur;
        element.generateVisualContent += Draw;
        element.RegisterCallback<CustomStyleResolvedEvent>(_ => ReadStyle());
        element.RegisterCallback<GeometryChangedEvent>(_ => UpdateBackdrop());
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
        UpdateBackdrop();
        element.MarkDirtyRepaint();
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

    private float CutSize(Vector2 size) => Mathf.Min(cutSize, size.x / 2, size.y / 2);

    private void Draw(MeshGenerationContext context)
    {
        Rect rect = new Rect(Vector2.zero, element.layout.size);
        if (rect.width <= 0 || rect.height <= 0) return;
        Painter2D painter = context.painter2D;
        if (fillColor.a > 0)
        {
            TracePath(painter, rect, 0);
            painter.fillColor = fillColor;
            painter.Fill();
        }
        if (lineColor.a > 0 && lineWidth > 0)
        {
            TracePath(painter, rect, lineWidth / 2);
            painter.strokeColor = lineColor;
            painter.lineWidth = lineWidth;
            painter.lineJoin = LineJoin.Miter;
            painter.Stroke();
        }
    }

    /// <summary>
    /// Traces the outline of the shape, inset by the given distance so that a stroke stays inside the element
    /// </summary>
    private void TracePath(Painter2D painter, Rect rect, float inset)
    {
        // Insetting a 45° edge by d shortens the cut by d * (sqrt 2 - 1) along the sides
        float c = Mathf.Max(0, CutSize(rect.size) - inset * (Mathf.Sqrt(2) - 1));
        float l = rect.xMin + inset, r = rect.xMax - inset, t = rect.yMin + inset, b = rect.yMax - inset;
        painter.BeginPath();
        painter.MoveTo(new Vector2(l + (topLeft ? c : 0), t));
        painter.LineTo(new Vector2(r - (topRight ? c : 0), t));
        if (topRight) painter.LineTo(new Vector2(r, t + c));
        painter.LineTo(new Vector2(r, b - (bottomRight ? c : 0)));
        if (bottomRight) painter.LineTo(new Vector2(r - c, b));
        painter.LineTo(new Vector2(l + (bottomLeft ? c : 0), b));
        if (bottomLeft) painter.LineTo(new Vector2(l, b - c));
        painter.LineTo(new Vector2(l, t + (topLeft ? c : 0)));
        painter.ClosePath();
    }
}
