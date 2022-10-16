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
    protected Dictionary<string, StatusEffect> statusEffects = new Dictionary<string, StatusEffect>();
    private List<string> toRemoveStatusEffects = new List<string>();

	private void Start() {
        currentHealth = maxHealth;
	}

    /// <summary>
    /// Called on the entity's start of turn
    /// </summary>
	public virtual void OnTurnLaunch() {
        armor = 0;
        foreach (StatusEffect status in statusEffects.Values) status.OnTurnStart();
        RemoveQueuedStatusEffects();
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

    /// <summary>
    /// Deals damage to the entity, losing armor if possible then HP, and killing it if it has no HP
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(int amount) {
        int remainingDamage = amount;
        if (armor > 0) {
            if (armor >= remainingDamage) {
                armor -= remainingDamage;
                remainingDamage = 0;
			}
            else {
                remainingDamage -= armor;
                armor = 0;
			}
		}
        currentHealth -= remainingDamage;
        if (currentHealth <= 0) Die();
	}

    /// <summary>
    /// Grants armor to the entity
    /// </summary>
    /// <param name="amount"></param>
    public void GainArmor(int amount) {
        armor += amount;
	}

    /// <summary>
    /// Heals the entity, increasing his current HP
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount) {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
	}

    /// <summary>
    /// Called when the entity dies, and removes it from combat.
    /// </summary>
    protected virtual void Die() {
        GetComponent<EntityTurn>().RemoveFromTurnSystem();
        Destroy(gameObject);
	}

    /// <summary>
    /// Applies a status effect to the entity
    /// </summary>
    /// <param name="effect"></param>
    public void AddStatusEffect(StatusEffect effect) {
        if (HasStatusEffect(effect.ID)) {
            if (GetStatusEffect(effect.ID).Duration >= effect.Duration) return;
            else GetStatusEffect(effect.ID).Duration = effect.Duration;
        }
        else {
            statusEffects.Add(effect.ID, effect);
            effect.OnApply(this);
		}
	}

    /// <summary>
    /// Removes a status effect from the entity if present
    /// </summary>
    /// <param name="effect"></param>
    public void RemoveStatusEffect(StatusEffect effect) {
        if (!HasStatusEffect(effect.ID)) return;
        effect.OnRemove();
        statusEffects.Remove(effect.ID);
	}

    public bool HasStatusEffect(string id) {
        return statusEffects.ContainsKey(id);
	}

    public StatusEffect GetStatusEffect(string id) {
        return statusEffects[id];
	}

    /// <summary>
    /// Registers a status effect to be removed by RemoveQueuedStatusEffects.
    /// Use this in a foreach loop instead of RemoveStatusEffect to prevent loop modifications while iterating
    /// </summary>
    /// <param name="status"></param>
    public void QueueStatusEffectForRemoval(StatusEffect status) {
        toRemoveStatusEffects.Add(status.ID);
    }

    /// <summary>
    /// Removes all status effects registered by QueueStatusEffectForRemoval
    /// </summary>
    private void RemoveQueuedStatusEffects() {
        foreach (string id in toRemoveStatusEffects) RemoveStatusEffect(GetStatusEffect(id));
        toRemoveStatusEffects.Clear();
    }

    //Properties
    public float DamageDealtMultiplier { get => damageDealtMultiplier; set => damageDealtMultiplier = value; }
    public float DamageReceivedMultiplier { get => damageReceivedMultiplier; set => damageReceivedMultiplier = value; }
}
