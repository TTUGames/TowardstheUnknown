using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayerDeploy : PlayerDeploy
{
    [SerializeField] Tile spawnTile; //Set in inspector

	public override IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
		if (fromDirection == Direction.NULL)
			MovePlayerToTile(player, spawnTile);
		else {
			DefaultDeploy(player, fromDirection);
		}
		NextTurnButton.instance.EnterState(NextTurnButton.State.EXPLORATION);
		yield return null;
	}
}
