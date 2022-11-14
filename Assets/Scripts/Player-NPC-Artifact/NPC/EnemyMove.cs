using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : TacticsMove
{
    public void MoveTowardsTarget(Tile target, int distanceToTarget) {
        Tile objectiveTile = SelectObjectiveTile(target, distanceToTarget);

        TileSearch searchFromObjective = new MovementTS(0, int.MaxValue, objectiveTile);
        searchFromObjective.Search();

        FindSelectibleTiles();

        Tile bestTile = null;
        int bestScore = int.MaxValue;
        foreach (Tile reachableTile in selectableTiles.GetTiles()) {
            if (reachableTile == objectiveTile) {
                bestTile = objectiveTile;
                break;
			}
            if (reachableTile == this.CurrentTile) continue;
            int currentScore = searchFromObjective.GetDistance(reachableTile);

            if (currentScore < bestScore) {
                bestScore = currentScore;
                bestTile = reachableTile;
			}
		}

        
        MoveToTile(bestTile);
    }

    private Tile SelectObjectiveTile(Tile target, int distanceToTarget) {
        Tile objectiveTile = null;
        TileSearch objectiveTileSearch = new CircleTileSearch(distanceToTarget, distanceToTarget, target);
        objectiveTileSearch.Search();

        FindSelectibleTiles(int.MaxValue);

        foreach (Tile possibleObjectiveTile in objectiveTileSearch.GetTiles()) {
            if (objectiveTile == null || selectableTiles.GetDistance(possibleObjectiveTile) < selectableTiles.GetDistance(objectiveTile))
                objectiveTile = possibleObjectiveTile;
        }
        return objectiveTile;
    }
}
