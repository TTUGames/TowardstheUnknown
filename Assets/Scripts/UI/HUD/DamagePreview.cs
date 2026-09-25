using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Over each entity the player's selected artifact would hit: the health it would lose, and whether the hit kills it for sure or may kill it
/// </summary>
public class DamagePreview : IDisposable
{
    // Above the entity, in panel points
    private const float OffsetUp = 150;

    private readonly VisualElement root;
    private readonly PlayerAttack attack;
    private readonly List<Label> labels = new();

    public DamagePreview(VisualElement root, PlayerAttack attack)
    {
        this.root = root;
        this.attack = attack;
        attack.TargetsPreviewed += Show;
    }

    public void Dispose()
    {
        if (attack != null) attack.TargetsPreviewed -= Show;
    }

    private void Show(Artifact artifact, IReadOnlyList<EntityStats> targets)
    {
        int shown = 0;
        if (artifact != null && root.panel != null && Camera.main != null)
            foreach (EntityStats target in targets)
            {
                (int min, int max) = artifact.PreviewDamage(attack.Stats, target);
                if (max <= 0) continue;
                // The armor takes the damage first
                int minLoss = Mathf.Max(0, min - target.Armor), maxLoss = Mathf.Max(0, max - target.Armor);
                bool lethal = minLoss >= target.CurrentHealth, mayKill = maxLoss >= target.CurrentHealth;

                if (shown == labels.Count)
                {
                    var created = new Label { pickingMode = PickingMode.Ignore };
                    created.AddToClassList("damage-preview");
                    root.Add(created);
                    labels.Add(created);
                }
                Label label = labels[shown++];
                label.text = (minLoss == maxLoss ? minLoss.ToString() : minLoss + "-" + maxLoss)
                    + (lethal ? "\n" + Localization.UI("PreviewLethal") : mayKill ? "\n" + Localization.UI("PreviewMayKill") : "");
                label.EnableInClassList("damage-preview--lethal", lethal);
                label.EnableInClassList("damage-preview--may-kill", mayKill && !lethal);
                Vector2 position = root.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, target.transform.position, Camera.main));
                label.style.left = position.x;
                label.style.top = position.y - OffsetUp;
                label.style.display = DisplayStyle.Flex;
            }
        for (int i = shown; i < labels.Count; i++)
            labels[i].style.display = DisplayStyle.None;
    }
}
