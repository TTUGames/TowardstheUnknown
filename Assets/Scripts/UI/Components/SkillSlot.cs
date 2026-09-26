using UnityEngine.UIElements;

/// <summary>
/// The diamond of an artifact's skill: its icon, its energy cost, its remaining cooldown and its queued casts
/// </summary>
[UxmlElement]
public partial class SkillSlot : SlantedPanel
{
    private readonly VisualElement icon = new() { pickingMode = PickingMode.Ignore };
    private readonly Label cooldown = new() { pickingMode = PickingMode.Ignore };
    private readonly CostTag cost = new() { pickingMode = PickingMode.Ignore };
    private readonly Label key = new() { pickingMode = PickingMode.Ignore };
    private readonly Label queued = new() { pickingMode = PickingMode.Ignore };

    public SkillSlot()
    {
        corners = Corners.All;
        AddToClassList("skill");
        AddToClassList("panel");
        icon.AddToClassList("skill__icon");
        icon.AddToClassList("stretch");
        cooldown.AddToClassList("skill__cooldown");
        cooldown.AddToClassList("stretch");
        cost.AddToClassList("skill__cost");
        key.AddToClassList("skill__key");
        queued.AddToClassList("skill__queued");
        Add(icon);
        Add(cooldown);
        Add(cost);
        Add(key);
        Add(queued);
    }

    /// <summary>
    /// The keyboard key selecting the skill, none if empty
    /// </summary>
    public string Key
    {
        get => key.text;
        set => key.text = value;
    }

    /// <summary>
    /// The number of casts of the skill waiting in the queue, hidden if none
    /// </summary>
    public int Queued
    {
        set
        {
            queued.text = value > 0 ? "\u00D7" + value : "";
            EnableInClassList("skill--queued", value > 0);
        }
    }

    public void Set(Artifact artifact, bool usable)
    {
        icon.style.backgroundImage = artifact.SkillBarIcon != null ? new StyleBackground(artifact.SkillBarIcon) : StyleKeyword.Null;
        cooldown.text = artifact.RemainingCooldown == 0 ? "" : artifact.RemainingCooldown.ToString();
        EnableInClassList("skill--cooldown", artifact.RemainingCooldown > 0);
        EnableInClassList("unusable", !usable);
        cost.value = artifact.Cost;
    }
}
