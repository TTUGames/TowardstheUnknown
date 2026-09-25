/// <summary>
/// Runtime instance of an <c>EnemyPatternData</c>, an attack an enemy can use during its turn
/// </summary>
public class EnemyPattern : Ability
{
    public EnemyPattern(EnemyPatternData data) : base(data) { }

    /// <summary>
    /// Checks if the pattern can be used on the target from the current position
    /// </summary>
    public bool CanTarget(Tile currentTile, EntityStats target)
    {
        TacticsMove targetMove = target.GetComponent<TacticsMove>();
        return IsTargetable(targetMove) && CanReach(currentTile, targetMove.CurrentTile);
    }
}
