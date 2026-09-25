/// <summary>
/// Circle Tile Search only going through walkable tiles
/// </summary>
public class CircleWalkableTileSearch : CircleTileSearch {
	/// <param name="avoidCollectables">If paths must go around the collectables instead of through them</param>
	public CircleWalkableTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null, bool avoidCollectables = false) : base(minRange, maxRange, startingTile) {
		pathConstraints.Add(new WalkableTileConstraint());
		if (avoidCollectables) pathConstraints.Add(new NoCollectableTileConstraint());
	}
}
