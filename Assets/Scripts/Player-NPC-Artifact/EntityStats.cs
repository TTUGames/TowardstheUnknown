using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains stats and methods common to all combat entities (player + enemies)
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [SerializeField] protected int maxHealth;
    protected int currentHealth;
    protected int armor;
    [SerializeField] protected float damageDealtMultiplier;
    [SerializeField] protected float damageReceivedMultiplier;

	private void Start() {
        currentHealth = maxHealth;
	}

    /// <summary>
    /// Called on the entity's start of turn
    /// </summary>
	public virtual void OnTurnLaunch() {
        armor = 0;
	}

    /// <summary>
    /// Called on the entity's end of turn
    /// </summary>
	public virtual void OnTurnStop() {
        return;
	}

    /// <summary>
    /// Uses one movement point. Implementation depends on which resource is used by the entity
    /// </summary>
    public abstract void UseMovement();

    /// <summary>
    /// Get the max distance the entity can move. Implementation depends on which resource is used by the entity
    /// </summary>
    /// <returns></returns>
    public abstract int GetMovementDistance();
}
