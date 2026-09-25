using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains stats and methods common to all combat entities (player + enemies)
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [SerializeField, Tooltip("Icon of the entity in the turn timeline")] private Sprite timelineIcon;
    [SerializeField] private float hitVFXHeight;
    [SerializeField] private Animator animator;

    [Space]

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int armor;
    [SerializeField, Tooltip("Before the status effects")] protected float damageDealtMultiplier = 1f;
    [SerializeField, Tooltip("Before the status effects")] protected float damageReceivedMultiplier = 1f;

    [Space]

    public EntityType type;
    public int entityKilledScore = 1;

    private readonly Dictionary<StatusEffectData, StatusEffect> statusEffects = new Dictionary<StatusEffectData, StatusEffect>();


    /// <summary>
    /// Fired when the health, armor, damage multipliers or status effects change
    /// </summary>
    public event System.Action StatsChanged;

    protected void NotifyStatsChanged() => StatsChanged?.Invoke();

    /// <summary>
    /// Fired when any entity takes damage, with the damage before armor
    /// </summary>
    public static event System.Action<EntityStats, int> AnyDamageTaken;

    /// <summary>
    /// Fired when the entity loses health, with the health lost
    /// </summary>
    public event System.Action<int> HealthLost;

    public virtual void Start()
    {
        currentHealth = maxHealth;
        NotifyStatsChanged();
    }

    /// <summary>
    /// Called on the entity's start of turn
    /// </summary>
	public virtual void OnTurnLaunch()
    {
        armor = 0;
        foreach (StatusEffect status in statusEffects.Values.ToList())
            if (--status.Duration <= 0) statusEffects.Remove(status.Data);
        NotifyStatsChanged();
    }

    /// <summary>
    /// Called on the entity's end of turn
    /// </summary>
	public virtual void OnTurnStop() { }

    public virtual void OnCombatEnd()
    {
        armor = 0;
        statusEffects.Clear();
        NotifyStatsChanged();
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

        if (remainingDamage > 0)
            HealthLost?.Invoke(remainingDamage);

        if (animator != null)
        {
            animator.SetTrigger("isTakingDamage");
            animator.SetInteger("DamageValue", remainingDamage);
        }

        AnyDamageTaken?.Invoke(this, amount);
        currentHealth = Mathf.Max(0, currentHealth - remainingDamage);
        OnDamageTaken(amount);
        NotifyStatsChanged();
        if (currentHealth <= 0)
        {
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
        NotifyStatsChanged();
    }

    /// <summary>
    /// Heals the entity, increasing his current HP
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        NotifyStatsChanged();
    }

    /// <summary>
    /// Called when the entity dies, and removes it from combat.
    /// </summary>
    protected virtual void Die()
    {
        GameEvents.Die(this);
        GetComponent<TacticsMove>().CurrentTile.SetEntity(null);
        GetComponent<EntityTurn>().RemoveFromTurnSystem();
        ActionManager.AddToBottom(new DieAction(this));
    }

    /// <summary>
    /// Applies a status effect for some turns, or extends it. Cancels the opposite status instead if the entity has it
    /// </summary>
    public void AddStatusEffect(StatusEffectData status, int duration)
    {
        if (status.opposite != null && statusEffects.ContainsKey(status.opposite))
            statusEffects.Remove(status.opposite);
        else if (statusEffects.TryGetValue(status, out StatusEffect current))
            current.Duration = Mathf.Max(current.Duration, duration);
        else
            statusEffects.Add(status, new StatusEffect(status, duration));
        NotifyStatsChanged();
    }

    public IEnumerable<StatusEffect> StatusEffects => statusEffects.Values;

    private float StatusModifier(StatusEffectData.Stat stat) => statusEffects.Keys.Where(status => status.stat == stat).Sum(status => status.delta);

    /// <summary>
    /// Add a VFX on hit
    /// </summary>
    private void VFXonHit()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = hitVFXHeight;
        Destroy(Instantiate(GameAssets.Instance.hit, spawnPosition, Quaternion.identity), 0.5f);
    }

    /// <summary>
    /// Identifies the entity in the localization and the run stats: the name of its prefab
    /// </summary>
    public string ID => name.Replace("(Clone)", "").Trim();

    //Properties
    public float DamageDealtMultiplier => damageDealtMultiplier + StatusModifier(StatusEffectData.Stat.DamageDealt);
    public float DamageReceivedMultiplier => damageReceivedMultiplier + StatusModifier(StatusEffectData.Stat.DamageReceived);
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int Armor => armor;
    public Sprite TimelineIcon => timelineIcon;
}
