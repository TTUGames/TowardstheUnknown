using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnPoint : MonoBehaviour, SpawnPoint {
	[SerializeField] private ArtifactPool artifactPool;

	public void Spawn() {
		Collectable collectable = Collectable.InstantiateCollectable(artifactPool.GetRandomElement());
		collectable.transform.SetParent(GetComponentInParent<Room>().transform);
		collectable.transform.position = transform.position;
	}
}
