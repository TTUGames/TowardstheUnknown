using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCombatStarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TurnSystem turnSystem = FindObjectOfType<TurnSystem>();

        turnSystem.RegisterPlayer(FindObjectOfType<PlayerTurn>());

        foreach(EnemyAI enemy in FindObjectsOfType<EnemyAI>()) {
            turnSystem.RegisterEnemy(enemy);
		}
        turnSystem.CheckForCombatStart();
    }
}
