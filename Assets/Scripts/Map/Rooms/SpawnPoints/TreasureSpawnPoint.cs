using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnPoint : MonoBehaviour {
	[SerializeField] private ArtifactPool artifactPool;
	[SerializeField] private Collectable collectablePrefab;

	/// <param name="popIn">The collectable grows out of its tile rather than being there at once</param>
	public void Spawn(bool popIn = false) {
		Spawn(artifactPool.GetRandomElement(), popIn);
	}

	public void Spawn(List<Artifact> artifacts, bool popIn = false) {
		if (artifacts.Count == 0) return;
		Collectable collectable = Instantiate(collectablePrefab);
		collectable.SetArtifacts(artifacts);
		collectable.transform.SetParent(GetComponentInParent<Room>().transform);
		collectable.transform.position = transform.position;
		if (popIn) collectable.PopIn();
	}
}
