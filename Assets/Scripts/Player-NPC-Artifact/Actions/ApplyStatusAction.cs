using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyStatusAction : Action {

	EntityStats target;
	StatusEffect statusEffect;
	public ApplyStatusAction(EntityStats target, StatusEffect statusEffect){
		this.target = target;
		this.statusEffect = statusEffect;
	}

	public override void Apply() {
		target.AddStatusEffect(statusEffect);
		isDone = true;
	}
}
