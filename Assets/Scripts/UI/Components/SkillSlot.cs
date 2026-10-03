using UnityEngine.UIElements;

/// <summary>
/// The diamond of an artifact's skill: its icon, its energy cost, its remaining cooldown, its queued casts and, for a skill cast
/// more than once a turn, a pip per cast, lit while it is left
/// </summary>
[UxmlElement]
public partial class SkillSlot : SlantedPanel
{
    private readonly VisualElement icon = new() { pickingMode = PickingMode.Ignore };
    private readonly Label cooldown = new() { pickingMode = PickingMode.Ignore };
    private readonly CostTag cost = new() { pickingMode = PickingMode.Ignore };
    private readonly Label queued = new() { pickingMode = PickingMode.Ignore };
    private readonly VisualElement uses = new() { pickingMode = PickingMode.Ignore };

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
        queued.AddToClassList("skill__queued");
        Add(icon);
        Add(cooldown);
        Add(cost);
        Add(queued);
        uses.AddToClassList("skill__uses");
        Add(uses);
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
        icon.style.backgroundImage = artifact.Data.skillBarIcon != null ? new StyleBackground(artifact.Data.skillBarIcon) : StyleKeyword.Null;
        cooldown.text = artifact.RemainingCooldown == 0 ? "" : artifact.RemainingCooldown.ToString();
        EnableInClassList("skill--cooldown", artifact.RemainingCooldown > 0);
        EnableInClassList("unusable", !usable);
        cost.value = artifact.Data.cost;
        ShowUses(artifact.Data.maximumUsePerTurn > 1 ? artifact.Data.maximumUsePerTurn : 0, artifact.RemainingUsesThisTurn);
    }

    private void ShowUses(int maximum, int left)
    {
        while (uses.childCount < maximum)
        {
            var pip = new VisualElement { pickingMode = PickingMode.Ignore };
            pip.AddToClassList("skill__use");
            uses.Add(pip);
        }
        for (int i = 0; i < uses.childCount; i++)
        {
            VisualElement pip = uses[i];
            pip.style.display = i < maximum ? DisplayStyle.Flex : DisplayStyle.None;
            pip.EnableInClassList("skill__use--spent", i >= left);
        }
    }
}
