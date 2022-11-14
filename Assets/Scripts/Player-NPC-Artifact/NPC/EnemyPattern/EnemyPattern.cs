using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyPattern
{
    protected TileSearch range;

    public bool CanTarget(Tile currentTile, Tile target) {
        range.SetStartingTile(currentTile);
        range.Search();
        return range.GetTiles().Contains(target);
	}

    public abstract void Use(EntityStats source, EntityStats target);
}
