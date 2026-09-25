using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains stats and methods common to all combat entities (player + enemies)
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [SerializeField, Sirenix.OdinInspector.Required, Tooltip("The kind of entity: its name, icon, score and sounds")] private EntityData data;
    [Space]

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int armor;
    [SerializeField, Tooltip("Before the status effects")] protected float damageDealtMultiplier = 1f;
    [SerializeField, Tooltip("Before the status effects")] protected float damageReceivedMultiplier = 1f;

    [Space]

    public EntityType type;

    private readonly Dictionary<StatusEffectData, StatusEffect> statusEffects = new Dictionary<StatusEffectData, StatusEffect>();


    /// <summary>
    /// Fired when the health, armor, damage multipliers or status effects change
    /// </summary>
    public event System.Action StatsChanged;

    protected void NotifyStatsChanged() => StatsChanged?.Invoke();

    /// <summary>
    /// Fired when any entity takes damage, with the damage before armor and the health lost: 0 if its armor took it all
    /// </summary>
    public static event System.Action<EntityStats, int, int> AnyDamageTaken;

    /// <summary>
    /// Fired when any entity heals, with the health gained
    /// </summary>
    public static event System.Action<EntityStats, int> AnyHealed;

    /// <summary>
    /// Fired when any entity gains armor
    /// </summary>
    public static event System.Action<EntityStats, int> AnyArmorGained;

    /// <summary>
    /// Fired when a status effect is applied on any entity, even when it cancels the opposite one
    /// </summary>
    public static event System.Action<EntityStats, StatusEffectData> AnyStatusApplied;

    /// <summary>
    /// Fired when the entity takes damage, with the health lost: 0 if its armor took it all
    /// </summary>
    public event System.Action<int> Hit;

    /// <summary>
    /// Fired when the entity dies, before it leaves the combat
    /// </summary>
    public event System.Action Died;

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
    /// <param name="ignoreArmor">The damage goes straight to the health, the armor is kept</param>
    public void TakeDamage(int amount, bool ignoreArmor = false)
    {
        if (currentHealth <= 0) return;

        int remainingDamage = ignoreArmor ? amount : Mathf.Max(0, amount - armor);
        if (!ignoreArmor) armor = Mathf.Max(0, armor - amount);

        Hit?.Invoke(remainingDamage);
        AnyDamageTaken?.Invoke(this, amount, remainingDamage);
        currentHealth = Mathf.Max(0, currentHealth - remainingDamage);
        OnDamageTaken(amount);
        NotifyStatsChanged();
        if (currentHealth <= 0)
        {
            IsDead = true;
            Died?.Invoke();
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
        AnyArmorGained?.Invoke(this, amount);
    }

    /// <summary>
    /// Heals the entity, increasing his current HP
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        int healed = Mathf.Min(currentHealth + amount, maxHealth) - currentHealth;
        currentHealth += healed;
        NotifyStatsChanged();
        AnyHealed?.Invoke(this, healed);
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
        AnyStatusApplied?.Invoke(this, status);
    }

    public IEnumerable<StatusEffect> StatusEffects => statusEffects.Values;

    private float StatusModifier(StatusEffectData.Stat stat) => statusEffects.Keys.Where(status => status.stat == stat).Sum(status => status.delta);

    public EntityData Data => data;

    /// <summary>
    /// Identifies the entity in the localization and the run stats
    /// </summary>
    public string ID => data.ID;

    //Properties
    public float DamageDealtMultiplier => damageDealtMultiplier + StatusModifier(StatusEffectData.Stat.DamageDealt);
    public float DamageReceivedMultiplier => damageReceivedMultiplier + StatusModifier(StatusEffectData.Stat.DamageReceived);
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }
    public int Armor => armor;
    public Sprite TimelineIcon => data.timelineIcon;
}
