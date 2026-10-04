using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsAction : GameAction
{
	private EntityStats source;
	private EntityStats target;
    private int distance;
    private readonly TacticsMove.DashStyle dash;

    /// <param name="dash">How a dash towards the other entity plays, if it runs</param>
    public MoveTowardsAction(EntityStats source, EntityStats target, int distance, TacticsMove.DashStyle dash = null) {
		this.source = source;
		this.target = target;
        this.distance = distance;
        this.dash = dash;
	}

	/// <summary>
	/// The tiles an entity standing on <paramref name="from"/> crosses when moved <paramref name="distance"/> tiles towards
	/// (positive) or away from (negative) <paramref name="other"/>, from its own tile to where it stops: along the dominant
	/// axis, before the first tile missing, taken or not walkable. The previews read it as the move does
	/// </summary>
	public static List<Tile> Path(Tile from, Tile other, int distance) {
		//The grid direction along the dominant axis, snapped since the positions are floats
		Vector3 delta = other.transform.position - from.transform.position;
		Vector3 direction = Mathf.Sign(distance) * (Mathf.Abs(delta.x) > Mathf.Abs(delta.z) ? new Vector3(Mathf.Sign(delta.x), 0, 0) : new Vector3(0, 0, Mathf.Sign(delta.z)));

		List<Tile> path = new List<Tile>();
		Tile targetTile = from;
		path.Add(targetTile);
		for (int i = 0; i < Mathf.Abs(distance); ++i) {
			if (!targetTile.lAdjacent.TryGetValue(direction, out Tile newTile)) break;
			if (newTile == null || !TileConstraints.Empty(targetTile, newTile) || !TileConstraints.Walkable(targetTile, newTile)) break;
			targetTile = newTile;
			path.Add(targetTile);
		}
		return path;
	}

	public override void Apply() {
		isDone = true;
		//An entity killed by the same attack is not moved: its corpse would take the tile it lands on
		if (source.IsDead) return;
		TacticsMove sourceMove = source.Move;

		List<Tile> path = Path(sourceMove.CurrentTile, target.Tile, distance);
		Tile targetTile = path[path.Count - 1];
		path.Reverse();
		//Moving towards the other entity is a dash, away from it a push
		if (Edition.Profile.slideMoves) sourceMove.SlideToTile(targetTile, new Stack<Tile>(path), distance > 0, distance > 0 ? dash : null);
		// The original's pushes and dashes were walks
		else sourceMove.MoveToTile(targetTile, new Stack<Tile>(path), false);
	}
}
