using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsAction : GameAction
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
		isDone = true;
		//An entity killed by the same attack is not moved: its corpse would take the tile it lands on
		if (source.IsDead) return;
		TacticsMove sourceMove = source.Move;

		//The grid direction along the dominant axis, snapped since the positions are floats
		Vector3 delta = target.Tile.transform.position - sourceMove.CurrentTile.transform.position;
		Vector3 direction = Mathf.Sign(distance) * (Mathf.Abs(delta.x) > Mathf.Abs(delta.z) ? new Vector3(Mathf.Sign(delta.x), 0, 0) : new Vector3(0, 0, Mathf.Sign(delta.z)));

		List<Tile> path = new List<Tile>();
		Tile targetTile = sourceMove.CurrentTile;
		path.Add(targetTile);
		for (int i = 0; i < Mathf.Abs(distance); ++i) {
			if (!targetTile.lAdjacent.TryGetValue(direction, out Tile newTile)) break;
			if (newTile == null || !TileConstraints.Empty(targetTile, newTile) || !TileConstraints.Walkable(targetTile, newTile)) break;
			targetTile = newTile;
			path.Add(targetTile);
		}
		path.Reverse();
		//Moving towards the other entity is a dash, away from it a push
		if (Edition.Profile.slideMoves) sourceMove.SlideToTile(targetTile, new Stack<Tile>(path), distance > 0);
		// The original's pushes and dashes were walks
		else sourceMove.MoveToTile(targetTile, new Stack<Tile>(path), false);
	}
}
