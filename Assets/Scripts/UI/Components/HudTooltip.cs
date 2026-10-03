using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A tooltip shared by the elements registered on it: hovering one shows its text after <see cref="Delay"/> (or the
/// registration's delay), near it or where the USS places the tooltip; leaving it, removing it or blocking the tooltip (a menu opens) hides it. An element
/// hovered inside another registered one takes over, and gives the tooltip back to it when left
/// </summary>
[UxmlElement]
public partial class HudTooltip : SlantedLabel
{
    // Between the pointer entering an element and its tooltip showing (UssTime), unless the registration gives its own
    private static readonly CustomStyleProperty<string> delayProperty = new("--tooltip-delay");
    // Between the element and the tooltip, and between the tooltip and the edges of its parent
    private const float Gap = 10;
    private const float EdgeMargin = 16;
    private static readonly CustomStyleProperty<Color> statColorProperty = new("--tooltip-stat-color");
    // The size of a text's details, in percent of the text's
    private static readonly CustomStyleProperty<float> detailsSizeProperty = new("--tooltip-details-size");

    /// <summary>
    /// The sizes of the original's infobox (TimelinePanel), in points, set on the tooltip by the Classic's style:
    /// its title, its stat lines and their spacing
    /// </summary>
    public static readonly CustomStyleProperty<float> TitleSize = new("--tooltip-title-size");
    public static readonly CustomStyleProperty<float> StatSize = new("--tooltip-stat-size");
    public static readonly CustomStyleProperty<float> StatLineHeight = new("--tooltip-stat-line-height");

    /// <summary>
    /// The colors of the stats in a tooltip's text (<see cref="Tint"/>), set on the tooltip itself
    /// </summary>
    public static readonly CustomStyleProperty<Color> HealthColor = new("--tooltip-health-color");
    public static readonly CustomStyleProperty<Color> ArmorColor = new("--tooltip-armor-color");
    public static readonly CustomStyleProperty<Color> EnergyColor = new("--tooltip-energy-color");
    public static readonly CustomStyleProperty<Color> MovementColor = new("--tooltip-movement-color");
    // A value raised or lowered by a status (the skills' damage under AttackUp or AttackDown)
    public static readonly CustomStyleProperty<Color> BuffColor = new("--tooltip-buff-color");
    public static readonly CustomStyleProperty<Color> DebuffColor = new("--tooltip-debuff-color");

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
        public Func<long> delay;
    }

    // The hovered registered elements, the innermost last
    private readonly List<Registration> hovered = new();
    private Registration shown;
    private IVisualElementScheduledItem pendingShow;
    private bool blocked;
    private Color? statColor;

    /// <summary>
    /// The color of a text's secondary lines: <c>--tooltip-stat-color</c>, set on the tooltip itself (customStyle doesn't
    /// see the inherited ones), or else the text's color
    /// </summary>
    public Color StatColor => statColor ?? resolvedStyle.color;

    /// <summary>
    /// <paramref name="text"/> in the stat color <paramref name="color"/> of this tooltip, or as it is if the tooltip
    /// has none
    /// </summary>
    public string Tint(string text, CustomStyleProperty<Color> color) =>
        customStyle.TryGetValue(color, out Color value) ? $"<color=#{ColorUtility.ToHtmlStringRGBA(value)}>{text}</color>" : text;

    /// <summary>
    /// <paramref name="text"/> in a rich text <paramref name="tag"/> (size, line-height) set to the value of
    /// <paramref name="property"/> on this tooltip followed by <paramref name="unit"/>, or as it is if the tooltip has none
    /// </summary>
    public string Wrap(string text, string tag, CustomStyleProperty<float> property, string unit = "") =>
        customStyle.TryGetValue(property, out float value)
            ? $"<{tag}={value.ToString(CultureInfo.InvariantCulture)}{unit}>{text}</{tag}>"
            : text;

    /// <summary>
    /// A tooltip's text: its title in bold, its body, then its details smaller (<c>--tooltip-details-size</c>) if any
    /// </summary>
    public string Format(string title, string body, string details = null) =>
        $"<b>{title}</b>\n{body}" + (string.IsNullOrEmpty(details) ? "" : "\n" + Wrap(details, "size", detailsSizeProperty, "%"));

    /// <summary>
    /// The entity's status effects with their remaining turns, on one line; empty without any
    /// </summary>
    public static string StatusLine(EntityStats stats) =>
        string.Join("   ", stats.StatusEffects.Select(status => $"{Localization.UI("Status" + status.Data.name)} ({status.Duration})"));

    public HudTooltip()
    {
        pickingMode = PickingMode.Ignore;
        // The size is known once the new text is laid out
        RegisterCallback<GeometryChangedEvent>(_ => Place());
        RegisterCallback<CustomStyleResolvedEvent>(_ =>
            statColor = customStyle.TryGetValue(statColorProperty, out Color color) ? color : (Color?)null);
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
    /// an empty text. Call <see cref="Refresh"/> when the text may have changed. <paramref name="delay"/> gives the
    /// milliseconds before it shows, <see cref="Delay"/> if none
    /// </summary>
    public void Register(VisualElement target, Func<string> text, Placement placement = Placement.Above, Func<long> delay = null)
    {
        var registration = new Registration { target = target, text = text, placement = placement, delay = delay };
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
        ShowLater(registration);
    }

    private void ShowLater(Registration registration)
    {
        long delay = registration.delay?.Invoke() ?? customStyle.Milliseconds(delayProperty, 0);
        if (delay <= 0) Show(registration);
        else pendingShow = schedule.Execute(() => Show(registration)).StartingIn(delay);
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
        else ShowLater(outer);
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
