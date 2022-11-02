using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health;

    /// <summary>
    /// Lower the health
    /// </summary>
    /// <param name="damage">How much health must be removed</param>
    public void LowerHealth(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Debug.Log("DEAD");
            GetComponent<EnemyTurn>().RemoveFromTurnSystem();
            transform.gameObject.SetActive(false);
        }
    }
}
