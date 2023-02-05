using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnLayout : MonoBehaviour, SpawnLayout {
	public int difficulty = 1;

	public bool IsRoomReward() {
		return false;
	}

	public void Spawn() {
		foreach (EnemySpawnPoint spawnPoint in GetComponentsInChildren<EnemySpawnPoint>()) {
			spawnPoint.Spawn();
		}
	}
}
