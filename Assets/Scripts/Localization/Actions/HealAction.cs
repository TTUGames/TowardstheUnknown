using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAction : Action
{
	private EntityStats target;
	private int amount;
	public HealAction(EntityStats target, int amount){
		this.target = target;
		this.amount = amount;
	}

	public override void Apply() {
		target.Heal(amount);
		isDone = true;
	}
}
