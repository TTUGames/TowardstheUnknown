using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnPoint : MonoBehaviour, SpawnPoint {
	[SerializeField] private ArtifactPool artifactPool;

	public void Spawn() {
		Spawn(artifactPool.GetRandomElement());
	}

	public void Spawn(List<Artifact> artifacts) {
		if (artifacts.Count == 0) return;
		Collectable collectable = Collectable.InstantiateCollectable(artifacts);
		collectable.transform.SetParent(GetComponentInParent<Room>().transform);
		collectable.transform.position = transform.position;
	}
}
