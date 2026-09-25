using System;
using UnityEngine.UIElements;

/// <summary>
/// The health of the boss at the top of the screen while the player is in its room, with a mark at its phase change
/// </summary>
public class BossBar : IDisposable
{
    private readonly VisualElement panel;
    private readonly HealthBar bar;
    private readonly Label title;
    private readonly VisualElement phaseMark;
    private DraregStats boss;

    public BossBar(VisualElement panel)
    {
        this.panel = panel;
        bar = panel.Q<HealthBar>();
        title = panel.Q<Label>("BossName");
        phaseMark = panel.Q("BossPhaseMark");
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.RoomLeft += Unbind;
        GameEvents.RunEnded += OnRunEnded;
    }

    public void Dispose()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.RoomLeft -= Unbind;
        GameEvents.RunEnded -= OnRunEnded;
        Unbind();
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        DraregStats found = room.GetComponentInChildren<DraregStats>();
        if (found == null || found.IsDead) return;
        boss = found;
        boss.StatsChanged += Refresh;
        title.text = Localization.Entity(boss.ID);
        phaseMark.style.left = Length.Percent(100f * boss.PhaseThreshold / boss.MaxHealth);
        Refresh();
        panel.AddToClassList("shown");
    }

    private void OnRunEnded(bool isVictory) => Unbind();

    private void Unbind()
    {
        if (boss != null) boss.StatsChanged -= Refresh;
        boss = null;
        panel.RemoveFromClassList("shown");
    }

    private void Refresh()
    {
        if (boss == null) return;
        //The health is set in Start, after the room raised its entry
        int max = boss.MaxHealth;
        int current = boss.CurrentHealth > 0 || boss.IsDead ? boss.CurrentHealth : max;
        bar.Set(current, boss.Armor, max);
        phaseMark.EnableInClassList("hidden", boss.IsInSecondPhase);
        if (boss.IsDead) Unbind();
    }
}
