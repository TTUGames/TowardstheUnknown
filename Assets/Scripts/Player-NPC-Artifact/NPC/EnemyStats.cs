using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : EntityStats {
	[SerializeField] private int maxMovementPoints = 3;
	private int movementPoints;
	
	public override void Start()
	{
		base.Start();

		CreateHealthIndicator();
	}
	
	public override int GetMovementDistance() {
		return movementPoints;
	}

	public override void UseMovement(int distance) {
		movementPoints -= distance;
	}

	public override void OnTurnLaunch() {
		base.OnTurnLaunch();
		movementPoints = maxMovementPoints;
	}
}
