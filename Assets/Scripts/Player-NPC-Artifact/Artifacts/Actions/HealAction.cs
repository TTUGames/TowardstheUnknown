using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAction : IAction
{
	private int amount;
	public HealAction(int amount) {
		this.amount = amount;
	}

	public void Use(EntityStats source, EntityStats target) {
		target.Heal(amount);
	}
}
