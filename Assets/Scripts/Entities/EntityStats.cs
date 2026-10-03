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
    [SerializeField, Tooltip("Never falls under 1 health and heals back to full at the start of its turns: the training dummy")] private bool immortal;

    [Space]

    public EntityType type;

    private readonly Dictionary<StatusEffectData, StatusEffect> statusEffects = new Dictionary<StatusEffectData, StatusEffect>();
    private TacticsMove move;

    /// <summary>
    /// The entity's movement, on the same object
    /// </summary>
    public TacticsMove Move => move != null ? move : move = GetComponent<TacticsMove>();

    /// <summary>
    /// The tile the entity stands on
    /// </summary>
    public Tile Tile => Move.CurrentTile;


    /// <summary>
    /// Fired when the health, armor, damage multipliers or status effects change
    /// </summary>
    public event System.Action StatsChanged;

    protected void NotifyStatsChanged() => StatsChanged?.Invoke();

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
        if (immortal) currentHealth = maxHealth;
        CountDownStatuses(selfApplied: true);
        NotifyStatsChanged();
    }

    /// <summary>
    /// Called on the entity's end of turn: the statuses another entity put on it count their turns down, so that one of 1 turn
    /// lasts through its next turn
    /// </summary>
    public virtual void OnTurnStop()
    {
        if (CountDownStatuses(selfApplied: false)) NotifyStatsChanged();
    }

    /// <summary>
    /// Counts down the statuses the entity put on itself (at the start of its turns: one of 1 turn lasts until its next turn,
    /// through the others', as the original's) or those another entity put on it (at the end of its turns): true if any
    /// </summary>
    private bool CountDownStatuses(bool selfApplied)
    {
        bool any = false;
        foreach (StatusEffect status in statusEffects.Values.ToList())
        {
            if (status.SelfApplied != selfApplied) continue;
            any = true;
            if (--status.Duration <= 0) statusEffects.Remove(status.Data);
        }
        return any;
    }

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
    /// <param name="roll">Where the damage fell in its random range, 0 its lowest and 1 its highest; negative if not rolled</param>
    public void TakeDamage(int amount, bool ignoreArmor = false, float roll = -1)
    {
        if (currentHealth <= 0) return;

        int remainingDamage = ignoreArmor ? amount : Mathf.Max(0, amount - armor);
        if (!ignoreArmor) armor = Mathf.Max(0, armor - amount);

        HitRoll = roll;
        Hit?.Invoke(remainingDamage);
        GameEvents.TakeDamage(this, amount, remainingDamage);
        HitRoll = -1;
        currentHealth = Mathf.Max(immortal ? 1 : 0, currentHealth - remainingDamage);
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
    /// While its hit is told (<see cref="Hit"/>, <see cref="GameEvents.DamageTaken"/>): where its damage fell in its random
    /// range, 0 its lowest and 1 its highest; negative if not rolled
    /// </summary>
    public float HitRoll { get; private set; } = -1;

    /// <summary>
    /// Grants armor to the entity
    /// </summary>
    /// <param name="amount"></param>
    public void GainArmor(int amount)
    {
        armor += amount;
        NotifyStatsChanged();
        GameEvents.GainArmor(this, amount);
    }

    /// <summary>
    /// Heals the entity, increasing his current HP
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        if (IsDead) return;
        int healed = Mathf.Min(currentHealth + amount, maxHealth) - currentHealth;
        currentHealth += healed;
        NotifyStatsChanged();
        GameEvents.Heal(this, healed);
    }

    /// <summary>
    /// Called when the entity dies, and removes it from combat.
    /// </summary>
    protected virtual void Die()
    {
        GameEvents.Die(this);
        Tile.SetEntity(null);
        GetComponent<EntityTurn>().RemoveFromTurnSystem();
        ActionManager.AddToBottom(new DieAction(this));
    }

    /// <summary>
    /// Applies a status effect for some turns, or extends it. Cancels the opposite status instead if the entity has it.
    /// <paramref name="selfApplied"/>: the entity puts it on itself, which counts it down at the start of its turns
    /// </summary>
    public void AddStatusEffect(StatusEffectData status, int duration, bool selfApplied = false)
    {
        if (status.opposite != null && statusEffects.ContainsKey(status.opposite))
            statusEffects.Remove(status.opposite);
        else if (statusEffects.TryGetValue(status, out StatusEffect current))
        {
            // The longer application is kept, with its countdown
            if (duration > current.Duration)
            {
                current.Duration = duration;
                current.SelfApplied = selfApplied;
            }
        }
        else
            statusEffects.Add(status, new StatusEffect(status, duration) { SelfApplied = selfApplied });
        NotifyStatsChanged();
        GameEvents.ApplyStatus(this, status);
    }

    public IEnumerable<StatusEffect> StatusEffects => statusEffects.Values;

    /// <summary>
    /// How much applying the status would change the multiplier of its stat: its delta, nothing if the entity has it already,
    /// or the opposite status's delta taken back if it cancels it
    /// </summary>
    public float ModifierChangeIfApplied(StatusEffectData status)
    {
        if (status.opposite != null && statusEffects.ContainsKey(status.opposite)) return -status.opposite.delta;
        return statusEffects.ContainsKey(status) ? 0 : status.delta;
    }

    // Read by every damage calculation and preview: no LINQ
    private float StatusModifier(StatusEffectData.Stat stat)
    {
        float sum = 0;
        foreach (StatusEffectData status in statusEffects.Keys)
            if (status.stat == stat) sum += status.delta;
        return sum;
    }

    public EntityData Data => data;

    /// <summary>
    /// Identifies the entity in the localization and the run stats
    /// </summary>
    public string ID => data.ID;

    //Properties
    public float DamageDealtMultiplier => damageDealtMultiplier + StatusModifier(StatusEffectData.Stat.DamageDealt);
    public float DamageReceivedMultiplier => damageReceivedMultiplier + StatusModifier(StatusEffectData.Stat.DamageReceived);
    /// <summary>
    /// The damage an amount dealt by this entity does to the target, before its armor
    /// </summary>
    public int DamageTo(EntityStats target, int amount) => Mathf.CeilToInt(amount * DamageDealtMultiplier * target.DamageReceivedMultiplier);
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }

    /// <summary>
    /// Whether a hit taking all its health kills it: the dummy stops at 1, Drareg at its phase threshold
    /// </summary>
    public virtual bool CanDie => !immortal;

    /// <summary>
    /// Never falls under 1 health, healed to full at its turns: the training dummy, the player with a debug shortcut (DevCheats)
    /// </summary>
    public bool Immortal { get => immortal; set => immortal = value; }
    public int Armor => armor;
    public Sprite TimelineIcon => data.timelineIcon;
}
