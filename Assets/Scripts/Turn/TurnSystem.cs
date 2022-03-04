using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    [SerializeField]private Player  player;
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
        if (turnNumber == 0 && !isPlaying)
        {
            player.LaunchTurn();
            isPlaying = true;
        }
    }

    public void StopTurn()
    {
        if(turnNumber == 0)
        {
            player.StopTurn();
        }
        isPlaying = false;
        turnNumber++;
        turnNumber--;  //DEBUG
    }
}
