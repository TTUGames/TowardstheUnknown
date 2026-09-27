using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A parallelogram bar showing health and armor: the health just lost fades behind the bar, which flashes when hit,
/// and the health an action would take blinks at the end of the bar. A filter animates the health and the armor lightly
/// (UI/Filters/HealthBar.shader): a current and a sheen on the health, drifting stripes and a livelier sheen on the armor.
/// An inherited <c>--ui-effects</c> of 0 (the Classic edition's style) leaves the filter out. The text is also written in
/// three labels (health and armor, slash, maximum), hidden unless a style places them apart, as the original's
/// </summary>
[UxmlElement]
public partial class HealthBar : VisualElement
{
    private const long FlashDuration = 120;
    private const long PreviewBlink = 350;
    private const long EffectInterval = 33;
    private static readonly CustomStyleProperty<float> effectsProperty = new("--ui-effects");

    private readonly SlantedPanel trail;
    private readonly SlantedPanel fill;
    private readonly SlantedPanel preview;
    private readonly SlantedPanel shield;
    private readonly Label label;
    private readonly Label currentLabel;
    private readonly Label maxLabel;
    private int health = -1;
    private int maxHealth = 1;
    private int previewed;
    private IVisualElementScheduledItem blinking;
    private bool effects = true;

    /// <summary>
    /// The armor's part of the bar, over the health's left end
    /// </summary>
    public VisualElement Shield => shield;

    public HealthBar()
    {
        AddToClassList("health-bar");
        const Corners parallelogram = Corners.TopLeft | Corners.BottomRight;
        Add(trail = new SlantedPanel(parallelogram, "health-bar__trail"));
        Add(fill = new SlantedPanel(parallelogram, "health-bar__fill"));
        Add(preview = new SlantedPanel(parallelogram, "health-bar__preview"));
        Add(shield = new SlantedPanel(parallelogram, "health-bar__shield"));
        Add(label = new Label());
        label.AddToClassList("health-bar__text");
        label.AddToClassList("stretch");
        var split = new VisualElement();
        split.AddToClassList("health-bar__split");
        split.Add(currentLabel = new Label());
        currentLabel.AddToClassList("health-bar__current");
        var slash = new Label("/");
        slash.AddToClassList("health-bar__slash");
        split.Add(slash);
        split.Add(maxLabel = new Label());
        maxLabel.AddToClassList("health-bar__max");
        foreach (VisualElement part in split.Children()) part.pickingMode = PickingMode.Ignore;
        Add(split);
        foreach (VisualElement child in Children()) child.pickingMode = PickingMode.Ignore;
        // Redrawn about 30 times a second while shown
        schedule.Execute(Animate).Every(EffectInterval);
        RegisterCallback<CustomStyleResolvedEvent>(_ =>
            effects = !customStyle.TryGetValue(effectsProperty, out float value) || value > 0);
    }

    private void Animate()
    {
        FilterFunctionDefinition effect = GameAssets.Instance.healthBarEffect;
        if (!effects)
        {
            fill.style.filter = StyleKeyword.Null;
            shield.style.filter = StyleKeyword.Null;
            return;
        }
        if (effect == null || panel == null) return;
        SetEffect(fill, effect, 0);
        SetEffect(shield, effect, 1);
    }

    private static void SetEffect(VisualElement part, FilterFunctionDefinition effect, float mode)
    {
        Vector2 size = part.layout.size;
        if (size.x <= 0 || size.y <= 0) return;
        var function = new FilterFunction(effect);
        function.AddParameter(new FilterParameter(mode));
        function.AddParameter(new FilterParameter(size.x));
        function.AddParameter(new FilterParameter(size.y));
        function.AddParameter(new FilterParameter(Time.unscaledTime % 1000));
        part.style.filter = new StyleList<FilterFunction>(new List<FilterFunction> { function });
    }

    public void Set(int current, int armor, int max)
    {
        // The trail follows the bar late (transitions of Hud.uss), showing the health just lost
        Length width = Length.Percent(100f * current / max);
        fill.style.width = width;
        trail.style.width = width;
        shield.style.width = Length.Percent(100f * Mathf.Min(armor, max) / max);
        label.text = current + " (" + armor + ") / " + max;
        currentLabel.text = current + " (" + armor + ")";
        maxLabel.text = max.ToString();

        if (health >= 0 && current < health)
        {
            AddToClassList("health-bar--hit");
            schedule.Execute(() => RemoveFromClassList("health-bar--hit")).StartingIn(FlashDuration);
        }
        health = current;
        maxHealth = max;
        PlacePreview();
    }

    /// <summary>
    /// Blinks the health an action would take at the end of the bar, none if 0
    /// </summary>
    public void Preview(int healthLost)
    {
        previewed = healthLost;
        PlacePreview();
        if (previewed > 0) blinking ??= schedule.Execute(() => preview.ToggleInClassList("health-bar__preview--dim")).Every(PreviewBlink);
        else
        {
            blinking?.Pause();
            blinking = null;
            preview.RemoveFromClassList("health-bar__preview--dim");
        }
    }

    private void PlacePreview()
    {
        int lost = Mathf.Clamp(previewed, 0, Mathf.Max(0, health));
        preview.EnableInClassList("hidden", lost <= 0);
        preview.style.left = Length.Percent(100f * (health - lost) / maxHealth);
        preview.style.width = Length.Percent(100f * lost / maxHealth);
    }
}
