using UnityEngine.UIElements;

public static class ElementVisibility
{
    /// <summary>
    /// Whether the element is drawn: on a panel, visible (an inherited visibility, which stays visible while a screen
    /// fades in or out) and neither it nor an ancestor out of the layout. Lets the animations of a hidden element pause
    /// </summary>
    public static bool IsShown(this VisualElement element)
    {
        if (element.panel == null || element.resolvedStyle.visibility == Visibility.Hidden) return false;
        for (VisualElement e = element; e != null; e = e.hierarchy.parent)
            if (e.resolvedStyle.display == DisplayStyle.None) return false;
        return true;
    }
}
