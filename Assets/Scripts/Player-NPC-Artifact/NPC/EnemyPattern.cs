using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a pattern an enemy can use during his turn
/// </summary>
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

    /// <summary>
    /// Checks if the pattern can be used on the target from the current position
    /// </summary>
    /// <param name="currentTile"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool CanTarget(Tile currentTile, EntityStats target) {
        if (target.type != targetType) return false;

        range.SetStartingTile(currentTile);
        range.Search();
        return range.GetTiles().Contains(target.GetComponent<TacticsMove>().CurrentTile);
	}

    /// <summary>
    /// Plays the pattern's SFX
    /// </summary>
    /// <param name="target"></param>
    protected void PlayVFX(EntityStats target) {
        GameObject.Instantiate(vfx, target.GetComponent<TacticsMove>().CurrentTile.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
    }

    /// <summary>
    /// Use this pattern from the source on the target
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public abstract void Use(EntityStats source, EntityStats target);

    /// <summary>
    /// Gets the pattern's range
    /// </summary>
    /// <returns></returns>
    public TileSearch GetRange() {
        return range;
	}
}
