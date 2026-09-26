using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A tooltip shared by the elements registered on it: hovering one shows its text after <see cref="Delay"/>, near it or
/// where the USS places the tooltip; leaving it, removing it or blocking the tooltip (a menu opens) hides it. An element
/// hovered inside another registered one takes over, and gives the tooltip back to it when left
/// </summary>
[UxmlElement]
public partial class HudTooltip : SlantedLabel
{
    /// <summary>
    /// Between the pointer entering an element and its tooltip showing
    /// </summary>
    public const long Delay = 400;
    // Between the element and the tooltip, and between the tooltip and the edges of its parent
    private const float Gap = 10;
    private const float EdgeMargin = 16;

    public enum Placement
    {
        // The tooltip stays where its USS places it
        Styled,
        // Centered above the element, below it if there is no room, kept inside the parent
        Above,
    }

    private class Registration
    {
        public VisualElement target;
        public Func<string> text;
        public Placement placement;
    }

    // The hovered registered elements, the innermost last
    private readonly List<Registration> hovered = new();
    private Registration shown;
    private IVisualElementScheduledItem pendingShow;
    private bool blocked;

    public HudTooltip()
    {
        pickingMode = PickingMode.Ignore;
        // The size is known once the new text is laid out
        RegisterCallback<GeometryChangedEvent>(_ => Place());
    }

    /// <summary>
    /// A blocked tooltip (a menu covers the HUD) hides and ignores the pointer until unblocked
    /// </summary>
    public bool Blocked
    {
        get => blocked;
        set
        {
            blocked = value;
            if (!blocked) return;
            hovered.Clear();
            Hide();
        }
    }

    /// <summary>
    /// Shows <paramref name="text"/> while the pointer is over <paramref name="target"/>; no tooltip when it returns null or
    /// an empty text. Call <see cref="Refresh"/> when the text may have changed
    /// </summary>
    public void Register(VisualElement target, Func<string> text, Placement placement = Placement.Above)
    {
        var registration = new Registration { target = target, text = text, placement = placement };
        target.RegisterCallback<PointerEnterEvent>(_ => Enter(registration));
        target.RegisterCallback<PointerLeaveEvent>(_ => Leave(registration, false));
        // A removed element gets no pointer leave event
        target.RegisterCallback<DetachFromPanelEvent>(_ => Leave(registration, true));
    }

    /// <summary>
    /// Asks the shown tooltip's text again, hiding it if there is none any more
    /// </summary>
    public void Refresh()
    {
        if (shown != null) Show(shown);
    }

    private void Enter(Registration registration)
    {
        if (blocked) return;
        hovered.Remove(registration);
        hovered.Add(registration);
        Hide();
        pendingShow = schedule.Execute(() => Show(registration)).StartingIn(Delay);
    }

    private void Leave(Registration registration, bool removed)
    {
        if (!hovered.Remove(registration) && shown != registration) return;
        bool wasShown = shown == registration;
        Hide();
        if (removed || hovered.Count == 0) return;
        // Back to the element around the one left: at once if a tooltip was shown
        Registration outer = hovered[^1];
        if (wasShown) Show(outer);
        else pendingShow = schedule.Execute(() => Show(outer)).StartingIn(Delay);
    }

    private void Show(Registration registration)
    {
        pendingShow?.Pause();
        string content = blocked ? null : registration.text();
        if (string.IsNullOrEmpty(content))
        {
            Hide();
            return;
        }
        shown = registration;
        text = content;
        AddToClassList("shown");
        Place();
    }

    private void Hide()
    {
        pendingShow?.Pause();
        shown = null;
        RemoveFromClassList("shown");
    }

    /// <summary>
    /// Places the tooltip next to its element, in its parent's space
    /// </summary>
    private void Place()
    {
        if (shown == null || hierarchy.parent == null) return;
        if (shown.placement == Placement.Styled)
        {
            style.left = StyleKeyword.Null;
            style.top = StyleKeyword.Null;
            style.bottom = StyleKeyword.Null;
            return;
        }
        VisualElement parent = hierarchy.parent;
        Rect target = parent.WorldToLocal(shown.target.worldBound);
        Rect area = parent.contentRect;
        float width = layout.width, height = layout.height;
        float x = Mathf.Clamp(target.center.x - width / 2, area.xMin + EdgeMargin, Mathf.Max(area.xMin + EdgeMargin, area.xMax - EdgeMargin - width));
        float y = target.yMin - Gap - height;
        if (y < area.yMin + EdgeMargin) y = Mathf.Min(target.yMax + Gap, area.yMax - EdgeMargin - height);
        style.left = x;
        style.top = y;
        style.bottom = StyleKeyword.Auto;
    }
}
