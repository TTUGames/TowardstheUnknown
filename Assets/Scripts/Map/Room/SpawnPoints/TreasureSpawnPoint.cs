using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawnPoint : MonoBehaviour, SpawnPoint {
	public Collectable collectablePrefab;

	public void Spawn() {
		Collectable collectable = Instantiate<Collectable>(collectablePrefab);
		collectable.transform.SetParent(GetComponentInParent<Room>().transform);
		collectable.transform.position = transform.position;
	}
}
