using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : TacticsMove
{
    private TileSearch distanceToTarget = new CircleTileSearch(1, int.MaxValue);

    public void MoveTowardsTarget(Tile target, int distanceToTarget) {
        this.distanceToTarget.SetStartingTile(target);
        this.distanceToTarget.Search();

        FindSelectibleTiles();

        Tile closestTile = CurrentTile;
        foreach (Tile tile in selectableTiles.GetTiles()) {
            if (Mathf.Abs(this.distanceToTarget.GetDistance(tile) - distanceToTarget) < Mathf.Abs(this.distanceToTarget.GetDistance(closestTile) - distanceToTarget)) {
                closestTile = tile;
            }
        }
        MoveToTile(closestTile);
    }
}
