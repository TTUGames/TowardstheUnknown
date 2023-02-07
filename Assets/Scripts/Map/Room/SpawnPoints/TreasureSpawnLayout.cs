using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnLayout : MonoBehaviour, SpawnLayout {
	[SerializeField] private bool isRoomReward = false;
	public bool IsRoomReward() {
		return isRoomReward;
	}

	public void Spawn() {
		foreach (TreasureSpawnPoint spawnPoint in GetComponentsInChildren<TreasureSpawnPoint>()) {
			spawnPoint.Spawn();
		}
	}
}
