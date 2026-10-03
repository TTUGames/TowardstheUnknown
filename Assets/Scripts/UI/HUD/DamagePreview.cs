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
        // The lethal previews beat together (transition of Hud.uss), only while some are shown
        beat = root.schedule.Execute(() => {
            for (int i = 0; i < labels.Count; i++) labels[i].ToggleInClassList("damage-preview--beat");
        }).Every(BeatInterval);
        beat.Pause();
    }

    public void Dispose()
    {
        if (attack != null) attack.TargetsPreviewed -= Show;
    }

    /// <summary>
    /// Writes in the label the health the artifact's own damage would cost the player: false if none
    /// </summary>
    private bool ShowSelfDamage(Artifact artifact, Label label)
    {
        EntityStats player = attack.Stats;
        (int minLoss, int maxLoss) = artifact.PreviewSelfDamage(player);
        if (maxLoss <= 0) return false;
        bool lethal = player.CanDie && minLoss >= player.CurrentHealth, mayKill = player.CanDie && maxLoss >= player.CurrentHealth;
        label.text = "-" + (minLoss == maxLoss ? minLoss.ToString() : minLoss + "-" + maxLoss)
            + (lethal ? "\n" + Localization.UI("PreviewLethal") : mayKill ? "\n" + Localization.UI("PreviewMayKill") : "");
        label.AddToClassList("damage-preview--self");
        label.RemoveFromClassList("damage-preview--armor");
        label.RemoveFromClassList("damage-preview--heal");
        label.EnableInClassList("damage-preview--lethal", lethal);
        label.EnableInClassList("damage-preview--may-kill", mayKill && !lethal);
        WorldLabels.Place(root, label, player.transform.position, OffsetUp);
        return true;
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
        if (artifact != null && WorldLabels.CanPlace(root))
            foreach (EntityStats target in Edition.Profile.damagePreview ? targets : System.Array.Empty<EntityStats>())
            {
                (int min, int max) = artifact.PreviewDamage(attack.Stats, target);
                if (max <= 0) continue;
                // The armor takes the damage first
                int minLoss = Mathf.Max(0, min - target.Armor), maxLoss = Mathf.Max(0, max - target.Armor);
                // An entity that cannot die (Drareg in his first phase, the training dummy) is never killed
                bool lethal = target.CanDie && minLoss >= target.CurrentHealth, mayKill = target.CanDie && maxLoss >= target.CurrentHealth;

                Label label = labels.Show(shown++);
                label.RemoveFromClassList("damage-preview--self");
                label.RemoveFromClassList("damage-preview--armor");
                label.RemoveFromClassList("damage-preview--heal");
                label.text = (minLoss == maxLoss ? minLoss.ToString() : minLoss + "-" + maxLoss)
                    + (lethal ? "\n" + Localization.UI("PreviewLethal") : mayKill ? "\n" + Localization.UI("PreviewMayKill") : "");
                label.EnableInClassList("damage-preview--lethal", lethal);
                label.EnableInClassList("damage-preview--may-kill", mayKill && !lethal);
                WorldLabels.Place(root, label, target.transform.position, OffsetUp);
            }
        if (artifact != null && Edition.Profile.damagePreview && WorldLabels.CanPlace(root) && ShowSelfDamage(artifact, labels.Show(shown))) shown++;
        if (artifact != null && Edition.Profile.gainPreview && WorldLabels.CanPlace(root) && ShowGains(artifact, targets.Count, labels.Show(shown))) shown++;
        labels.HideFrom(shown);
        if (shown > 0) beat.Resume();
        else beat.Pause();
    }
}
