using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Shows what happens to each entity over it: the health lost (heavy hits bigger), the damage its armor took, heals, armor gained,
/// status effects applied and the score of a kill. A popup pops, stays, then rises and fades out (transitions of Hud.uss);
/// the popups of an entity shown together stack upwards
/// </summary>
public class CombatPopups : IDisposable
{
    private const long PopDelay = 170;
    private const long FadeDelay = 1000;
    private const long RemoveDelay = 1200;
    // Above the entity, in panel points
    private const float OffsetUp = 100;
    private const float StackStep = 38;
    // Popups of an entity closer in time than this stack
    private const float StackWindow = 0.5f;
    private const int HeavyHit = 40;

    private readonly VisualElement root;
    private readonly Dictionary<Transform, (float time, int count)> stacks = new();

    public CombatPopups(VisualElement root)
    {
        this.root = root;
        GameEvents.DamageTaken += OnDamageTaken;
        GameEvents.Healed += OnHealed;
        GameEvents.ArmorGained += OnArmorGained;
        GameEvents.StatusApplied += OnStatusApplied;
        GameEvents.EntityDied += OnEntityDied;
    }

    public void Dispose()
    {
        GameEvents.DamageTaken -= OnDamageTaken;
        GameEvents.Healed -= OnHealed;
        GameEvents.ArmorGained -= OnArmorGained;
        GameEvents.StatusApplied -= OnStatusApplied;
        GameEvents.EntityDied -= OnEntityDied;
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        int blocked = damage - healthLost;
        if (healthLost > 0)
            Spawn(entity, healthLost.ToString(), entity.type == EntityType.PLAYER ? "popup--player-hurt" : "popup--hurt", healthLost >= HeavyHit ? "popup--heavy" : null);
        if (blocked > 0)
            Spawn(entity, "-" + blocked, "popup--blocked");
    }

    private void OnHealed(EntityStats entity, int healed)
    {
        if (healed > 0) Spawn(entity, "+" + healed, "popup--heal");
    }

    private void OnArmorGained(EntityStats entity, int armor)
    {
        if (armor > 0) Spawn(entity, "+" + armor, "popup--armor");
    }

    private void OnStatusApplied(EntityStats entity, StatusEffectData status)
    {
        string stat = status.stat == StatusEffectData.Stat.DamageDealt ? "popup--attack" : "popup--defense";
        Spawn(entity, Localization.UI("Status" + status.name), "popup--status", stat, status.isBuff ? "up" : "down");
    }

    private void OnEntityDied(EntityStats entity)
    {
        if (entity.type != EntityType.PLAYER && entity.Data.score > 0)
            Spawn(entity, "+" + entity.Data.score, "popup--score");
    }

    private void Spawn(EntityStats entity, string text, params string[] classes)
    {
        if (root.panel == null || Camera.main == null) return;
        Vector2 position = root.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, entity.transform.position, Camera.main));
        int stacked = Stack(entity.transform);

        var popup = new Label(text) { pickingMode = PickingMode.Ignore };
        popup.AddToClassList("popup");
        foreach (string className in classes)
            if (className != null) popup.AddToClassList(className);
        popup.style.left = position.x;
        popup.style.top = position.y - OffsetUp - stacked * StackStep;
        root.Add(popup);
        popup.schedule.Execute(() => popup.AddToClassList("popup--shown")).StartingIn(PopDelay);
        popup.schedule.Execute(() => popup.AddToClassList("popup--fading")).StartingIn(FadeDelay);
        popup.schedule.Execute(popup.RemoveFromHierarchy).StartingIn(RemoveDelay);
    }

    /// <summary>
    /// The number of popups the entity shows already, counting this one
    /// </summary>
    private int Stack(Transform entity)
    {
        float now = Time.unscaledTime;
        int count = stacks.TryGetValue(entity, out var stack) && now - stack.time < StackWindow ? stack.count : 0;
        stacks[entity] = (now, count + 1);
        //Forgets the entities no longer shown
        if (stacks.Count > 32)
        {
            var old = new List<Transform>();
            foreach (var (key, value) in stacks)
                if (key == null || now - value.time >= StackWindow) old.Add(key);
            foreach (Transform key in old) stacks.Remove(key);
        }
        return count;
    }
}
