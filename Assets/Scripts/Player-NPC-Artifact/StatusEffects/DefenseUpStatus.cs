using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseDownStatus : StatusEffect {
	public DefenseDownStatus(int duration) : base(duration) {
		id = "DefenseDown";
	}

	public override void OnApply(EntityStats owner) {
		base.OnApply(owner);
		owner.DamageReceivedMultiplier -= 0.2f;
		if (owner.HasStatusEffect("DefenseUp")) {
			owner.RemoveStatusEffect(owner.GetStatusEffect("DefenseUp"));
			owner.RemoveStatusEffect(this);
		}
	}

	public override void OnRemove() {
		owner.DamageReceivedMultiplier += 0.2f;
	}
}
