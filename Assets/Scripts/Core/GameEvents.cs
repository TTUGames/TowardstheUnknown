using System.Collections.Generic;
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
    /// Fired when a room's loot changes: a relic lies in it or was picked up (<see cref="Room.HasLoot"/>)
    /// </summary>
    public static event System.Action<Room> LootChanged;

    /// <summary>
    /// Fired when a pushed entity stops short of its push's distance, against a wall or an entity
    /// </summary>
    public static event System.Action<EntityStats> PushBlocked;
    /// <summary>
    /// Fired when an attack starts, with its caster and the tiles it hits (the targeted tile, or its area)
    /// </summary>
    public static event System.Action<EntityStats, IReadOnlyList<Tile>> AttackStarted;

    /// <summary>
    /// Fired when a relic is opened, with its artifacts: the inventory opens them in a chest
    /// </summary>
    public static event System.Action<IReadOnlyList<Artifact>> ChestOpened;

    /// <summary>
    /// Fired when the room an exit leads to is pointed at (an open exit under the pointer), with null when none
    /// </summary>
    public static event System.Action<Vector2Int?> ExitTargeted;

    /// <summary>
    /// Fired once the combat room is revealed and the player can choose their deploy tile, with the deploy to end
    /// </summary>
    public static event System.Action<CombatPlayerDeploy> DeployChoiceShown;

    /// <summary>
    /// Fired when the player starts choosing their deploy tile, before a combat
    /// </summary>
    public static event System.Action DeployStarted;

    /// <summary>
    /// Fired when the player, in the deploy phase, moves to another deploy tile, with the position it leaves
    /// </summary>
    public static event System.Action<EntityStats, Vector3> Redeployed;

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
    /// Fired when a boss makes its entrance, at the start of its combat before the first turn plays (EditionProfile.bossIntro),
    /// with the boss and the game seconds it lasts
    /// </summary>
    public static event System.Action<EntityStats, float> BossIntroStarted;

    /// <summary>
    /// Fired when the run ends, with true if the player won
    /// </summary>
    public static event System.Action<bool> RunEnded;

    /// <summary>
    /// Fired when the platform ranked the run's score among every player's best ones, with the player's world rank and true if
    /// the score is the player's new best
    /// </summary>
    public static event System.Action<int, bool> ScoreRanked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        RoomEntered = null;
        RoomLeft = null;
        LootChanged = null;
        ChestOpened = null;
        PushBlocked = null;
        AttackStarted = null;
        ExitTargeted = null;
        DeployChoiceShown = null;
        DeployStarted = null;
        Redeployed = null;
        CombatStarted = null;
        CombatEnded = null;
        ExplorationStarted = null;
        EntityDied = null;
        DamageTaken = null;
        Healed = null;
        ArmorGained = null;
        StatusApplied = null;
        BossPhaseChanged = null;
        BossIntroStarted = null;
        RunEnded = null;
        ScoreRanked = null;
    }

    public static void EnterRoom(Room room, bool firstVisit) => RoomEntered?.Invoke(room, firstVisit);

    public static void LeaveRoom() => RoomLeft?.Invoke();

    public static void ChangeLoot(Room room) => LootChanged?.Invoke(room);

    public static void StartDeploy() => DeployStarted?.Invoke();

    public static void Redeploy(EntityStats entity, Vector3 from) => Redeployed?.Invoke(entity, from);

    public static void ShowDeployChoice(CombatPlayerDeploy deploy) => DeployChoiceShown?.Invoke(deploy);

    public static void OpenChest(IReadOnlyList<Artifact> artifacts) => ChestOpened?.Invoke(artifacts);

    public static void BlockPush(EntityStats entity) => PushBlocked?.Invoke(entity);
    public static void StartAttack(EntityStats caster, IReadOnlyList<Tile> tiles) => AttackStarted?.Invoke(caster, tiles);

    public static void TargetExit(Vector2Int? room) => ExitTargeted?.Invoke(room);

    public static void StartCombat() => CombatStarted?.Invoke();

    public static void EndCombat() => CombatEnded?.Invoke();

    public static void StartExploration() => ExplorationStarted?.Invoke();

    public static void Die(EntityStats entity) => EntityDied?.Invoke(entity);

    public static void TakeDamage(EntityStats entity, int damage, int healthLost) => DamageTaken?.Invoke(entity, damage, healthLost);

    public static void Heal(EntityStats entity, int health) => Healed?.Invoke(entity, health);

    public static void GainArmor(EntityStats entity, int armor) => ArmorGained?.Invoke(entity, armor);

    public static void ApplyStatus(EntityStats entity, StatusEffectData status) => StatusApplied?.Invoke(entity, status);

    public static void ChangeBossPhase(int phase) => BossPhaseChanged?.Invoke(phase);

    public static void StartBossIntro(EntityStats boss, float seconds) => BossIntroStarted?.Invoke(boss, seconds);

    public static void EndRun(bool isVictory) => RunEnded?.Invoke(isVictory);

    public static void RankScore(int rank, bool newBest) => ScoreRanked?.Invoke(rank, newBest);
}
