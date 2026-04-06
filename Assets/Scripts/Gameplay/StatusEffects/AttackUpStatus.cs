using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackUpStatus : StatusEffect {
	public AttackUpStatus(int duration) : base(duration) {
		id = "AttackUp";
	}

	public override void OnApply(EntityStats owner) {
		base.OnApply(owner);
		owner.DamageDealtMultiplier += 0.25f;
		if (owner.HasStatusEffect("AttackDown")) {
			owner.RemoveStatusEffect(owner.GetStatusEffect("AttackDown"));
			owner.RemoveStatusEffect(this);
		}
	}

	public override void OnRemove() {
		owner.DamageDealtMultiplier -= 0.25f;
	}
}
