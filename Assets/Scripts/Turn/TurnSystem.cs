using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    private Player  player;
    private Enemy[] aEnemy; //Need to search all tagged "Enemy" in scene
    private int     turnNumber = 0;
    private bool    isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        aEnemy = new Enemy[GameObject.FindGameObjectsWithTag("Enemy").Length];

        int i = 0;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
            aEnemy[i++] = obj.GetComponent<Enemy>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlaying)
        {
            if (turnNumber == 0)
                player.LaunchTurn();
            else if (turnNumber > 0)
                aEnemy[turnNumber - 1].LaunchTurn();

            isPlaying = true;
        }
    }

    /// <summary>
    /// Stop the current turn
    /// </summary>
    public void StopTurn()
    {
        if(!player.GetIsMoving())
        {
            if (turnNumber == 0)
                player.StopTurn();
            else if (turnNumber > 0)
                aEnemy[turnNumber - 1].StopTurn();

            isPlaying = false;

            turnNumber++;
            if (turnNumber > aEnemy.Length)
                turnNumber = 0;
        }
    }
}
