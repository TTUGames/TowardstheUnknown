using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : TacticsMove
{
    [SerializeField] private int distance;
    public void MoveTowardsTarget(Tile target) {
        TileSearch distanceToTarget = new CircleTileSearch(target, 1, int.MaxValue);

        Tile closestTile = currentTile;
        foreach (Tile tile in selectableTiles.GetTiles()) {
            if (Mathf.Abs(distanceToTarget.GetDistance(tile) - distance) < Mathf.Abs(distanceToTarget.GetDistance(closestTile) - distance)) {
                closestTile = tile;
            }
        }
        MoveToTile(closestTile);
    }
}
