using UnityEngine;

public class EnemySpawnLayout : MonoBehaviour, SpawnLayout {
	public int difficulty = 1;

	public void Spawn() {
		foreach (EnemySpawnPoint spawnPoint in GetComponentsInChildren<EnemySpawnPoint>()) {
			spawnPoint.Spawn();
		}
	}
}
