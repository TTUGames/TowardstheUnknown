using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The attack and defense buffs or debuffs of the player in the HUD, with their remaining turns. Hovering one shows what it does
/// </summary>
public class StatusEffectsPanel : IDisposable
{
    // The element showing the status effects on each stat
    private static readonly (StatusEffectData.Stat stat, string element)[] stats = {
        (StatusEffectData.Stat.DamageDealt, "Attack"), (StatusEffectData.Stat.DamageReceived, "Defense") };

    private readonly EntityStats entityStats;
    private readonly VisualElement root;
    private readonly HudTooltip tooltip;

    public StatusEffectsPanel(VisualElement root, HudTooltip tooltip, EntityStats entityStats)
    {
        this.root = root;
        this.tooltip = tooltip;
        this.entityStats = entityStats;
        // The tooltip keeps its place, right of the status effects
        foreach ((StatusEffectData.Stat stat, string elementName) in stats)
            tooltip.Register(root.Q(elementName), () => TooltipText(stat), HudTooltip.Placement.Styled);
        entityStats.StatsChanged += Refresh;
        Refresh();
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (entityStats != null) entityStats.StatsChanged -= Refresh;
    }

    private StatusEffect GetStatus(StatusEffectData.Stat stat) =>
        entityStats.StatusEffects.FirstOrDefault(effect => effect.Data.stat == stat);

    /// <summary>
    /// Each stat element gets the up or down class of its status, and shows its remaining turns
    /// </summary>
    private void Refresh()
    {
        foreach ((StatusEffectData.Stat stat, string elementName) in stats)
        {
            VisualElement element = root.Q(elementName);
            StatusEffect status = GetStatus(stat);
            element.EnableInClassList("up", status != null && status.Data.isBuff);
            element.EnableInClassList("down", status != null && !status.Data.isBuff);
            element.Q<Label>().text = status?.Duration.ToString() ?? "";
            // A hidden status lets the pointer through to the tiles
            element.pickingMode = status != null ? PickingMode.Position : PickingMode.Ignore;
        }
        tooltip.Refresh();
    }

    /// <summary>
    /// The status's name, its change of the damage in percent and its remaining turns, its line in the buff or debuff
    /// color; none if the stat has no status
    /// </summary>
    private string TooltipText(StatusEffectData.Stat stat)
    {
        StatusEffect status = GetStatus(stat);
        if (status == null) return null;
        StatusEffectData data = status.Data;
        string key = (data.stat == StatusEffectData.Stat.DamageDealt ? "StatusDamageDealt" : "StatusDamageReceived") + (data.delta >= 0 ? "More" : "Less");
        int percent = Mathf.RoundToInt(Mathf.Abs(data.delta) * 100);
        tooltip.EnableInClassList("buff", data.isBuff);
        tooltip.EnableInClassList("debuff", !data.isBuff);
        return HudTooltip.Format(Localization.UI("Status" + data.name), string.Format(Localization.UI(key), percent),
            string.Format(Localization.UI("StatusTurnsLeft"), status.Duration));
    }
}
