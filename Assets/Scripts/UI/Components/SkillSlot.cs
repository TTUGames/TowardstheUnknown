using UnityEngine.UIElements;

/// <summary>
/// The diamond of an artifact's skill: its icon, its energy cost and its remaining cooldown
/// </summary>
[UxmlElement]
public partial class SkillSlot : SlantedPanel
{
    private readonly VisualElement icon = new() { pickingMode = PickingMode.Ignore };
    private readonly Label cooldown = new() { pickingMode = PickingMode.Ignore };
    private readonly CostTag cost = new() { pickingMode = PickingMode.Ignore };

    public SkillSlot()
    {
        corners = Corners.All;
        AddToClassList("skill");
        AddToClassList("panel");
        icon.AddToClassList("skill__icon");
        cooldown.AddToClassList("skill__cooldown");
        cost.AddToClassList("skill__cost");
        Add(icon);
        Add(cooldown);
        Add(cost);
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
