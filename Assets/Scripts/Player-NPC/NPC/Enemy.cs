using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public void LaunchTurn()
    {
        Debug.Log("Entity : " + transform.name + " | Start his turn");
    }

    public void StopTurn()
    {
        Debug.Log("Entity : " + transform.name + " | End his turn");
    }
}
