using UnityEngine;

/// <summary>
/// Where a combat room lays its reward (RewardSpawnPoint.prefab, on CombatRoom.prefab), drawn from its pool
/// </summary>
public class TreasureSpawnPoint : MonoBehaviour {
	[SerializeField] private ArtifactPool artifactPool;
	[SerializeField] private Collectable collectablePrefab;

	/// <param name="popIn">The collectable grows out of its tile rather than being there at once</param>
	public void Spawn(bool popIn = false) {
		Collectable.Spawn(collectablePrefab, artifactPool.GetRandomElement(), GetComponentInParent<Room>(), transform.position, popIn);
	}
}
