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

    protected List<VFXInfo> vfxInfos = new List<VFXInfo>();

    protected string animStateName = "";
    protected float patternDuration = 0;

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
    /// Play the pattern's VFX and animation
    /// </summary>
    public void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source) {
        float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
        source.transform.rotation = Quaternion.Euler(0, rotation, 0);


        Animator animator = source.GetComponent<Animator>();
        if (animator != null && animStateName != "") animator.Play(animStateName);

        WaitForAttackEndAction action = new WaitForAttackEndAction(patternDuration, source.gameObject, null);
        ActionManager.AddToBottom(action);

        foreach (VFXInfo vfxInfo in vfxInfos) {
            vfxInfo.Play(action, source.gameObject, targetTile);
        }
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

    protected void RotateTowardsTarget(Tile sourceTile, Tile targetTile, GameObject source) {
        float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
        source.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }
}
