using UnityEngine;

/// <summary>
/// Runtime instance of an <c>EnemyPatternData</c>, an attack an enemy can use during its turn
/// </summary>
public class EnemyPattern
{
    private readonly EnemyPatternData data;
    private readonly TileSearch range;

    public EnemyPattern(EnemyPatternData data) {
        this.data = data;
        range = data.range.Create();
	}

    /// <summary>
    /// Checks if the pattern can be used on the target from the current position
    /// </summary>
    public bool CanTarget(Tile currentTile, EntityStats target) {
        if (target.type != data.targetType) return false;
        range.SetStartingTile(currentTile);
        range.Search();
        return range.Contains(target.GetComponent<TacticsMove>().CurrentTile);
	}

    /// <summary>
    /// Play the pattern's VFX and animation
    /// </summary>
    public void PlayAnimation(Tile sourceTile, Tile targetTile, GameObject source) {
        float rotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
        source.transform.rotation = Quaternion.Euler(0, rotation, 0);

        ActionManager.AddToBottom(new AttackAnimationAction(source, targetTile, data.duration, data.animStateName, data.vfx));
    }

    public void PlaySound(GameObject gameObject) {
        AkUnitySoundEngine.PostEvent(data.name, gameObject);
    }

    /// <summary>
    /// Use this pattern from the source on the target
    /// </summary>
    public void Use(EntityStats source, EntityStats target) {
        foreach (CombatEffect effect in data.effects) effect.Apply(source, target);
    }

    /// <summary>
    /// Gets the pattern's range
    /// </summary>
    public TileSearch GetRange() {
        return range;
	}
}
