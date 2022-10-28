using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleAttackWithLos : CircleTileSearch
{
	public CircleAttackWithLos(Tile startingTile, int minDistance, int maxDistance) : base(startingTile, minDistance, maxDistance) {
		pathConstraints = new List<TileConstraint>();

		tileConstraints = new List<TileConstraint>();
		tileConstraints.Add(new WalkableTileConstraint());
		tileConstraints.Add(new LineOfSightConstraint());
	}
}
