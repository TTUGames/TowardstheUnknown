using UnityEngine;

public class DebugCombatStarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TurnSystem turnSystem = TurnSystem.Instance;

        turnSystem.RegisterPlayer(FindAnyObjectByType<PlayerTurn>());

        foreach(EnemyAI enemy in FindObjectsByType<EnemyAI>()) {
            turnSystem.RegisterEnemy(enemy);
		}
        turnSystem.CheckForCombatStart();
    }
}
