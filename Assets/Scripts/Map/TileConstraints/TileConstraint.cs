using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileConstraint
{
    public abstract bool isValid(Tile origin, Tile tile);

    public static bool CheckTileConstraints(List<TileConstraint> constraints, Tile origin, Tile tile, List<Tile> ignoreConstraintTiles = null) {
        if (constraints == null) return true;
        if (ignoreConstraintTiles != null && ignoreConstraintTiles.Contains(tile)) return true;
        foreach (TileConstraint constraint in constraints) {
            if (!constraint.isValid(origin, tile)) return false;
		}
        return true;
	}
}
