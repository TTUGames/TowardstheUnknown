using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains stats and methods common to all combat entities (player + enemies)
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    [Space]

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] public int currentHealth;
    [SerializeField] protected int armor;
    [SerializeField] protected float damageDealtMultiplier = 1f;
    [SerializeField] protected float damageReceivedMultiplier = 1f;
    
    [Space]
    
    public EntityType type;

    protected Dictionary<string, StatusEffect> statusEffects = new Dictionary<string, StatusEffect>();
    private List<string> toRemoveStatusEffects = new List<string>();
    
	public virtual void Start() {
        canvas = FindObjectOfType<MainUICanvas>().GetComponent<Canvas>();

        currentHealth = maxHealth;
	}

    public void CreateHealthIndicator() {
        if (canvas == null)
        {
            Debug.LogError("Canvas is null into EntityStats (type: " + type + ")");
            return;
        }
        
        HealthIndicator prefab = Resources.Load<HealthIndicator>("Prefabs/UI/InGameDisplay/HealthIndicator");
        HealthIndicator healthIndicator = Instantiate(prefab, canvas.transform);
        healthIndicator.entityGameObject = gameObject;
        healthIndicator.entityStats = this;
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

    public virtual void OnCombatEnd() {
        armor = 0;
        foreach (StatusEffect statusEffect in statusEffects.Values) {
            QueueStatusEffectForRemoval(statusEffect);
        }
        RemoveQueuedStatusEffects();
    }

    /// <summary>
    /// Uses one movement point. Implementation depends on which resource is used by the entity
    /// </summary>
    public abstract void UseMovement(int distance);

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
        DamageIndicator.DisplayDamage(amount, transform);
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
        if (currentHealth <= 0)
            Die();
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
        ActionManager.AddToBottom(new DieAction(this));
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
    public int MaxHealth { get { return maxHealth; } }
    public int CurrentHealth { get { return currentHealth; } }
    public int Armor { get { return armor; } }
}
