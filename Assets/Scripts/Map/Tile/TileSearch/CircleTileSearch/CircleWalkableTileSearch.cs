using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleWalkableTileSearch : CircleTileSearch {
	protected override void SetConstraints() {
		base.SetConstraints();
		pathConstraints.Add(new WalkableTileConstraint());
	}

	public CircleWalkableTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {

	}
}
