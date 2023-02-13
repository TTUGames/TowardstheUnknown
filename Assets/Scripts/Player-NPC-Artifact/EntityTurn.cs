using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract class defining an entity's turn start and end.
/// </summary>
public abstract class EntityTurn : MonoBehaviour
{
    [HideInInspector] public EntityStats stats;
    protected TurnSystem turnSystem;

	private void Awake() {
        stats = GetComponent<EntityStats>();
        turnSystem = FindObjectOfType<TurnSystem>();
        Init();
	}

    protected virtual void Init() {}

	public void RemoveFromTurnSystem() {
        turnSystem.Remove(this);
	}


    /// <summary>
    /// Called when the turn begins
    /// </summary>
    public virtual void OnTurnLaunch() {
        stats.OnTurnLaunch();
    }

    /// <summary>
    /// Called when the turn ends
    /// </summary>
    public virtual void OnTurnStop() {
        stats.OnTurnStop();
    }

    /// <summary>
    /// Called when the comabt ends
    /// </summary>
    public virtual void OnCombatEnd() {
        stats.OnCombatEnd();
	}

    /// <summary>
    /// Calls every frame when the turn is active
    /// </summary>
    public abstract void TurnUpdate();
}
