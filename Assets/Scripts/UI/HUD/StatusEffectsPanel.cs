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
    private readonly Label tooltip;
    private IVisualElementScheduledItem showTooltip;
    // The stat whose tooltip is shown or about to be, refreshed with the status
    private StatusEffectData.Stat? hoveredStat;

    public StatusEffectsPanel(VisualElement root, Label tooltip, EntityStats entityStats)
    {
        this.root = root;
        this.tooltip = tooltip;
        this.entityStats = entityStats;
        // The tooltip hides when a menu opens over it
        tooltip.schedule.Execute(() => {
            if (GameScene.IsGameplayBlocked) HideTooltip();
        }).Every(100);
        foreach ((StatusEffectData.Stat stat, string elementName) in stats)
        {
            VisualElement element = root.Q(elementName);
            element.RegisterCallback<PointerEnterEvent>(_ => {
                showTooltip?.Pause();
                hoveredStat = stat;
                showTooltip = tooltip.schedule.Execute(ShowTooltip).StartingIn(SkillsBar.TooltipDelay);
            });
            element.RegisterCallback<PointerLeaveEvent>(_ => HideTooltip());
        }
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
        if (tooltip.ClassListContains("shown")) ShowTooltip();
    }

    /// <summary>
    /// The status's name, its change of the damage in percent and its remaining turns; hidden if the stat has no status
    /// </summary>
    private void ShowTooltip()
    {
        StatusEffect status = hoveredStat.HasValue ? GetStatus(hoveredStat.Value) : null;
        if (status == null || GameScene.IsGameplayBlocked)
        {
            HideTooltip();
            return;
        }
        StatusEffectData data = status.Data;
        string key = (data.stat == StatusEffectData.Stat.DamageDealt ? "StatusDamageDealt" : "StatusDamageReceived") + (data.delta >= 0 ? "More" : "Less");
        int percent = Mathf.RoundToInt(Mathf.Abs(data.delta) * 100);
        tooltip.text = "<b>" + Localization.UI("Status" + data.name) + "</b>\n" + string.Format(Localization.UI(key), percent)
            + "\n<size=85%>" + string.Format(Localization.UI("StatusTurnsLeft"), status.Duration) + "</size>";
        tooltip.EnableInClassList("buff", data.isBuff);
        tooltip.EnableInClassList("debuff", !data.isBuff);
        tooltip.AddToClassList("shown");
    }

    private void HideTooltip()
    {
        showTooltip?.Pause();
        hoveredStat = null;
        tooltip.RemoveFromClassList("shown");
    }
}
