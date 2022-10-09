using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : IAction {
	private int amount;
	public DamageAction(int amount) {
		this.amount = amount;
	}

	public void Use(EntityStats source, EntityStats target) {
		target.TakeDamage(Mathf.CeilToInt(amount * source.DamageDealtMultiplier * target.DamageReceivedMultiplier));
	}
}
