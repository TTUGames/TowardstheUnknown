using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The turn order in the HUD. Hovering an entity shows its stats and outlines it
/// </summary>
public class TimelinePanel : IDisposable
{
    private readonly VisualElement root;
    private readonly FilterFunctionDefinition slantedBlur;
    private readonly List<VisualElement> items = new();
    private readonly List<(EntityStats stats, Action refresh)> watchedStats = new();

    public TimelinePanel(VisualElement root, FilterFunctionDefinition slantedBlur)
    {
        this.root = root;
        this.slantedBlur = slantedBlur;
        TurnSystem.Instance.TurnOrderChanged += Refresh;
        Refresh();
    }

    public void Dispose()
    {
        //The turn system may be destroyed first when the scene unloads
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnOrderChanged -= Refresh;
        Clear();
    }

    private void Clear()
    {
        foreach (VisualElement item in items)
            item.RemoveFromHierarchy();
        items.Clear();
        foreach ((EntityStats stats, Action refresh) in watchedStats)
            if (stats != null) stats.StatsChanged -= refresh;
        watchedStats.Clear();
    }

    private void Refresh()
    {
        Clear();
        foreach (EntityTurn turn in TurnSystem.Instance.Turns)
            items.Add(CreateItem(turn));
    }

    private VisualElement CreateItem(EntityTurn turn)
    {
        EntityStats stats = turn.stats;
        var item = new VisualElement();
        item.AddToClassList("timeline-item");
        item.style.backgroundImage = new StyleBackground(stats.TimelineIcon);

        var panel = new VisualElement();
        panel.AddToClassList("timeline-item__stats");
        panel.AddToClassList(CutShape.ClassName);
        panel.pickingMode = PickingMode.Ignore;
        var name = new Label(Localization.Entity(turn.gameObject.name.Replace("(Clone)", "")));
        name.AddToClassList("timeline-item__name");
        var values = new Label();
        panel.Add(name);
        panel.Add(values);
        item.Add(panel);
        root.Add(item);
        CutShape.AttachAll(panel, slantedBlur);

        Action refresh = () => values.text =
            string.Format(Localization.UI("PlayerStatsHP"), stats.CurrentHealth, stats.Armor, stats.MaxHealth) + "\n" +
            string.Format(Localization.UI("PlayerStatsAttack"), Mathf.RoundToInt((stats.DamageDealtMultiplier - 1) * 100)) + "\n" +
            string.Format(Localization.UI("PlayerStatsDefense"), Mathf.RoundToInt((1 - stats.DamageReceivedMultiplier) * 100));
        stats.StatsChanged += refresh;
        watchedStats.Add((stats, refresh));
        refresh();

        Outline outline = turn.GetComponent<Outline>();
        item.RegisterCallback<PointerEnterEvent>(_ => {
            if (GameScene.UI.IsMenuOpen) return;
            AkUnitySoundEngine.PostEvent("HoverTimeline", turn.gameObject);
            if (outline != null) outline.enabled = true;
        });
        item.RegisterCallback<PointerLeaveEvent>(_ => {
            if (outline != null) outline.enabled = false;
        });
        return item;
    }
}
