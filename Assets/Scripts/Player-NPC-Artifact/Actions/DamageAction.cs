using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : Action{
	EntityStats source;
	EntityStats target;
	private int minAmount;
	private int maxAmount;
	public DamageAction(EntityStats source, EntityStats target, int minAmount, int maxAmount){
		this.source = source;
		this.target = target;
		this.minAmount = minAmount;
		this.maxAmount = maxAmount;
	}

	public override void Apply() {
		target.TakeDamage(Mathf.CeilToInt(Random.Range(minAmount, maxAmount+1) * source.DamageDealtMultiplier * target.DamageReceivedMultiplier));
		isDone = true;
	}
}
