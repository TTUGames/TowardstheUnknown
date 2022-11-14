using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Circle Tile Search for movement
/// </summary>
public class MovementTS : CircleTileSearch {

	protected override void SetConstraints() {
		base.SetConstraints();
		pathConstraints.Add(new EmptyTileConstraint());
	}

	public MovementTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
	}
}
