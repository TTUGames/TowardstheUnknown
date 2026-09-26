using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// The turn order in the HUD: during a combat, the entity playing stands out. Hovering an entity shows its tooltip (name,
/// health, armor, movement points and status effects), outlines it and points the board at its tile
/// (<see cref="Room.PointAt"/>): the tile, ring, threat and targets react as when the pointer is on it, the board's info
/// panel staying hidden since the tooltip shows the same, and clicking it casts the selected artifact on it
/// </summary>
public class TimelinePanel : IDisposable
{
    // Between the stats on the tooltip's line, as on the board's info panel
    private const string Separator = "  |  ";

    private readonly VisualElement root;
    private readonly AK.Wwise.Event hoverSound;
    private readonly HudTooltip tooltip;
    private readonly List<(EntityTurn turn, VisualElement item)> items = new();
    private readonly List<EntityStats> watchedStats = new();
    private EntityTurn hoveredTurn;

    public TimelinePanel(VisualElement root, HudTooltip tooltip, AK.Wwise.Event hoverSound)
    {
        this.root = root;
        this.tooltip = tooltip;
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
        //The items are removed without a pointer leave event, which hides their tooltip too
        if (hoveredTurn != null) Unhover(hoveredTurn);
        hoveredTurn = null;
        foreach ((_, VisualElement item) in items)
            item.RemoveFromHierarchy();
        items.Clear();
        foreach (EntityStats stats in watchedStats)
        {
            if (stats == null) continue;
            stats.StatsChanged -= tooltip.Refresh;
            if (stats is PlayerStats player) player.EnergyChanged -= tooltip.Refresh;
        }
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
        root.Add(item);

        tooltip.Register(item, () => TooltipText(stats));
        // The shown tooltip follows the stats, and the player's energy
        stats.StatsChanged += tooltip.Refresh;
        if (stats is PlayerStats player) player.EnergyChanged += tooltip.Refresh;
        watchedStats.Add(stats);

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

    /// <summary>
    /// The entity's name, health, armor if any, movement points (the player's energy) and status effects with their turns
    /// </summary>
    private static string TooltipText(EntityStats stats)
    {
        if (stats == null || stats.IsDead) return null;
        string text = string.Format(Localization.UI("TooltipHealthValue"), stats.CurrentHealth, stats.MaxHealth);
        if (stats.Armor > 0) text += Separator + string.Format(Localization.UI("TooltipEntityArmor"), stats.Armor);
        // The player moves with its energy, an enemy with its movement points, all of them on its turn
        text += Separator + (stats is PlayerStats player
            ? string.Format(Localization.UI("TooltipEntityEnergy"), player.CurrentEnergy, player.MaxEnergy)
            : string.Format(Localization.UI("TooltipEntityMovement"), stats is EnemyStats enemy ? enemy.maxMovementPoints : stats.GetMovementDistance()));
        return HudTooltip.Format(Localization.Entity(stats.ID), text, HudTooltip.StatusLine(stats));
    }
}
