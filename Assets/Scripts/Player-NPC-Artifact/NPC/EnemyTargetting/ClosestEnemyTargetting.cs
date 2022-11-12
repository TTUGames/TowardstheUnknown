using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestEnemyTargetting : AbstractTargetting {
	public TurnSystem turnSystem;
	public AbstractTargetting fallback;

	public ClosestEnemyTargetting(int distance, AbstractTargetting fallback) : base(distance) {
		this.fallback = fallback;
		turnSystem = GameObject.FindObjectOfType<TurnSystem>();
	}
	public override EntityStats GetTarget(EntityStats source) {
		List<EntityTurn> enemies = turnSystem.GetEnemies();
		if (enemies.Count == 1) return fallback.GetTarget(source);

		TileSearch search = new CircleTileSearch(1, int.MaxValue, source.GetComponent<TacticsMove>().CurrentTile);
		EnemyStats closestEnemy = null;
		foreach (EntityTurn enemy in enemies) {
			if (enemy == source.GetComponent<EntityTurn>()) continue;
			if (closestEnemy == null
				|| search.GetDistance(enemy.GetComponent<TacticsMove>().CurrentTile)
					< search.GetDistance(closestEnemy.GetComponent<TacticsMove>().CurrentTile)) {
				closestEnemy = enemy.GetComponent<EnemyStats>();
			}
		}
		return closestEnemy;
	}
}
