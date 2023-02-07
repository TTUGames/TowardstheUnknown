using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : TacticsAttack
{
	protected List<EnemyPattern> patterns = new List<EnemyPattern>();
	protected EnemyStats stats;

	private void Start() {
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
	public virtual void TryAttack(EntityStats target) {
		SetCurrentTile();
		foreach (EnemyPattern pattern in patterns) {
			if (pattern.CanTarget(currentTile, target)) {
				pattern.Use(stats, target);
				pattern.PlayAnimation(currentTile, target.GetComponent<TacticsMove>().CurrentTile, gameObject);
				pattern.PlaySound(gameObject);
				return;
			}
		}
	}

	/// <summary>
	/// Returns the enemy's favorite pattern
	/// </summary>
	/// <returns></returns>
	public EnemyPattern GetFavoritePattern() {
		return patterns[0];
	}
}
