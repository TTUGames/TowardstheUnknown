using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTileSearch : CircleTileSearch {

	public MovementTileSearch(Tile startingTile, int minDistance, int maxDistance) : base(startingTile, minDistance, maxDistance) {
		pathConstraints = new List<TileConstraint>();
		pathConstraints.Add(new WalkableTileConstraint());
		pathConstraints.Add(new EmptyTileConstraint());

		tileConstraints = new List<TileConstraint>();
	}
}
