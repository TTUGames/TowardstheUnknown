using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The turn order in the HUD: during a combat, the entity playing stands out. Hovering an entity shows its stats, outlines
/// it and points the board at its tile (<see cref="Room.PointAt"/>): the tile, info, ring and targets react as when the
/// pointer is on it, and clicking it casts the selected artifact on it
/// </summary>
public class TimelinePanel : IDisposable
{
    private readonly VisualElement root;
    private readonly AK.Wwise.Event hoverSound;
    private readonly List<(EntityTurn turn, VisualElement item)> items = new();
    private readonly List<(EntityStats stats, Action refresh)> watchedStats = new();
    private EntityTurn hoveredTurn;

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
        //The items are removed without a pointer leave event
        if (hoveredTurn != null) Unhover(hoveredTurn);
        hoveredTurn = null;
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
        var name = new Label(Localization.Entity(stats.ID)) { pickingMode = PickingMode.Ignore };
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

        item.RegisterCallback<PointerEnterEvent>(_ => {
            if (GameScene.IsGameplayBlocked || turn == null) return;
            hoverSound.Post(turn.gameObject);
            hoveredTurn = turn;
            if (turn.TryGetComponent(out EntityOutline outline)) outline.enabled = true;
            Room.PointAt(turn.GetComponent<TacticsMove>());
        });
        item.RegisterCallback<PointerLeaveEvent>(_ => {
            if (hoveredTurn != turn) return;
            Unhover(turn);
            hoveredTurn = null;
        });
        return item;
    }

    private static void Unhover(EntityTurn turn)
    {
        if (turn == null) return;
        if (turn.TryGetComponent(out EntityOutline outline)) outline.enabled = false;
        Room.StopPointingAt(turn.GetComponent<TacticsMove>());
    }

    private static Label AddStat(VisualElement panel, string className)
    {
        var label = new Label { pickingMode = PickingMode.Ignore };
        label.AddToClassList(className);
        panel.Add(label);
        return label;
    }
}
