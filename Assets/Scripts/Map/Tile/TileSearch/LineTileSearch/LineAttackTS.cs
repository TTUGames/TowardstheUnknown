using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets all the tiles aligned and within a distance from the target, only with line of sight
/// </summary>
public class LineAttackTS : LineTileSearch
{
	protected override void SetConstraints() {
		base.SetConstraints();
		tileConstraints.Add(new LineOfSightConstraint());
	}
	public LineAttackTS(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) {
	}
}
