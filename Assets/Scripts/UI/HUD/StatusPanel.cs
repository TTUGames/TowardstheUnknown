using System;
using UnityEngine.UIElements;

/// <summary>
/// The player's health, armor and energy in the HUD
/// </summary>
public class StatusPanel : IDisposable
{
    private readonly PlayerStats stats;
    private readonly HealthBar health;
    private readonly EnergyGauge energy;
    private int previewedEnergy;

    public StatusPanel(VisualElement root, PlayerStats stats)
    {
        this.stats = stats;
        health = root.Q<HealthBar>();
        energy = root.Q<EnergyGauge>();
        stats.StatsChanged += RefreshHealth;
        stats.EnergyChanged += RefreshEnergy;
        stats.EnergyCostPreviewed += PreviewEnergy;
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
    }

    private void RefreshHealth() => health.Set(stats.CurrentHealth, stats.Armor, stats.MaxHealth);

    private void RefreshEnergy()
    {
        previewedEnergy = 0;
        energy.Set(stats.CurrentEnergy, stats.MaxEnergy);
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
}
