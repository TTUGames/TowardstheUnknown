/// <summary>
/// Rejects the tiles holding a collectable, so that paths go around it: it is only reached when it is the destination
/// </summary>
public class NoCollectableTileConstraint : TileConstraint {
	public override bool isValid(Tile origin, Tile tile) {
		return origin == tile || tile.Collectable == null;
	}
}
