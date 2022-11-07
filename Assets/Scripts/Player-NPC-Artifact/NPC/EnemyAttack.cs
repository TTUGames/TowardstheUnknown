using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : TacticsAttack
{
	public void Attack(EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(GetComponent<EntityStats>(), target, 10, 10));
	}

	public void TryAttack(EntityStats target) {
		FindSelectibleTiles(new AreaInfo(1, 1, AreaType.CIRCLE));
		if (selectableTiles.GetTiles().Contains(target.GetComponent<TacticsMove>().currentTile)) {
			Attack(target);
		}
	}
}
