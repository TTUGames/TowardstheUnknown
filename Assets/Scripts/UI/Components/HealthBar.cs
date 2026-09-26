using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// A parallelogram bar showing health and armor: the health just lost fades behind the bar, which flashes when hit,
/// and the health an action would take blinks at the end of the bar. The armor's part shatters into shards when the armor
/// is lost
/// </summary>
[UxmlElement]
public partial class HealthBar : VisualElement
{
    private const long FlashDuration = 120;
    private const long PreviewBlink = 350;
    private const int ShardCount = 9;
    private const int ShardDuration = 550;

    private readonly SlantedPanel trail;
    private readonly SlantedPanel fill;
    private readonly SlantedPanel preview;
    private readonly SlantedPanel shield;
    private readonly Label label;
    private int health = -1;
    private int armor;
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
        if (this.armor > 0 && armor <= 0 && health >= 0) Shatter(Mathf.Min(this.armor, max) / (float)max);
        this.armor = armor;
        health = current;
        maxHealth = max;
        PlacePreview();
    }

    /// <summary>
    /// The armor's part of the bar, of this share of its width, bursts into shards that fly up, spin and fade
    /// </summary>
    private void Shatter(float share)
    {
        float width = resolvedStyle.width * share;
        if (width <= 0 || float.IsNaN(width)) return;
        for (int i = 0; i < ShardCount; i++)
        {
            var shard = new VisualElement { pickingMode = PickingMode.Ignore };
            shard.AddToClassList("health-bar__shard");
            float x = width * (i + Random.value) / ShardCount;
            shard.style.left = x;
            shard.style.top = Random.Range(2f, resolvedStyle.height - 12);
            Add(shard);
            Vector2 flight = new(Random.Range(-40f, 40f) + (x - width / 2) * 0.3f, Random.Range(-70f, -25f));
            float spin = Random.Range(-360f, 360f);
            shard.experimental.animation.Start(0f, 1f, ShardDuration, (element, t) =>
            {
                // Thrown up, then falling back a little
                float eased = 1 - (1 - t) * (1 - t);
                element.style.translate = new Translate(flight.x * eased, flight.y * eased + 60 * t * t);
                element.style.rotate = new Rotate(spin * eased);
                element.style.opacity = 1 - t * t;
            }).OnCompleted(shard.RemoveFromHierarchy);
        }
        AddToClassList("health-bar--armor-break");
        schedule.Execute(() => RemoveFromClassList("health-bar--armor-break")).StartingIn(FlashDuration * 2);
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
