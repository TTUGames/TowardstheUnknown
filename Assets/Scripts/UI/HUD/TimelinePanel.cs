using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The turn order in the HUD: during a combat, the entity playing stands out. Hovering an entity shows its stats and outlines it
/// </summary>
public class TimelinePanel : IDisposable
{
    private readonly VisualElement root;
    private readonly AK.Wwise.Event hoverSound;
    private readonly List<(EntityTurn turn, VisualElement item)> items = new();
    private readonly List<(EntityStats stats, Action refresh)> watchedStats = new();

    public TimelinePanel(VisualElement root, AK.Wwise.Event hoverSound)
    {
        this.root = root;
        this.hoverSound = hoverSound;
        TurnSystem.Instance.TurnOrderChanged += Refresh;
        TurnSystem.Instance.TurnChanged += HighlightCurrentTurn;
        Refresh();
    }

    public void Dispose()
    {
        //The turn system may be destroyed first when the scene unloads
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.TurnOrderChanged -= Refresh;
            TurnSystem.Instance.TurnChanged -= HighlightCurrentTurn;
        }
        Clear();
    }

    private void Clear()
    {
        foreach ((_, VisualElement item) in items)
            item.RemoveFromHierarchy();
        items.Clear();
        foreach ((EntityStats stats, Action refresh) in watchedStats)
            if (stats != null) stats.StatsChanged -= refresh;
        watchedStats.Clear();
    }

    /// <summary>
    /// Rebuilds the items from the turn order
    /// </summary>
    public void Refresh()
    {
        Clear();
        foreach (EntityTurn turn in TurnSystem.Instance.Turns)
            items.Add((turn, CreateItem(turn)));
        HighlightCurrentTurn();
    }

    private void HighlightCurrentTurn()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        root.EnableInClassList("in-combat", turnSystem.IsCombat);
        foreach ((EntityTurn turn, VisualElement item) in items)
            item.EnableInClassList("current", turnSystem.IsCurrentTurn(turn));
    }

    private VisualElement CreateItem(EntityTurn turn)
    {
        EntityStats stats = turn.stats;
        var item = new VisualElement();
        item.AddToClassList("timeline-item");
        item.style.backgroundImage = new StyleBackground(stats.TimelineIcon);
        var marker = new VisualElement { pickingMode = PickingMode.Ignore };
        marker.AddToClassList("timeline-item__marker");
        item.Add(marker);

        var panel = new SlantedPanel(Corners.TopLeft | Corners.BottomRight, "timeline-item__stats", "panel", "fade-in") { pickingMode = PickingMode.Ignore };
        var name = new Label(Localization.Entity(stats.ID));
        name.AddToClassList("timeline-item__name");
        panel.Add(name);
        Label health = AddStat(panel, "stat--health"), attack = AddStat(panel, "stat--attack"), defense = AddStat(panel, "stat--defense");
        item.Add(panel);
        root.Add(item);

        Action refresh = () => {
            health.text = string.Format(Localization.UI("PlayerStatsHP"), stats.CurrentHealth, stats.Armor, stats.MaxHealth);
            attack.text = string.Format(Localization.UI("PlayerStatsAttack"), Mathf.RoundToInt((stats.DamageDealtMultiplier - 1) * 100));
            defense.text = string.Format(Localization.UI("PlayerStatsDefense"), Mathf.RoundToInt((1 - stats.DamageReceivedMultiplier) * 100));
        };
        stats.StatsChanged += refresh;
        watchedStats.Add((stats, refresh));
        refresh();

        EntityOutline outline = turn.GetComponent<EntityOutline>();
        item.RegisterCallback<PointerEnterEvent>(_ => {
            if (GameScene.IsGameplayBlocked) return;
            hoverSound.Post(turn.gameObject);
            if (outline != null) outline.enabled = true;
        });
        item.RegisterCallback<PointerLeaveEvent>(_ => {
            if (outline != null) outline.enabled = false;
        });
        return item;
    }

    private static Label AddStat(VisualElement panel, string className)
    {
        var label = new Label();
        label.AddToClassList(className);
        panel.Add(label);
        return label;
    }
}
