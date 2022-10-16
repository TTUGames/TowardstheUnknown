using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseUpStatus : StatusEffect {
	public DefenseUpStatus(int duration) : base(duration) {
		id = "DefenseUp";
	}

	public override void OnApply(EntityStats owner) {
		base.OnApply(owner);
		owner.DamageReceivedMultiplier += 0.2f;
		if (owner.HasStatusEffect("DefenseDown")) {
			owner.RemoveStatusEffect(owner.GetStatusEffect("DefenseDown"));
			owner.RemoveStatusEffect(this);
		}
	}

	public override void OnRemove() {
		owner.DamageReceivedMultiplier -= 0.2f;
	}
}
