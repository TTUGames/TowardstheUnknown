using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    /// <summary>
    /// Launch the turn
    /// </summary>
    public void LaunchTurn()
    {
        Debug.Log("Entity : " + transform.name + " | Start his turn");
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public void StopTurn()
    {
        Debug.Log("Entity : " + transform.name + " | End his turn");
    }
}
