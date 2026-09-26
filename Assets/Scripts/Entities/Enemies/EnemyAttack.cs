using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : TacticsAttack
{
	protected List<EnemyPattern> patterns = new List<EnemyPattern>();
	protected EnemyStats stats;

	protected override void Init() {
		base.Init();
		stats = GetComponent<EnemyStats>();
	}

	/// <summary>
	/// Adds a pattern to the enemy's pattern list. The first added is defined as the main pattern
	/// </summary>
	/// <param name="pattern"></param>
	public void AddPattern(EnemyPattern pattern) {
		patterns.Add(pattern);
	}

	public void ClearPatterns() {
		patterns.Clear();
	}

	/// <summary>
	/// Tries to use the first pattern possible, in the order they were added
	/// </summary>
	/// <param name="target"></param>
	public void TryAttack(EntityStats target) {
		EnemyPattern pattern = patterns.Find(p => p.CanTarget(CurrentTile, target));
		if (pattern != null) UsePattern(pattern, target);
	}

	protected void UsePattern(EnemyPattern pattern, EntityStats target) {
		pattern.Cast(stats, target.GetComponent<TacticsMove>().CurrentTile);
	}

	/// <summary>
	/// The tiles the enemy can hit the player on this turn: the ranges of its attacks from every tile it can walk to
	/// </summary>
	public HashSet<Tile> GetThreatenedTiles() {
		var threatened = new HashSet<Tile>();
		if (CurrentTile == null) return threatened;
		var reachable = TileSearch.Movement(stats.maxMovementPoints, CurrentTile);
		reachable.Search();
		var origins = reachable.GetTiles();
		origins.Add(CurrentTile);

		//The enemy's own body must not block the lines of sight from its future positions
		Collider body = GetComponent<Collider>();
		bool bodyEnabled = body != null && body.enabled;
		if (body != null) body.enabled = false;
		foreach (EnemyPattern pattern in patterns) {
			if (pattern.Target != EntityType.PLAYER) continue;
			foreach (Tile origin in origins) {
				pattern.Range.SetStartingTile(origin);
				pattern.Range.Search();
				threatened.UnionWith(pattern.Range.GetTiles());
			}
		}
		if (body != null) body.enabled = bodyEnabled;
		return threatened;
	}

	/// <summary>
	/// Returns the enemy's favorite pattern
	/// </summary>
	/// <returns></returns>
	public EnemyPattern GetFavoritePattern() {
		return patterns[0];
	}
}
