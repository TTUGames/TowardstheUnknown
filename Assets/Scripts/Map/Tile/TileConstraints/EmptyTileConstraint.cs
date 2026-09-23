public class EmptyTileConstraint : TileConstraint {
	public override bool isValid(Tile origin, Tile tile) {
		return origin == tile || tile.GetEntity() == null;
	}
}
