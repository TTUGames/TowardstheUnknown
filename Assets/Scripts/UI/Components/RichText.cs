using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Colors the tags of the localized texts for the element showing them
/// </summary>
public static class RichText
{
    private static readonly CustomStyleProperty<Color> highlightColorProperty = new("--highlight-color");

    /// <summary>
    /// Colors the damage (D) and block (B) tags of a text with <c>--highlight-color</c>, set on the element itself
    /// (customStyle doesn't see the inherited ones), or removes them if the element has none
    /// </summary>
    public static string Highlight(VisualElement element, string text)
    {
        string open = element.customStyle.TryGetValue(highlightColorProperty, out Color color)
            ? "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" : "";
        string close = open == "" ? "" : "</color>";
        return text?.Replace("<D>", open).Replace("</D>", close).Replace("<B>", open).Replace("</B>", close);
    }
}
