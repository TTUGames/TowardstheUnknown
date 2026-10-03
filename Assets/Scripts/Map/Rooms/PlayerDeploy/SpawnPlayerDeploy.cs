using System.Collections;
using UnityEngine;

/// <summary>
/// The spawn room's deploy: a run starts on the layout's first deploy cell, and coming back from another room, beside its entrance
/// </summary>
public class SpawnPlayerDeploy : PlayerDeploy
{
	public override IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
		if (fromDirection == Direction.NULL) {
			if (DeployTiles.Count == 0) throw new System.Exception(room.name + " has no deploy cell to start the run on");
			MovePlayerToTile(player, DeployTiles[0]);
		}
		else DefaultDeploy(player, fromDirection);
		yield return null;
	}
}
