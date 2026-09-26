using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A parallelogram bar showing health and armor: the health just lost fades behind the bar, which flashes when hit,
/// and the health an action would take blinks at the end of the bar
/// </summary>
[UxmlElement]
public partial class HealthBar : VisualElement
{
    private const long FlashDuration = 120;
    private const long PreviewBlink = 350;

    private readonly SlantedPanel trail;
    private readonly SlantedPanel fill;
    private readonly SlantedPanel preview;
    private readonly SlantedPanel shield;
    private readonly Label label;
    private int health = -1;
    private int maxHealth = 1;
    private int previewed;
    private IVisualElementScheduledItem blinking;

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
        foreach (VisualElement child in Children()) child.pickingMode = PickingMode.Ignore;
    }

    public void Set(int current, int armor, int max)
    {
        // The trail follows the bar late (transitions of Hud.uss), showing the health just lost
        Length width = Length.Percent(100f * current / max);
        fill.style.width = width;
        trail.style.width = width;
        shield.style.width = Length.Percent(100f * Mathf.Min(armor, max) / max);
        label.text = current + " (" + armor + ") / " + max;

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
