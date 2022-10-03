using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract class defining an entity's turn start and end.
/// </summary>
public abstract class EntityTurn : MonoBehaviour
{
    private TurnSystem turnSystem;
    public bool isFirstToPlay = false;

    /// <summary>
    /// Initializes turnSystem and adds this to its queue
    /// </summary>
    void Start()
    {
        turnSystem = FindObjectOfType<TurnSystem>();
        if (isFirstToPlay) turnSystem.AddToStart(this);
        else turnSystem.AddToEnd(this);
        Init();
    }

    public void RemoveFromTurnSystem() {
        turnSystem.Remove(this);
	}

    public void StopTurn() {
        turnSystem.GoToNextTurn();
	}

    /// <summary>
    /// Initializes the turn's specific values. Use this instead of Start.
    /// </summary>
    protected virtual void Init() { }

    /// <summary>
    /// Called when the turn begins
    /// </summary>
    public virtual void OnTurnLaunch() { }

    /// <summary>
    /// Called when the turn ends
    /// </summary>
    public virtual void OnTurnStop() { }
}
