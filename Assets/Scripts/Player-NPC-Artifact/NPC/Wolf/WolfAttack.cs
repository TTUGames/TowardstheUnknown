using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAttack : EnemyAttack
{
	protected override void SetPatterns() {
		patterns = new List<EnemyPattern>();
		patterns.Add(new WolfClawPattern());
		patterns.Add(new WolfHowlPattern());
	}
}
