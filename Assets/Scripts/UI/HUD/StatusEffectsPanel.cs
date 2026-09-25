using System;
using UnityEngine.UIElements;

/// <summary>
/// The attack and defense buffs or debuffs of the player in the HUD, with their remaining turns
/// </summary>
public class StatusEffectsPanel : IDisposable
{
    private static readonly string[] stats = { "Attack", "Defense" };

    private readonly EntityStats entityStats;
    private readonly VisualElement root;

    public StatusEffectsPanel(VisualElement root, EntityStats entityStats)
    {
        this.root = root;
        this.entityStats = entityStats;
        entityStats.StatsChanged += Refresh;
        Refresh();
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (entityStats != null) entityStats.StatsChanged -= Refresh;
    }

    /// <summary>
    /// Each stat element gets the up or down class of its status, and shows its remaining turns
    /// </summary>
    private void Refresh()
    {
        foreach (string stat in stats)
        {
            VisualElement element = root.Q(stat);
            StatusEffect up = entityStats.HasStatusEffect(stat + "Up") ? entityStats.GetStatusEffect(stat + "Up") : null;
            StatusEffect down = up == null && entityStats.HasStatusEffect(stat + "Down") ? entityStats.GetStatusEffect(stat + "Down") : null;
            element.EnableInClassList("up", up != null);
            element.EnableInClassList("down", down != null);
            element.Q<Label>().text = (up ?? down)?.Duration.ToString() ?? "";
        }
    }
}
