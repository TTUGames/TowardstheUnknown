using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Shows what happens to each entity over it: the health lost (bigger with the damage, the lethal hit in the accent color),
/// the damage its armor took, heals, armor gained, status effects applied (or cancelled by their opposite) and the score of a kill. A popup pops, stays,
/// then rises and fades out (transitions of Hud.uss); the popups of an entity stack upwards while they are shown, and the hits
/// following each other on an entity add up in its health popup, which pops again. A popup follows its entity while it moves
/// (pushed, dashing), and stays where the entity was once it is gone. The labels of the popups gone are reused
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
        // What it follows, its last place and its height above it in panel points
        public Transform entity;
        public Vector3 world;
        public float offsetUp;
        // Its height in the entity's stack
        public int slot;
        public bool removed;
        public IVisualElementScheduledItem fade, remove;
    }

    private readonly VisualElement root;
    // The popups each entity shows, holding their heights in its stack
    private readonly Dictionary<Transform, List<Popup>> stacks = new();
    // The health popup of each entity, adding up the hits
    private readonly Dictionary<Transform, (Popup popup, int total, float time)> sums = new();
    // The labels of the popups gone, shown again by the next ones. Their timings are scheduled on the root: a label's own
    // items, paused as it leaves the panel, would resume when it is shown again
    private readonly Stack<Label> pool = new();
    // The popups shown, placed again each frame over their entity
    private readonly List<Popup> active = new();
    private readonly IVisualElementScheduledItem follow;

    public CombatPopups(VisualElement root)
    {
        this.root = root;
        GameEvents.DamageTaken += OnDamageTaken;
        GameEvents.Healed += OnHealed;
        GameEvents.ArmorGained += OnArmorGained;
        GameEvents.StatusApplied += OnStatusApplied;
        GameEvents.EntityDied += OnEntityDied;
        follow = root.schedule.Execute(Follow).Every(0);
        follow.Pause();
    }

    public void Dispose()
    {
        GameEvents.DamageTaken -= OnDamageTaken;
        GameEvents.Healed -= OnHealed;
        GameEvents.ArmorGained -= OnArmorGained;
        GameEvents.StatusApplied -= OnStatusApplied;
        GameEvents.EntityDied -= OnEntityDied;
        follow.Pause();
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
        // A damage rolled high in its range shines, unless it kills (the lethal color says more)
        bool highRoll = !lethal && entity.HitRoll >= Edition.Profile.highRollPopup;
        Popup shown = Spawn(entity, total.ToString(), HealthScale(total, lethal),
            entity.type == EntityType.PLAYER ? "popup--player-hurt" : "popup--hurt", lethal ? "popup--lethal" : highRoll ? "popup--high-roll" : null);
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
        // A status cancelling the opposite one isn't on the entity afterwards: both are gone
        if (!Has(entity, status)) Spawn(entity, Localization.UI("StatusCancelled"), 1, "popup--status", stat, "popup--cancelled");
        else Spawn(entity, Localization.UI("Status" + status.name), 1, "popup--status", stat, status.isBuff ? "up" : "down");
    }

    private static bool Has(EntityStats entity, StatusEffectData status)
    {
        foreach (StatusEffect effect in entity.StatusEffects)
            if (effect.Data == status) return true;
        return false;
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

        Label label = pool.Count > 0 ? pool.Pop() : new Label { pickingMode = PickingMode.Ignore };
        label.text = text;
        label.ClearClassList();
        label.style.scale = StyleKeyword.Null;
        label.AddToClassList("popup");
        foreach (string className in classes)
            if (className != null) label.AddToClassList(className);
        root.Add(label);
        var popup = new Popup { label = label, scale = scale, entity = entity.transform, world = entity.transform.position };
        if (detailed) Stack(entity.transform, popup);
        popup.offsetUp = (detailed ? OffsetUp : PlainOffsetUp) + popup.slot * StackStep;
        WorldLabels.Place(root, label, popup.world, popup.offsetUp);
        active.Add(popup);
        follow.Resume();
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
            active.Remove(popup);
            if (active.Count == 0) follow.Pause();
            label.RemoveFromHierarchy();
            pool.Push(label);
        }).StartingIn(detailed ? RemoveDelay : PlainRemoveDelay);
        return popup;
    }

    /// <summary>
    /// Places the popups shown over their entity, where it is now
    /// </summary>
    private void Follow()
    {
        if (!WorldLabels.CanPlace(root)) return;
        foreach (Popup popup in active)
        {
            if (popup.entity != null) popup.world = popup.entity.position;
            WorldLabels.Place(root, popup.label, popup.world, popup.offsetUp);
        }
    }

    /// <summary>
    /// Puts the popup at the lowest height of the entity's stack that no popup still shown holds
    /// </summary>
    private void Stack(Transform entity, Popup popup)
    {
        if (!stacks.TryGetValue(entity, out var shown)) stacks[entity] = shown = new List<Popup>();
        shown.RemoveAll(other => other.removed);
        while (shown.Exists(other => other.slot == popup.slot)) popup.slot++;
        shown.Add(popup);
        //Forgets the entities no longer shown
        if (stacks.Count > 32)
        {
            var gone = new List<Transform>();
            foreach (var (key, value) in stacks)
                if (key == null || value.TrueForAll(other => other.removed)) gone.Add(key);
            foreach (Transform key in gone) stacks.Remove(key);
            gone.Clear();
            foreach (var (key, value) in sums)
                if (key == null || value.popup.removed) gone.Add(key);
            foreach (Transform key in gone) sums.Remove(key);
        }
    }
}
