using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Shows what happens to each entity over it: the health lost (bigger with the damage, the lethal hit in the accent color),
/// the damage its armor took, heals, armor gained, status effects applied and the score of a kill. A popup pops, stays,
/// then rises and fades out (transitions of Hud.uss); the popups of an entity shown together stack upwards, and the hits
/// following each other on an entity add up in its health popup, which pops again. The labels of the popups gone are reused
/// </summary>
public class CombatPopups : IDisposable
{
    private const long PopDelay = 170;
    private const long FadeDelay = 1000;
    private const long RemoveDelay = 1200;
    // The original's plain numbers (DamageIndicator.anim): up to 1.2 at 283 ms, back to 1 at 400 ms, gone at 1167 ms
    private const float PlainPeak = 1.2f;
    private const long PlainSettleDelay = 287;
    private const long PlainRemoveDelay = 1167;
    // Above the entity, in panel points; the original's plain numbers sat lower
    private const float OffsetUp = 100;
    private const float PlainOffsetUp = 30;
    private const float StackStep = 38;
    // Popups of an entity closer in time than this stack
    private const float StackWindow = 0.5f;
    // Hits on an entity closer in time than this add up in one popup
    private const float SumWindow = 0.6f;
    // Health lost for the largest popup, and the popup's scale from the least health to it
    private const int HeavyHit = 40;
    private const float LightScale = 0.8f;
    private const float HeavyScale = 1.45f;
    private const float LethalScale = 1.15f;
    // The punch of a popup adding a hit, over its scale
    private const float SumPunch = 1.3f;
    private const long SumPunchDelay = 90;

    private sealed class Popup
    {
        public Label label;
        public float scale;
        public bool removed;
        public IVisualElementScheduledItem fade, remove;
    }

    private readonly VisualElement root;
    private readonly Dictionary<Transform, (float time, int count)> stacks = new();
    // The health popup of each entity, adding up the hits
    private readonly Dictionary<Transform, (Popup popup, int total, float time)> sums = new();
    // The labels of the popups gone, shown again by the next ones. Their timings are scheduled on the root: a label's own
    // items, paused as it leaves the panel, would resume when it is shown again
    private readonly Stack<Label> pool = new();

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

    // Raised before the health goes down: the current health tells a lethal hit
    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (!Edition.Profile.detailedPopups)
        {
            Spawn(entity, damage.ToString(), 1, "popup--hurt");
            return;
        }
        int blocked = damage - healthLost;
        if (healthLost > 0) ShowHealthLost(entity, healthLost, entity.CanDie && healthLost >= entity.CurrentHealth);
        if (blocked > 0)
            Spawn(entity, "-" + blocked, 1, "popup--blocked");
    }

    private void ShowHealthLost(EntityStats entity, int healthLost, bool lethal)
    {
        float now = Time.unscaledTime;
        Transform key = entity.transform;
        int total = healthLost;
        if (sums.TryGetValue(key, out var sum) && now - sum.time < SumWindow && !sum.popup.removed)
        {
            total += sum.total;
            Popup popup = sum.popup;
            popup.label.text = total.ToString();
            popup.scale = HealthScale(total, lethal);
            if (lethal) popup.label.AddToClassList("popup--lethal");
            popup.label.style.scale = new Scale(Vector2.one * popup.scale * SumPunch);
            root.schedule.Execute(() => popup.label.style.scale = new Scale(Vector2.one * popup.scale)).StartingIn(SumPunchDelay);
            popup.fade.ExecuteLater(FadeDelay);
            popup.remove.ExecuteLater(RemoveDelay);
            sums[key] = (popup, total, now);
            return;
        }
        Popup shown = Spawn(entity, total.ToString(), HealthScale(total, lethal),
            entity.type == EntityType.PLAYER ? "popup--player-hurt" : "popup--hurt", lethal ? "popup--lethal" : null);
        if (shown != null) sums[key] = (shown, total, now);
    }

    private static float HealthScale(int healthLost, bool lethal) =>
        Mathf.Lerp(LightScale, HeavyScale, Mathf.InverseLerp(1, HeavyHit, healthLost)) * (lethal ? LethalScale : 1);

    private void OnHealed(EntityStats entity, int healed)
    {
        if (healed > 0 && Edition.Profile.detailedPopups) Spawn(entity, "+" + healed, 1, "popup--heal");
    }

    private void OnArmorGained(EntityStats entity, int armor)
    {
        if (armor > 0 && Edition.Profile.detailedPopups) Spawn(entity, "+" + armor, 1, "popup--armor");
    }

    private void OnStatusApplied(EntityStats entity, StatusEffectData status)
    {
        if (!Edition.Profile.detailedPopups) return;
        string stat = status.stat == StatusEffectData.Stat.DamageDealt ? "popup--attack" : "popup--defense";
        Spawn(entity, Localization.UI("Status" + status.name), 1, "popup--status", stat, status.isBuff ? "up" : "down");
    }

    private void OnEntityDied(EntityStats entity)
    {
        if (entity.type != EntityType.PLAYER && entity.Data.score > 0 && Edition.Profile.detailedPopups)
            Spawn(entity, "+" + entity.Data.score, 1, "popup--score");
    }

    private Popup Spawn(EntityStats entity, string text, float scale, params string[] classes)
    {
        if (!WorldLabels.CanPlace(root)) return null;
        // The original's plain numbers don't stack: each one shows where the entity is
        bool detailed = Edition.Profile.detailedPopups;
        int stacked = detailed ? Stack(entity.transform) : 0;

        Label label = pool.Count > 0 ? pool.Pop() : new Label { pickingMode = PickingMode.Ignore };
        label.text = text;
        label.ClearClassList();
        label.style.scale = StyleKeyword.Null;
        label.AddToClassList("popup");
        foreach (string className in classes)
            if (className != null) label.AddToClassList(className);
        root.Add(label);
        WorldLabels.Place(root, label, entity.transform.position, (detailed ? OffsetUp : PlainOffsetUp) + stacked * StackStep);
        var popup = new Popup { label = label, scale = scale };
        // Its scale pops from 0 (Hud.uss) to its own, which grows with the damage; a plain number overshoots then settles
        root.schedule.Execute(() =>
        {
            label.AddToClassList("popup--shown");
            label.style.scale = new Scale(Vector2.one * (detailed ? popup.scale : PlainPeak));
        }).StartingIn(PopDelay);
        if (!detailed) root.schedule.Execute(() => label.style.scale = new Scale(Vector2.one * popup.scale)).StartingIn(PlainSettleDelay);
        popup.fade = root.schedule.Execute(() => label.AddToClassList("popup--fading")).StartingIn(FadeDelay);
        popup.remove = root.schedule.Execute(() =>
        {
            popup.removed = true;
            label.RemoveFromHierarchy();
            pool.Push(label);
        }).StartingIn(detailed ? RemoveDelay : PlainRemoveDelay);
        return popup;
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
            var gone = new List<Transform>();
            foreach (var (key, value) in sums)
                if (key == null || value.popup.removed) gone.Add(key);
            foreach (Transform key in gone) sums.Remove(key);
        }
        return count;
    }
}
