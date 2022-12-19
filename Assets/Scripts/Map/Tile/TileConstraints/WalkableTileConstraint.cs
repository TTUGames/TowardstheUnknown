using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkableTileConstraint : TileConstraint
{
	public override bool isValid(Tile origin, Tile tile) {
		return tile.isWalkable;
	}
}
