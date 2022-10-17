using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorAction : IAction
{
	private int amount;
    public ArmorAction(int amount) {
		this.amount = amount;
	}

	public void Use(EntityStats source, EntityStats target) {
		target.GainArmor(amount);
	}
}
