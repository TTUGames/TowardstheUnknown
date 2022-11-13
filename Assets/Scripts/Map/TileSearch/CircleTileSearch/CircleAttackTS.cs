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
	public CircleAttackTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
	}
}
