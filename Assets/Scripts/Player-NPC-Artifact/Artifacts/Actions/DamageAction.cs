using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : IAction {
	private int minAmount;
	private int maxAmount;
	public DamageAction(int minAmount, int maxAmount) {
		this.minAmount = minAmount;
		this.maxAmount = maxAmount;
	}

	public void Use(EntityStats source, EntityStats target) {
		target.TakeDamage(Mathf.CeilToInt(Random.Range(minAmount, maxAmount+1) * source.DamageDealtMultiplier * target.DamageReceivedMultiplier));
	}
}
