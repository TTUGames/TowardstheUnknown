using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnPoint : MonoBehaviour {
	[SerializeField] private ArtifactPool artifactPool;
	[SerializeField] private Collectable collectablePrefab;

	public void Spawn() {
		Spawn(artifactPool.GetRandomElement());
	}

	public void Spawn(List<Artifact> artifacts) {
		if (artifacts.Count == 0) return;
		Collectable collectable = Instantiate(collectablePrefab);
		collectable.SetArtifacts(artifacts);
		collectable.transform.SetParent(GetComponentInParent<Room>().transform);
		collectable.transform.position = transform.position;
	}
}
