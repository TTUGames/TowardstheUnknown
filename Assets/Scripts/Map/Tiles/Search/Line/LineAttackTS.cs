/// <summary>
/// Gets all the tiles aligned and within a distance from the target, only with line of sight
/// </summary>
public class LineAttackTS : LineTileSearch
{
	public LineAttackTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
		tileConstraints.Add(new LineOfSightConstraint());
	}
}
