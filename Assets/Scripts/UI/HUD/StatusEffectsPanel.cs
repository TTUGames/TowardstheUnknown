using System;
using System.Linq;
using UnityEngine.UIElements;

/// <summary>
/// The attack and defense buffs or debuffs of the player in the HUD, with their remaining turns
/// </summary>
public class StatusEffectsPanel : IDisposable
{
    // The element showing the status effects on each stat
    private static readonly (StatusEffectData.Stat stat, string element)[] stats = {
        (StatusEffectData.Stat.DamageDealt, "Attack"), (StatusEffectData.Stat.DamageReceived, "Defense") };

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
        foreach ((StatusEffectData.Stat stat, string elementName) in stats)
        {
            VisualElement element = root.Q(elementName);
            StatusEffect status = entityStats.StatusEffects.FirstOrDefault(effect => effect.Data.stat == stat);
            element.EnableInClassList("up", status != null && status.Data.isBuff);
            element.EnableInClassList("down", status != null && !status.Data.isBuff);
            element.Q<Label>().text = status?.Duration.ToString() ?? "";
        }
    }
}
