using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsAction : Action
{
	private EntityStats source;
	private EntityStats target;
    private int distance;

    public MoveTowardsAction(EntityStats source, EntityStats target, int distance) {
		this.source = source;
		this.target = target;
        this.distance = distance;
	}

	public override void Apply() {
		TacticsMove sourceMove = source.GetComponent<TacticsMove>();
		TacticsMove targetMove = target.GetComponent<TacticsMove>();

		Vector3 direction = targetMove.currentTile.transform.position - sourceMove.currentTile.transform.position;
		if (!(direction.x == 0 ^ direction.z == 0)) throw new System.Exception("Target and Source are not valid for MoveTowardsAction");
		direction = direction.normalized;
		if (distance < 0) direction = -direction;

		Tile targetTile = sourceMove.currentTile;
		for (int i = 0; i < Mathf.Abs(distance); ++i) {
			Tile newTile = targetTile.lAdjacent[direction];
			if (newTile == null || !TileConstraint.CheckTileConstraints(TileConstraint.defaultMovePathConstraints, targetTile, newTile)) break;
			targetTile = newTile;
		}

		sourceMove.currentTile.MakePathAndGetDistance(targetTile);
		sourceMove.MoveToTile(targetTile, false);

		isDone = true;
	}
}
