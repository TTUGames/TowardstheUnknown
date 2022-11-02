using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract class defining an entity's turn start and end.
/// </summary>
public abstract class EntityTurn : MonoBehaviour
{
    protected TurnSystem turnSystem;
    public bool isFirstToPlay = false;

    public void RemoveFromTurnSystem() {
        turnSystem.Remove(this);
	}


    /// <summary>
    /// Called when the turn begins
    /// </summary>
    public virtual void OnTurnLaunch() { }

    /// <summary>
    /// Called when the turn ends
    /// </summary>
    public virtual void OnTurnStop() { }
}
