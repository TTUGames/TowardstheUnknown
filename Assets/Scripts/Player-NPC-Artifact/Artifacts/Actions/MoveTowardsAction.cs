using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsAction : IAction
{
    private int distance;

    public MoveTowardsAction(int distance) {
        this.distance = distance;
	}

	public void Use(EntityStats source, EntityStats target) {
		TacticsMove sourceMove = source.GetComponent<TacticsMove>();
		TacticsMove targetMove = target.GetComponent<TacticsMove>();

		Vector3 direction = targetMove.currentTile.transform.position - sourceMove.currentTile.transform.position;
		if (!(direction.x == 0 ^ direction.z == 0)) throw new System.Exception("Target and Source are not valid for MoveTowardsAction");
		direction = direction.normalized;

		Tile targetTile = sourceMove.currentTile;
		for (int i = 0; i < distance; ++i) {
			Tile newTile = targetTile.lAdjacent[direction];
			if (newTile == null || !TileConstraint.CheckTileConstraints(TileConstraint.defaultMovePathConstraints, targetTile, newTile)) break;
			targetTile = newTile;
		}

		sourceMove.MoveToTile(targetTile, false);
	}
}
