using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public EntityTurn enemyPrefab;


	public EntityTurn SpawnEntity() {
		EntityTurn enemy = Instantiate<EntityTurn>(enemyPrefab);
		enemy.transform.position = transform.position;
		return enemy;
	}
}
