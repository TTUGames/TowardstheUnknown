public class WalkableTileConstraint : TileConstraint
{
	public override bool isValid(Tile origin, Tile tile) {
		return tile.isWalkable;
	}
}
