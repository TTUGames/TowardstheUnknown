/// <summary>
/// Circle Tile Search for movement. Paths go around the collectables, which are reached only when targeted
/// </summary>
public class MovementTS : CircleTileSearch {
	public MovementTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
		pathConstraints.Add(new EmptyTileConstraint());
		pathConstraints.Add(new WalkableTileConstraint());
		pathConstraints.Add(new NoCollectableTileConstraint());
		tileConstraints.Add(new EmptyTileConstraint());
	}
}
