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
        CombatStarted = null;
        CombatEnded = null;
        ExplorationStarted = null;
        EntityDied = null;
        BossPhaseChanged = null;
        RunEnded = null;
    }

    public static void EnterRoom(Room room, bool firstVisit) => RoomEntered?.Invoke(room, firstVisit);

    public static void LeaveRoom() => RoomLeft?.Invoke();

    public static void StartCombat() => CombatStarted?.Invoke();

    public static void EndCombat() => CombatEnded?.Invoke();

    public static void StartExploration() => ExplorationStarted?.Invoke();

    public static void Die(EntityStats entity) => EntityDied?.Invoke(entity);

    public static void ChangeBossPhase(int phase) => BossPhaseChanged?.Invoke(phase);

    public static void EndRun(bool isVictory) => RunEnded?.Invoke(isVictory);
}
