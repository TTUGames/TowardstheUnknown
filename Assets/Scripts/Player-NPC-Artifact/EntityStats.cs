using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains stats and methods common to all combat entities (player + enemies)
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;
    protected int armor;
    [SerializeField] protected float damageDealtMultiplier = 1f;
    [SerializeField] protected float damageReceivedMultiplier = 1f;

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

    public void TakeDamage(int amount) {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
	}

    /// <summary>
    /// Called when the entity dies, and removes it from combat.
    /// </summary>
    protected virtual void Die() {
        GetComponent<EntityTurn>().RemoveFromTurnSystem();
        Destroy(gameObject);
	}

    //Properties
    public float DamageDealtMultiplier { get => damageDealtMultiplier; set => damageDealtMultiplier = value; }
    public float DamageReceivedMultiplier { get => damageReceivedMultiplier; set => damageReceivedMultiplier = value; }
}
