/// <summary>
/// Circle Tile Search for movement
/// </summary>
public class MovementTS : CircleTileSearch {
	public MovementTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
		pathConstraints.Add(new EmptyTileConstraint());
		pathConstraints.Add(new WalkableTileConstraint());
		tileConstraints.Add(new EmptyTileConstraint());
	}
}
