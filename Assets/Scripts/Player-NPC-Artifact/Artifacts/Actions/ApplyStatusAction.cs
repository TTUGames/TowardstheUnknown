using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyStatusAction : IAction {

	StatusEffect statusEffect;
	public ApplyStatusAction(StatusEffect statusEffect) {
		this.statusEffect = statusEffect;
	}

	public void Use(EntityStats source, EntityStats target) {
		target.AddStatusEffect(statusEffect);
	}
}
