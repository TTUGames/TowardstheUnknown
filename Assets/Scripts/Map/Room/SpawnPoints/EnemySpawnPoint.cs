using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour, SpawnPoint
{
    public EntityTurn enemyPrefab;


	public void Spawn() {
		EntityTurn enemy = Instantiate<EntityTurn>(enemyPrefab);
		enemy.transform.SetParent(GetComponentInParent<Room>().transform);
		enemy.transform.position = transform.position;
		FindObjectOfType<TurnSystem>().RegisterEnemy(enemy);
	}
}
