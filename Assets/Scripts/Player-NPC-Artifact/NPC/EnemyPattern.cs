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
    protected string animStateName = "";

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
        Debug.Log("ALED");
        if (target.type != targetType) return false;
        Debug.Log("CORRECT TYPE");
        range.SetStartingTile(currentTile);
        range.Search();
        foreach (Tile tile in range.GetTiles()) {
            Debug.Log(tile);
		}
        return range.GetTiles().Contains(target.GetComponent<TacticsMove>().CurrentTile);
	}

    /// <summary>
    /// Play the pattern's VFX and animation
    /// </summary>
    public abstract void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source);

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
