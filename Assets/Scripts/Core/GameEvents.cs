using UnityEngine;

/// <summary>
/// Game-wide events, raised by gameplay and listened to by the systems around it: UI, run stats, music, achievements
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// Fired when a room is loaded, its enemies and loot spawned, with true on its first visit
    /// </summary>
    public static event System.Action<Room, bool> RoomEntered;

    /// <summary>
    /// Fired when the player leaves a room for an adjacent one, before it is destroyed
    /// </summary>
    public static event System.Action RoomLeft;

    /// <summary>
    /// Fired when the player starts choosing their deploy tile, before a combat
    /// </summary>
    public static event System.Action DeployStarted;

    /// <summary>
    /// Fired when a combat starts, once the player is deployed and before the first turn
    /// </summary>
    public static event System.Action CombatStarted;

    /// <summary>
    /// Fired when the last enemy is dead and the action queue is empty
    /// </summary>
    public static event System.Action CombatEnded;

    /// <summary>
    /// Fired when the player can explore: a room without combat is entered, or its combat ended
    /// </summary>
    public static event System.Action ExplorationStarted;

    /// <summary>
    /// Fired when an entity dies, the player included
    /// </summary>
    public static event System.Action<EntityStats> EntityDied;

    /// <summary>
    /// Fired when any entity takes damage, with the damage before armor and the health lost: 0 if its armor took it all
    /// </summary>
    public static event System.Action<EntityStats, int, int> DamageTaken;

    /// <summary>
    /// Fired when any entity heals, with the health gained
    /// </summary>
    public static event System.Action<EntityStats, int> Healed;

    /// <summary>
    /// Fired when any entity gains armor
    /// </summary>
    public static event System.Action<EntityStats, int> ArmorGained;

    /// <summary>
    /// Fired when a status effect is applied on any entity, even when it cancels the opposite one
    /// </summary>
    public static event System.Action<EntityStats, StatusEffectData> StatusApplied;

    /// <summary>
    /// Fired when a boss enters a new phase, with the number of this phase (the first one being 1)
    /// </summary>
    public static event System.Action<int> BossPhaseChanged;

    /// <summary>
    /// Fired when the run ends, with true if the player won
    /// </summary>
    public static event System.Action<bool> RunEnded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        RoomEntered = null;
        RoomLeft = null;
        DeployStarted = null;
        CombatStarted = null;
        CombatEnded = null;
        ExplorationStarted = null;
        EntityDied = null;
        DamageTaken = null;
        Healed = null;
        ArmorGained = null;
        StatusApplied = null;
        BossPhaseChanged = null;
        RunEnded = null;
    }

    public static void EnterRoom(Room room, bool firstVisit) => RoomEntered?.Invoke(room, firstVisit);

    public static void LeaveRoom() => RoomLeft?.Invoke();

    public static void StartDeploy() => DeployStarted?.Invoke();

    public static void StartCombat() => CombatStarted?.Invoke();

    public static void EndCombat() => CombatEnded?.Invoke();

    public static void StartExploration() => ExplorationStarted?.Invoke();

    public static void Die(EntityStats entity) => EntityDied?.Invoke(entity);

    public static void TakeDamage(EntityStats entity, int damage, int healthLost) => DamageTaken?.Invoke(entity, damage, healthLost);

    public static void Heal(EntityStats entity, int health) => Healed?.Invoke(entity, health);

    public static void GainArmor(EntityStats entity, int armor) => ArmorGained?.Invoke(entity, armor);

    public static void ApplyStatus(EntityStats entity, StatusEffectData status) => StatusApplied?.Invoke(entity, status);

    public static void ChangeBossPhase(int phase) => BossPhaseChanged?.Invoke(phase);

    public static void EndRun(bool isVictory) => RunEnded?.Invoke(isVictory);
}
