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
    private const int ShardCount = 18;
    private const int ShardDuration = 1100;
    // Share of the shards' time they stay cracked in place before bursting
    private const float ShardHold = 0.12f;
    // The shards' colors, the shield color from Hud.uss (--shard-fill, --shard-edge)
    private static readonly CustomStyleProperty<Color> shardFillProperty = new("--shard-fill");
    private static readonly CustomStyleProperty<Color> shardEdgeProperty = new("--shard-edge");
    private Color shardFill = new(0.09f, 0.7f, 0.9f, 0.85f);
    private Color shardEdge = Color.white;

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
        RegisterCallback<CustomStyleResolvedEvent>(_ =>
        {
            if (customStyle.TryGetValue(shardFillProperty, out Color fill)) shardFill = fill;
            if (customStyle.TryGetValue(shardEdgeProperty, out Color edge)) shardEdge = edge;
        });
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
    /// The armor's part of the bar, of this share of its width, breaks like glass: shards of random triangles stay cracked
    /// in place for an instant, then burst outwards from its middle, tumble, fall back and fade at the end
    /// </summary>
    private void Shatter(float share)
    {
        float width = resolvedStyle.width * share, height = resolvedStyle.height;
        if (width <= 0 || float.IsNaN(width)) return;
        Vector2 middle = new(width / 2, height / 2);
        for (int i = 0; i < ShardCount; i++)
        {
            float size = Random.Range(9f, 20f);
            var shard = new VisualElement { pickingMode = PickingMode.Ignore };
            shard.AddToClassList("health-bar__shard");
            shard.style.width = size;
            shard.style.height = size;
            Vector2 start = new(width * (i + Random.value) / ShardCount, Random.Range(0f, height));
            shard.style.left = start.x - size / 2;
            shard.style.top = start.y - size / 2;
            // A random sharp triangle inside its square, drawn in the shield color with a pale edge
            Vector2 a = new(Random.Range(0f, 0.4f), Random.Range(0f, 0.4f)), b = new(Random.Range(0.6f, 1f), Random.Range(0f, 0.6f)), c = new(Random.Range(0.1f, 0.9f), Random.Range(0.7f, 1f));
            shard.generateVisualContent += context =>
            {
                Painter2D painter = context.painter2D;
                Rect rect = context.visualElement.contentRect;
                Vector2 Point(Vector2 p) => new(p.x * rect.width, p.y * rect.height);
                painter.BeginPath();
                painter.MoveTo(Point(a));
                painter.LineTo(Point(b));
                painter.LineTo(Point(c));
                painter.ClosePath();
                painter.fillColor = shardFill;
                painter.Fill();
                painter.strokeColor = shardEdge;
                painter.lineWidth = 1.5f;
                painter.Stroke();
            };
            Add(shard);
            Vector2 away = start - middle;
            Vector2 flight = new(away.x * 1.4f + Random.Range(-35f, 35f), Random.Range(-90f, -35f) + away.y * 2);
            float spin = Random.Range(-540f, 540f);
            shard.experimental.animation.Start(0f, 1f, ShardDuration, (element, t) =>
            {
                // Cracked in place, then thrown: fast out, falling back, fading only at the end
                float thrown = Mathf.Clamp01((t - ShardHold) / (1 - ShardHold));
                float eased = 1 - (1 - thrown) * (1 - thrown);
                element.style.translate = new Translate(flight.x * eased, flight.y * eased + 90 * thrown * thrown);
                element.style.rotate = new Rotate(spin * eased);
                element.style.opacity = 1 - Mathf.Clamp01((thrown - 0.6f) / 0.4f);
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
