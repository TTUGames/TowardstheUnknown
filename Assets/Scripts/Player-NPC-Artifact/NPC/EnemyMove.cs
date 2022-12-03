using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy's movement component
/// </summary>
public class EnemyMove : TacticsMove
{
    private const int canAttackBonus = 4;
    private const int canHideBonus = 2;

    private LineOfSightConstraint losConstraint = new LineOfSightConstraint();
    private TileSearch attackRange;
    private Collider enemyCollider;

	public override void Init() {
		base.Init();
        enemyCollider = GetComponent<Collider>();
	}

    /// <summary>
    /// Defines the enemy's main attack range, which the enemy will try to stay in
    /// </summary>
    /// <param name="attackRange"></param>
	public void SetAttackRange(TileSearch attackRange) {
        this.attackRange = attackRange;
	}

    /// <summary>
    /// Defines the best tile reachable this turn and move to it
    /// </summary>
    /// <param name="target">The target the enemy wants to get closer to</param>
    /// <param name="distanceToTarget">The distance the enemy wants to stay from his target. Must be in the main attack's range</param>
    public void MoveTowardsTarget(Tile target, int distanceToTarget) {
        enemyCollider.enabled = false;
        UpdateAttackRange(target);

        Tile objectiveTile = SelectObjectiveTile(target, distanceToTarget);

        TileSearch movementToObjective = new MovementTS(0, int.MaxValue, objectiveTile);
        movementToObjective.Search();

        FindSelectibleTiles(0, stats.GetMovementDistance());

        Tile bestTile = null;
        int bestScore = int.MinValue;
        foreach (Tile reachableTile in selectableTiles.GetTiles()) {
            if (reachableTile == objectiveTile) {
                bestTile = objectiveTile;
                break;
			}
            if (reachableTile == CurrentTile) continue;
            int currentScore = - movementToObjective.GetDistance(reachableTile);
            if (attackRange.GetTiles().Contains(reachableTile)) currentScore += canAttackBonus;
            else if (!losConstraint.isValid(target, reachableTile)) currentScore += canHideBonus;

            if (currentScore > bestScore) {
                bestScore = currentScore;
                bestTile = reachableTile;
			}
		}

        enemyCollider.enabled = true;
        MoveToTile(bestTile);
    }

    /// <summary>
    /// Defines the best possible tile without taking the movement range into account
    /// </summary>
    /// <param name="target"></param>
    /// <param name="objectiveDistance"></param>
    /// <returns></returns>
    private Tile SelectObjectiveTile(Tile target, int objectiveDistance) {
        if (attackRange.GetTiles().Count == 0) return target;
        Dictionary<int, List<Tile>> objectiveTiles = new Dictionary<int, List<Tile>>();

        FindSelectibleTiles(0, int.MaxValue);
        foreach (Tile tile in attackRange.GetTiles()) {
            if (! new EmptyTileConstraint().isValid(null, tile)) continue;
            int distance = attackRange.GetDistance(tile);
            if (!objectiveTiles.ContainsKey(distance))
                objectiveTiles.Add(distance, new List<Tile>());
            objectiveTiles[distance].Add(tile);
        }

        int bestDistance = int.MaxValue;
        foreach (int distance in objectiveTiles.Keys) {
            if (Mathf.Abs(distance - objectiveDistance) < Mathf.Abs(bestDistance - objectiveDistance)) bestDistance = distance;
		}
        Tile objectiveTile = null;
        foreach(Tile tile in objectiveTiles[bestDistance]) {
            if (!selectableTiles.GetTiles().Contains(tile)) continue;
            if (objectiveTile == null || selectableTiles.GetDistance(tile) < selectableTiles.GetDistance(objectiveTile)) 
                objectiveTile = tile;
		}

        if (objectiveTile == null) objectiveTile = currentTile; //Fix in case no path is found towards the target

        return objectiveTile;
    }

    /// <summary>
    /// Recalculates the attack <c>TileSearch</c>
    /// </summary>
    /// <param name="target"></param>
    private void UpdateAttackRange(Tile target) {
        attackRange.SetStartingTile(target);
        attackRange.Search();
    }
}
