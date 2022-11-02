using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Circle Tile Search removing tiles without Line Of Sight
/// </summary>
public class CircleAttackTS : CircleTileSearch
{
	protected override void SetConstraints() {
		base.SetConstraints();
		tileConstraints.Add(new LineOfSightConstraint());
	}
	public CircleAttackTS(Tile startingTile, int minDistance, int maxDistance) : base(startingTile, minDistance, maxDistance) {
	}
}
