using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An enemy's attack shows the tiles it hits in red as it starts, for <see cref="EditionProfile.attackTelegraph"/> real
/// seconds, so that the player reads where the blow falls before it lands. Purely visual: the queue doesn't wait for it
/// </summary>
public class AttackTelegraph : MonoBehaviour
{
    private void OnEnable() => GameEvents.AttackStarted += OnAttackStarted;

    private void OnDisable() => GameEvents.AttackStarted -= OnAttackStarted;

    private void OnAttackStarted(EntityStats caster, IReadOnlyList<Tile> tiles)
    {
        float seconds = Edition.Profile.attackTelegraph;
        if (seconds <= 0 || caster is not EnemyStats) return;
        foreach (Tile tile in tiles)
            if (tile != null) tile.FlashThreat(seconds);
    }
}
