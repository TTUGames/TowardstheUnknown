using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraregAttack : EnemyAttack
{
    private EnemyPattern specialAttackSuccess;
    private EnemyPattern specialAttackFail;

    public void SetSpecialPattern(EnemyPattern onSuccess, EnemyPattern onFail) {
        specialAttackSuccess = onSuccess;
        specialAttackFail = onFail;
	}

	public void UseSpecialPattern(EntityStats target) {
        SetCurrentTile();
        if (specialAttackSuccess.CanTarget(currentTile, target)) {
            specialAttackSuccess.Use(stats, target);
        }
        else {
            specialAttackFail.Use(stats, target);
		}
    }
}
