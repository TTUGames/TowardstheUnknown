using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : Action
{
    TacticsMove move;
    public MoveAction(TacticsMove move) {
        this.move = move;
	}

	public override void Apply() {
		move.Move();
		if (!move.isMoving) {
			Tile.ResetTargetTiles();
			isDone = true;
		}
	}
}
