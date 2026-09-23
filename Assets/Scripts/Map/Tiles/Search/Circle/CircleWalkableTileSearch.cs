/// <summary>
/// Circle Tile Search only going through walkable tiles
/// </summary>
public class CircleWalkableTileSearch : CircleTileSearch {
	public CircleWalkableTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
		pathConstraints.Add(new WalkableTileConstraint());
	}
}
