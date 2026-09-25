using System;
using UnityEngine.UIElements;

/// <summary>
/// The player's health, armor and energy in the HUD
/// </summary>
public class StatusPanel : IDisposable
{
    private readonly PlayerStats stats;
    private readonly VisualElement healthFill;
    private readonly VisualElement shieldFill;
    private readonly Label healthText;
    private readonly VisualElement[] energyCells;
    private int previewedEnergy;

    public StatusPanel(VisualElement root, PlayerStats stats, FilterFunctionDefinition slantedBlur)
    {
        this.stats = stats;
        healthFill = root.Q("HealthFill");
        shieldFill = root.Q("ShieldFill");
        healthText = root.Q<Label>("HealthText");

        VisualElement energy = root.Q("Energy");
        energyCells = new VisualElement[stats.MaxEnergy];
        for (int i = 0; i < energyCells.Length; i++)
        {
            energyCells[i] = new VisualElement();
            energyCells[i].AddToClassList("energy-cell");
            energyCells[i].AddToClassList(CutShape.ClassName);
            energy.Add(energyCells[i]);
        }
        CutShape.AttachAll(energy, slantedBlur);

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

    private void RefreshHealth()
    {
        healthFill.style.width = Length.Percent(100f * stats.CurrentHealth / stats.MaxHealth);
        shieldFill.style.width = Length.Percent(100f * Math.Min(stats.Armor, stats.MaxHealth) / stats.MaxHealth);
        healthText.text = stats.CurrentHealth + " (" + stats.Armor + ") / " + stats.MaxHealth;
    }

    private void RefreshEnergy()
    {
        previewedEnergy = 0;
        UpdateEnergyCells();
    }

    /// <summary>
    /// Shows the energy an action would cost
    /// </summary>
    private void PreviewEnergy(int cost)
    {
        if (previewedEnergy == cost) return;
        previewedEnergy = cost;
        UpdateEnergyCells();
    }

    private void UpdateEnergyCells()
    {
        int current = stats.CurrentEnergy;
        for (int i = 0; i < energyCells.Length; i++)
        {
            energyCells[i].EnableInClassList("energy-cell--empty", i >= current);
            energyCells[i].EnableInClassList("energy-cell--previewed", i < current && i >= current - previewedEnergy);
        }
    }
}
