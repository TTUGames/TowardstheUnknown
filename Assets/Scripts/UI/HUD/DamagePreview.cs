using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Over each entity the player's selected artifact would hit: the health it would lose, and whether the hit kills it for sure or may kill it,
/// those previews beating
/// </summary>
public class DamagePreview : IDisposable
{
    // Above the entity, in panel points
    private const float OffsetUp = 150;
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
                bool lethal = minLoss >= target.CurrentHealth, mayKill = maxLoss >= target.CurrentHealth;

                Label label = labels.Show(shown++);
                label.text = (minLoss == maxLoss ? minLoss.ToString() : minLoss + "-" + maxLoss)
                    + (lethal ? "\n" + Localization.UI("PreviewLethal") : mayKill ? "\n" + Localization.UI("PreviewMayKill") : "");
                label.EnableInClassList("damage-preview--lethal", lethal);
                label.EnableInClassList("damage-preview--may-kill", mayKill && !lethal);
                WorldLabels.Place(root, label, target.transform.position, OffsetUp);
            }
        labels.HideFrom(shown);
        if (shown > 0) beat.Resume();
        else beat.Pause();
    }
}
