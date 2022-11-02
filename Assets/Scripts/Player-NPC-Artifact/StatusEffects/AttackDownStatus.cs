using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDownStatus : StatusEffect {
	public AttackDownStatus(int duration) : base(duration) {
		id = "AttackDown";
	}

	public override void OnApply(EntityStats owner) {
		base.OnApply(owner);
		owner.DamageDealtMultiplier -= 0.2f;
		if (owner.HasStatusEffect("AttackUp")) {
			owner.RemoveStatusEffect(owner.GetStatusEffect("AttackUp"));
			owner.RemoveStatusEffect(this);
		}
	}

	public override void OnRemove() {
		owner.DamageDealtMultiplier += 0.2f;
	}
}
