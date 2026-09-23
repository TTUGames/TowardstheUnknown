using UnityEngine;

public class TreasureSpawnLayout : MonoBehaviour, SpawnLayout {
	public void Spawn() {
		foreach (TreasureSpawnPoint spawnPoint in GetComponentsInChildren<TreasureSpawnPoint>()) {
			spawnPoint.Spawn();
		}
	}
}
