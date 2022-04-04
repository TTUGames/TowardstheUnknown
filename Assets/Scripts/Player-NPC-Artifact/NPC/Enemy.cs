using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health;

    /// <summary>
    /// Launch the turn
    /// </summary>
    public void LaunchTurn()
    {
        Debug.Log("Entity : " + transform.name + " | Started his turn");
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public void StopTurn()
    {
        Debug.Log("Entity : " + transform.name + " | Ended his turn");
    }

    public void LowerHealth(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Debug.Log("DEAD");
            transform.gameObject.SetActive(false);
        }
    }
}
