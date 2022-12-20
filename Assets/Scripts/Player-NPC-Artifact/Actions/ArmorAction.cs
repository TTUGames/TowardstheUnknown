using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorAction : Action
{
	private EntityStats target;
	private int amount;
    
	public ArmorAction(EntityStats target, int amount) {
		this.target = target;
		this.amount = amount;
	}

	public override void Apply() {
		target.GainArmor(amount);
		isDone = true;
	}
}
