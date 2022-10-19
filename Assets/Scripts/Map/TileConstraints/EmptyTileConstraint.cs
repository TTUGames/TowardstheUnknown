using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyTileConstraint : TileConstraint {
	public override bool isValid(Tile origin, Tile tile) {
		return origin == tile || !Physics.Raycast(tile.transform.position, Vector3.up);
	}
}
