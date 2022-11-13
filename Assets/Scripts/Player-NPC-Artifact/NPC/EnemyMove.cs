using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : TacticsMove
{
    [SerializeField] private int distance;
    private TileSearch distanceToTarget = new CircleTileSearch(1, int.MaxValue);

    public void MoveTowardsTarget(Tile target) {
        distanceToTarget.SetStartingTile(target);
        distanceToTarget.Search();

        FindSelectibleTiles();

        Tile closestTile = CurrentTile;
        foreach (Tile tile in selectableTiles.GetTiles()) {
            if (Mathf.Abs(distanceToTarget.GetDistance(tile) - distance) < Mathf.Abs(distanceToTarget.GetDistance(closestTile) - distance)) {
                closestTile = tile;
            }
        }
        MoveToTile(closestTile);
    }
}
