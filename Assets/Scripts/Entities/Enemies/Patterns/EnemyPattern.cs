/// <summary>
/// Runtime instance of an <c>EnemyPatternData</c>, an attack an enemy can use during its turn
/// </summary>
public class EnemyPattern : Ability<EnemyPatternData>
{
    public EnemyPattern(EnemyPatternData data) : base(data) { }

    /// <summary>
    /// Aimed at the enemies' own side: cast on the enemy's own tile, whatever its range (a buff on itself), and it threatens nothing
    /// </summary>
    public bool OnSelf => Target == EntityType.ENEMY;

    /// <summary>
    /// Checks if the pattern can be used on the target from the current position; one on itself always can
    /// </summary>
    public bool CanTarget(Tile currentTile, EntityStats target)
    {
        if (OnSelf) return true;
        return IsTargetable(target.Move) && CanReach(currentTile, target.Tile);
    }
}
