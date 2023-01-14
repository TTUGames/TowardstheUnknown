using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnPoint : MonoBehaviour, SpawnPoint {
	private Collectable collectablePrefab;
	[SerializeField] private ArtifactPool artifactPool;

	private void Awake() {
		collectablePrefab = Resources.Load<Collectable>("Prefabs/Collectables/Collectable");
	}

	public void Spawn() {
		Collectable collectable = Instantiate<Collectable>(collectablePrefab);
		collectable.artifactName = artifactPool.GetRandomArtifact();
		collectable.transform.SetParent(GetComponentInParent<Room>().transform);
		collectable.transform.position = transform.position;
	}
}
