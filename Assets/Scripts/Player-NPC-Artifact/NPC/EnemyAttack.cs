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

	public void AddPattern(EnemyPattern pattern) {
		patterns.Add(pattern);
	}

	public void Attack(EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(GetComponent<EntityStats>(), target, 10, 10));
	}

	public void TryAttack(EntityStats target) {
		SetCurrentTile();
		foreach (EnemyPattern pattern in patterns) {
			if (pattern.CanTarget(currentTile, target.GetComponent<TacticsMove>().CurrentTile)) {
				pattern.Use(stats, target);
				return;
			}
		}
	}
}
