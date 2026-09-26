using System;
using UnityEngine.UIElements;

/// <summary>
/// The player's health, armor and energy in the HUD. Hovering the health bar, its armor or the energy explains them.
/// The energy gauge shakes when the player selects an artifact costing more than the energy left
/// </summary>
public class StatusPanel : IDisposable
{
    private readonly PlayerStats stats;
    private readonly HealthBar health;
    private readonly EnergyGauge energy;
    private readonly HudTooltip tooltip;
    private readonly PlayerAttack attack;
    private int previewedEnergy;

    public StatusPanel(VisualElement root, HudTooltip tooltip, PlayerStats stats, PlayerAttack attack)
    {
        this.stats = stats;
        this.attack = attack;
        this.tooltip = tooltip;
        health = root.Q<HealthBar>();
        energy = root.Q<EnergyGauge>();
        tooltip.Register(health, HealthText);
        tooltip.Register(health.Shield, ArmorText);
        tooltip.Register(energy, EnergyText);
        stats.StatsChanged += RefreshHealth;
        stats.EnergyChanged += RefreshEnergy;
        stats.EnergyCostPreviewed += PreviewEnergy;
        attack.ArtifactRefused += OnArtifactRefused;
        RefreshHealth();
        RefreshEnergy();
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (stats == null) return;
        stats.StatsChanged -= RefreshHealth;
        stats.EnergyChanged -= RefreshEnergy;
        stats.EnergyCostPreviewed -= PreviewEnergy;
        if (attack != null) attack.ArtifactRefused -= OnArtifactRefused;
    }

    private void OnArtifactRefused(Artifact artifact)
    {
        if (stats.CurrentEnergy < artifact.Cost) energy.Refuse();
    }

    private void RefreshHealth()
    {
        health.Set(stats.CurrentHealth, stats.Armor, stats.MaxHealth);
        // Without armor, its empty part lets the pointer through to the health
        health.Shield.pickingMode = stats.Armor > 0 ? PickingMode.Position : PickingMode.Ignore;
        tooltip.Refresh();
    }

    private void RefreshEnergy()
    {
        previewedEnergy = 0;
        energy.Set(stats.CurrentEnergy, stats.MaxEnergy);
        tooltip.Refresh();
    }

    /// <summary>
    /// Shows the energy an action would cost
    /// </summary>
    private void PreviewEnergy(int cost)
    {
        if (previewedEnergy == cost) return;
        previewedEnergy = cost;
        energy.Set(stats.CurrentEnergy, stats.MaxEnergy, cost);
    }

    /// <summary>
    /// The health out of the maximum, and the armor taking the damage first if any
    /// </summary>
    private string HealthText() => HudTooltip.Format(Localization.UI("TooltipHealth"),
        string.Format(Localization.UI("TooltipHealthValue"), stats.CurrentHealth, stats.MaxHealth),
        stats.Armor > 0 ? string.Format(Localization.UI("TooltipHealthArmor"), stats.Armor) : null);

    /// <summary>
    /// The damage the armor absorbs, and when it is lost (EntityStats.OnTurnLaunch and OnCombatEnd); none without armor
    /// </summary>
    private string ArmorText() => stats.Armor <= 0 ? null : HudTooltip.Format(Localization.UI("TooltipArmor"),
        string.Format(Localization.UI("TooltipArmorValue"), stats.Armor), Localization.UI("TooltipArmorLost"));

    /// <summary>
    /// The energy out of the maximum, what spends it (TacticsMove in combat, Artifact.ApplyCosts) and what refills it
    /// (PlayerStats.OnTurnLaunch and OnCombatEnd)
    /// </summary>
    private string EnergyText() => HudTooltip.Format(Localization.UI("TooltipEnergy"),
        string.Format(Localization.UI("PlayerStatsEnergy"), stats.CurrentEnergy, stats.MaxEnergy),
        Localization.UI("TooltipEnergyUse") + "\n" + Localization.UI("TooltipEnergyRefill"));
}
