using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Over each entity the player's selected artifact would hit: the health it would lose, and whether the hit kills it for sure or may kill it,
/// those previews beating; over the player, the health its own damage would cost it (ExplosiveSacrifice, HitBuff), likewise, and
/// under its feet the armor and health it would gain (Barrier, ProtectiveEnvelope, Vampirism; <see cref="EditionProfile.gainPreview"/>)
/// </summary>
public class DamagePreview : IDisposable
{
    // Above the entity, in panel points; the gains under its feet, clear of the info panels above
    private const float OffsetUp = 150;
    private const float GainOffsetUp = -40;
    private const long BeatInterval = 400;

    private readonly VisualElement root;
    private readonly PlayerAttack attack;
    private readonly WorldLabels labels;
    private readonly IVisualElementScheduledItem beat;

    public DamagePreview(VisualElement root, PlayerAttack attack)
    {
        this.root = root;
        this.attack = attack;
        labels = new WorldLabels(root, "damage-preview");
        attack.TargetsPreviewed += Show;
        // The lethal previews beat together (transition of Hud.uss), only while some are shown; not with the reduced animations
        beat = root.schedule.Execute(() => {
            bool still = GameSettings.ReducedMotion;
            for (int i = 0; i < labels.Count; i++)
                if (still) labels[i].RemoveFromClassList("damage-preview--beat");
                else labels[i].ToggleInClassList("damage-preview--beat");
        }).Every(BeatInterval);
        beat.Pause();
    }

    public void Dispose()
    {
        if (attack != null) attack.TargetsPreviewed -= Show;
    }

    /// <summary>
    /// Writes in the label, over the entity, the health it would lose and whether that kills it for sure or may: one that
    /// cannot die (Drareg in his first phase, the training dummy) is never killed. The player's own loss reads with a minus
    /// </summary>
    private void ShowLoss(Label label, EntityStats entity, int minLoss, int maxLoss, bool self)
    {
        bool lethal = entity.CanDie && minLoss >= entity.CurrentHealth, mayKill = entity.CanDie && maxLoss >= entity.CurrentHealth;
        label.text = (self ? "-" : "") + (minLoss == maxLoss ? minLoss.ToString() : minLoss + "-" + maxLoss)
            + (lethal ? "\n" + Localization.UI("PreviewLethal") : mayKill ? "\n" + Localization.UI("PreviewMayKill") : "");
        label.EnableInClassList("damage-preview--self", self);
        label.RemoveFromClassList("damage-preview--armor");
        label.RemoveFromClassList("damage-preview--heal");
        label.EnableInClassList("damage-preview--lethal", lethal);
        label.EnableInClassList("damage-preview--may-kill", mayKill && !lethal);
        WorldLabels.Place(root, label, entity.transform.position, OffsetUp);
    }

    /// <summary>
    /// Writes in the label the health the artifact's own damage would cost the player: false if none
    /// </summary>
    private bool ShowSelfDamage(Artifact artifact, Label label)
    {
        (int minLoss, int maxLoss) = artifact.PreviewSelfDamage(attack.Stats);
        if (maxLoss > 0) ShowLoss(label, attack.Stats, minLoss, maxLoss, true);
        return maxLoss > 0;
    }

    /// <summary>
    /// Writes in the label the armor and health the cast would give the player: false if none. A heal lost to the full health
    /// reads as such
    /// </summary>
    private bool ShowGains(Artifact artifact, int targets, Label label)
    {
        EntityStats player = attack.Stats;
        (int armor, int heal) = artifact.PreviewGains(player, targets);
        // A heal the full health would waste reads as such
        bool heals = heal > 0;
        int healed = Mathf.Min(heal, player.MaxHealth - player.CurrentHealth);
        if (armor <= 0 && !heals) return false;
        var lines = new List<string>();
        if (armor > 0) lines.Add(string.Format(Localization.UI("PreviewArmor"), armor));
        if (heals) lines.Add(healed > 0 ? string.Format(Localization.UI("PreviewHeal"), healed) : Localization.UI("PreviewHealthFull"));
        label.text = string.Join("\n", lines);
        label.RemoveFromClassList("damage-preview--self");
        label.RemoveFromClassList("damage-preview--lethal");
        label.RemoveFromClassList("damage-preview--may-kill");
        label.EnableInClassList("damage-preview--armor", armor > 0 && !heals);
        label.EnableInClassList("damage-preview--heal", heals);
        WorldLabels.Place(root, label, player.transform.position, GainOffsetUp);
        return true;
    }

    private void Show(Artifact artifact, IReadOnlyList<EntityStats> targets)
    {
        int shown = 0;
        bool can = artifact != null && WorldLabels.CanPlace(root), damage = can && Edition.Profile.damagePreview;
        if (damage)
            foreach (EntityStats target in targets)
            {
                (int min, int max) = artifact.PreviewDamage(attack.Stats, target);
                // The armor takes the damage first
                if (max > 0) ShowLoss(labels.Show(shown++), target, Mathf.Max(0, min - target.Armor), Mathf.Max(0, max - target.Armor), false);
            }
        if (damage && ShowSelfDamage(artifact, labels.Show(shown))) shown++;
        if (can && Edition.Profile.gainPreview && ShowGains(artifact, targets.Count, labels.Show(shown))) shown++;
        labels.HideFrom(shown);
        if (shown > 0) beat.Resume();
        else beat.Pause();
    }
}
