/// <summary>
/// Circle Tile Search removing tiles without Line Of Sight
/// </summary>
public class CircleAttackTS : CircleTileSearch
{
	public CircleAttackTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
		tileConstraints.Add(new LineOfSightConstraint());
	}
}
