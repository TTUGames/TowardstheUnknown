using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetting : AbstractTargetting {
	public PlayerTargetting(int distance) : base(distance) { }

	public override EntityStats GetTarget(EntityStats stats) {
		return GameObject.FindObjectOfType<PlayerStats>();
	}
}
