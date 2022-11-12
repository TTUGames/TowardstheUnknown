using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportAI : EnemyAI {
	protected override void SetAttackPatterns() {
		attack.AddPattern(new SupportBuffPattern());
	}

	protected override void SetTargetting() {
		targetting = new ClosestEnemyTargetting(2, new PlayerTargetting(1));
	}
}
