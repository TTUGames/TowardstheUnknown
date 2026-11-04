using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// How an enemy picks the entity it moves towards and attacks, among its hostiles (<see cref="Hostiles"/>), at the start of
/// each of its turns. Set on an <see cref="EnemyPatternSet"/> ([SerializeReference]); none is <see cref="TargetPlayer"/>,
/// the game's behaviour until there are other targets (a decoy, a summon, a second player)
/// </summary>
[System.Serializable]
public abstract class EnemyTargeting
{
    public static readonly EnemyTargeting Default = new TargetPlayer();

    /// <summary>
    /// The target among the hostiles, null if there is none
    /// </summary>
    public abstract EntityStats Pick(EnemyAI enemy, IReadOnlyList<EntityStats> hostiles);

    /// <summary>
    /// The living entities on the player's side, in turn order (the player first)
    /// </summary>
    public static List<EntityStats> Hostiles() => TurnSystem.Instance.Turns.OfType<PlayerTurn>()
        .Select(turn => (EntityStats)turn.Stats).Where(stats => stats != null && stats.CurrentHealth > 0).ToList();

    /// <summary>
    /// The distance in tiles between two entities, along the board's axes
    /// </summary>
    protected static int Distance(EntityStats from, EntityStats to)
    {
        if (from.Tile == null || to.Tile == null) return int.MaxValue;
        Vector3 offset = from.Tile.transform.position - to.Tile.transform.position;
        return Mathf.RoundToInt(Mathf.Abs(offset.x) + Mathf.Abs(offset.z));
    }
}

/// <summary>
/// Always the player
/// </summary>
[System.Serializable]
public class TargetPlayer : EnemyTargeting
{
    public override EntityStats Pick(EnemyAI enemy, IReadOnlyList<EntityStats> hostiles) => GameScene.Player.Stats;
}

/// <summary>
/// The nearest hostile; the first in turn order on a tie
/// </summary>
[System.Serializable]
public class TargetNearest : EnemyTargeting
{
    public override EntityStats Pick(EnemyAI enemy, IReadOnlyList<EntityStats> hostiles)
    {
        EntityStats self = enemy.GetComponent<EntityStats>();
        return hostiles.OrderBy(hostile => Distance(self, hostile)).FirstOrDefault();
    }
}

/// <summary>
/// The hostile with the least health and armor left, an assassin's; the first in turn order on a tie
/// </summary>
[System.Serializable]
public class TargetWeakest : EnemyTargeting
{
    public override EntityStats Pick(EnemyAI enemy, IReadOnlyList<EntityStats> hostiles) =>
        hostiles.OrderBy(hostile => hostile.CurrentHealth + hostile.Armor).FirstOrDefault();
}
