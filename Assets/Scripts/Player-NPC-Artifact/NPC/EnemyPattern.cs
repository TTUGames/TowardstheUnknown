using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyPattern
{
    protected TileSearch range;
    protected EntityType targetType;
    protected GameObject vfx;

    public EnemyPattern() {
        Init();
	}

    /// <summary>
    /// Initializes the pattern's range, target type and vfx
    /// </summary>
    public abstract void Init();

    public bool CanTarget(Tile currentTile, EntityStats target) {
        if (target.type != targetType) return false;

        range.SetStartingTile(currentTile);
        range.Search();
        return range.GetTiles().Contains(target.GetComponent<TacticsMove>().CurrentTile);
	}

    protected void PlayVFX(EntityStats target) {
        GameObject.Instantiate(vfx, target.GetComponent<TacticsMove>().CurrentTile.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
    }

    public abstract void Use(EntityStats source, EntityStats target);
}
