using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy's movement component
/// </summary>
public class EnemyMove : TacticsMove
{
    [SerializeField] private int canAttackBonus = 4;
    [SerializeField] private int canHideBonus = 2;

    private LineOfSightConstraint losConstraint = new LineOfSightConstraint();
    private Collider enemyCollider;

	public override void Init() {
		base.Init();
        enemyCollider = GetComponent<Collider>();
	}

    /// <summary>
    /// Defines the best tile reachable this turn and move to it
    /// </summary>
    /// <param name="target">The target the enemy wants to get closer to</param>
    /// <param name="distanceToTarget">The distance the enemy wants to stay from his target. Must be in the main attack's range</param>
    public void MoveTowardsTarget(Tile target, TileSearch attackRange, int distanceToTarget) {
        enemyCollider.enabled = false;

        Tile objectiveTile = SelectObjectiveTile(target, attackRange, distanceToTarget);

        FindSelectibleTiles(0, stats.GetMovementDistance());
        TileSearch distanceToObjective = new CircleWalkableTileSearch(0, int.MaxValue, objectiveTile);
        distanceToObjective.Search();
        attackRange.SetStartingTile(target);
        attackRange.Search();

        Tile bestTile = null;
        int bestScore = int.MinValue;
        foreach (Tile reachableTile in selectableTiles.GetTiles()) {
            if (reachableTile == objectiveTile) {
                bestTile = objectiveTile;
                break;
			}
            int currentScore = -distanceToObjective.GetDistance(reachableTile);
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
    private Tile SelectObjectiveTile(Tile target, TileSearch attackRange, int objectiveDistance) {
        GetComponent<Collider>().enabled = false;
        CurrentTile.SetEntity(null);
        attackRange.SetStartingTile(target);
        attackRange.Search();

        List<Tile> objectiveTiles = new List<Tile>();
        int bestDistanceMargin = int.MaxValue;

        foreach (Tile tile in attackRange.GetTiles()) {
            if (!new EmptyTileConstraint().isValid(null, tile)) continue;
            int distanceMargin = Mathf.Abs(objectiveDistance - attackRange.GetDistance(tile));
            if (distanceMargin < bestDistanceMargin) {
                objectiveTiles.Clear();
                bestDistanceMargin = distanceMargin;
			}
            if (distanceMargin == bestDistanceMargin) {
                objectiveTiles.Add(tile);
			}
        }

        if (objectiveTiles.Count == 0) return target;

        Tile objectiveTile = null;
        TileSearch distanceToSource = new CircleTileSearch(0, int.MaxValue, currentTile);
        distanceToSource.Search();

        foreach(Tile tile in objectiveTiles) {
            if (objectiveTile == null || distanceToSource.GetDistance(tile) < distanceToSource.GetDistance(objectiveTile)) 
                objectiveTile = tile;
		}

        GetComponent<Collider>().enabled = true;
        CurrentTile.SetEntity(this);

        return objectiveTile;
    }
}
