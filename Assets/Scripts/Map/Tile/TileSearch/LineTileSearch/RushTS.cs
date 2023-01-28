using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets all the tiles aligned and within a distance from the target, as long as they're reachable by movement
/// </summary>
public class RushTS : LineTileSearch
{
	protected override void SetConstraints() {
		base.SetConstraints();
		pathConstraints.Add(new WalkableTileConstraint());
		pathConstraints.Add(new LineOfSightConstraint());
	}
	public RushTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
	}
}