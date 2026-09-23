using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains stats and methods common to all combat entities (player + enemies)
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    private static GameObject hitVFXPrefab;
    [SerializeField] private float hitVFXHeight;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator camAnimator;

    [Space]

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] public int currentHealth;
    [SerializeField] protected int armor;
    [SerializeField] protected float damageDealtMultiplier = 1f;
    [SerializeField] protected float damageReceivedMultiplier = 1f;

    [Space]

    public EntityType type;
    public int entityKilledScore = 1;

    protected Dictionary<string, StatusEffect> statusEffects = new Dictionary<string, StatusEffect>();
    private List<string> toRemoveStatusEffects = new List<string>();

    protected PlayerInfo playerInfo;

    public virtual void Start()
    {
        currentHealth = maxHealth;
        playerInfo = FindAnyObjectByType<PlayerInfo>(FindObjectsInactive.Include);
        if (hitVFXPrefab == null) hitVFXPrefab = Resources.Load<GameObject>("VFX/HitEntity");
    }

    /// <summary>
    /// Called on the entity's start of turn
    /// </summary>
	public virtual void OnTurnLaunch()
    {
        armor = 0;
        foreach (StatusEffect status in statusEffects.Values) status.OnTurnStart();
        RemoveQueuedStatusEffects();
    }

    /// <summary>
    /// Called on the entity's end of turn
    /// </summary>
	public virtual void OnTurnStop() { }

    public virtual void OnCombatEnd()
    {
        armor = 0;
        foreach (StatusEffect statusEffect in statusEffects.Values)
            QueueStatusEffectForRemoval(statusEffect);
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
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        VFXonHit();

        int remainingDamage = Mathf.Max(0, amount - armor);
        armor = Mathf.Max(0, armor - amount);

        //ScreenShake
        if (camAnimator != null && remainingDamage > 0)
            camAnimator.SetTrigger("isTakingDamage");

        if (animator != null)
        {
            animator.SetTrigger("isTakingDamage");
            animator.SetInteger("DamageValue", remainingDamage);
        }

        DamageIndicator.DisplayDamage(amount, transform);
        currentHealth -= remainingDamage;
        OnDamageTaken(amount);
        if (currentHealth <= 0)
        {
            playerInfo.score += entityKilledScore;
            if (animator != null) animator.SetTrigger("isDying");
            Die();
        }
    }

    protected virtual void OnDamageTaken(int amount) { }

    /// <summary>
    /// Grants armor to the entity
    /// </summary>
    /// <param name="amount"></param>
    public void GainArmor(int amount)
    {
        armor += amount;
    }

    /// <summary>
    /// Heals the entity, increasing his current HP
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    /// <summary>
    /// Called when the entity dies, and removes it from combat.
    /// </summary>
    protected virtual void Die()
    {
        GetComponent<TacticsMove>().CurrentTile.SetEntity(null);
        GetComponent<EntityTurn>().RemoveFromTurnSystem();
        ActionManager.AddToBottom(new DieAction(this));
    }

    /// <summary>
    /// Applies a status effect to the entity
    /// </summary>
    /// <param name="effect"></param>
    public virtual void AddStatusEffect(StatusEffect effect)
    {
        if (statusEffects.TryGetValue(effect.ID, out StatusEffect current))
        {
            if (current.Duration < effect.Duration) current.Duration = effect.Duration;
        }
        else
        {
            statusEffects.Add(effect.ID, effect);
            effect.OnApply(this);
        }
    }

    /// <summary>
    /// Removes a status effect from the entity if present
    /// </summary>
    /// <param name="effect"></param>
    public void RemoveStatusEffect(StatusEffect effect)
    {
        if (!HasStatusEffect(effect.ID)) return;
        effect.OnRemove();
        statusEffects.Remove(effect.ID);
    }

    public bool HasStatusEffect(string id)
    {
        return statusEffects.ContainsKey(id);
    }

    public StatusEffect GetStatusEffect(string id)
    {
        return statusEffects[id];
    }

    /// <summary>
    /// Registers a status effect to be removed by RemoveQueuedStatusEffects.
    /// Use this in a foreach loop instead of RemoveStatusEffect to prevent loop modifications while iterating
    /// </summary>
    /// <param name="status"></param>
    public void QueueStatusEffectForRemoval(StatusEffect status)
    {
        toRemoveStatusEffects.Add(status.ID);
    }

    /// <summary>
    /// Removes all status effects registered by QueueStatusEffectForRemoval
    /// </summary>
    private void RemoveQueuedStatusEffects()
    {
        foreach (string id in toRemoveStatusEffects)
            if (statusEffects.TryGetValue(id, out StatusEffect status)) RemoveStatusEffect(status);
        toRemoveStatusEffects.Clear();
    }

    /// <summary>
    /// Add a VFX on hit
    /// </summary>
    private void VFXonHit()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = hitVFXHeight;
        Destroy(Instantiate(hitVFXPrefab, spawnPosition, Quaternion.identity), 0.5f);
    }

    //Properties
    public float DamageDealtMultiplier { get => damageDealtMultiplier; set => damageDealtMultiplier = value; }
    public float DamageReceivedMultiplier { get => damageReceivedMultiplier; set => damageReceivedMultiplier = value; }
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int Armor => armor;
}
