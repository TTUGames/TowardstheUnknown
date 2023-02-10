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

		Vector3 direction = targetMove.CurrentTile.transform.position - sourceMove.CurrentTile.transform.position;

		//Normalize the vector, must be done manually because the float substraction doesn't always result in 0
		if (Mathf.Abs(direction.x) <= 0.01 && Mathf.Abs(direction.z) >= 0.99) {
			direction = direction.z > 0 ? Vector3.forward : Vector3.back;
		}
		else if (Mathf.Abs(direction.z) <= 0.01 && Mathf.Abs(direction.x) >= 0.99) {
			direction = direction.x > 0 ? Vector3.right : Vector3.left;
		}
		else throw new System.Exception("Target and Source are not valid for MoveTowardsAction");

		if (distance < 0) direction = -direction;

		List<Tile> path = new List<Tile>();
		Tile targetTile = sourceMove.CurrentTile;
		path.Add(targetTile);
		for (int i = 0; i < Mathf.Abs(distance); ++i) {
			if (!targetTile.lAdjacent.ContainsKey(direction)) break;
			Tile newTile = targetTile.lAdjacent[direction];
			if (newTile == null || ! new EmptyTileConstraint().isValid(targetTile, newTile) || ! new WalkableTileConstraint().isValid(targetTile, newTile)) break;
			targetTile = newTile;
			path.Add(targetTile);
		}
		path.Reverse();
		sourceMove.MoveToTile(targetTile, new Stack<Tile>(path), false);

		isDone = true;
	}
}
