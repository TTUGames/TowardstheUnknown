using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetting : AbstractTargetting {
	public override EntityStats GetTarget() {
		return GameObject.FindObjectOfType<PlayerStats>();
	}
}
