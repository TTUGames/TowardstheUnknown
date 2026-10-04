using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The run's progress shown on the character sheet, the pause and the results: the player's name, kills, visited rooms,
/// score, time played and the enemy that dealt the player's last hit.
/// Counted from the game events: gameplay never writes to it
/// </summary>
public class RunStats : MonoBehaviour
{
    [SerializeField] private List<string> playerNames = new List<string>();

    // By ID of the kill family
    private readonly Dictionary<string, int> kills = new Dictionary<string, int>();

    public string PlayerName { get; private set; }
    public int KillCount { get; private set; }
    public int VisitedRoomCount { get; private set; }
    public int Score { get; private set; }

    // When the run started, in game time: the pause, which stops it, is left out
    private float startTime;

    /// <summary>
    /// Seconds played since the run started, the pauses left out
    /// </summary>
    public float PlayTime => Time.time - startTime;

    /// <summary>
    /// The kills by ID of the kill family
    /// </summary>
    public IReadOnlyDictionary<string, int> Kills => kills;

    /// <summary>
    /// The time played (<see cref="PlayTime"/>), frozen at the run's end
    /// </summary>
    public float Duration => ended ? endTime - startTime : PlayTime;

    /// <summary>
    /// The entity whose turn it was when the player last lost health: the one that killed it, after a defeat; null if none
    /// </summary>
    public EntityData LastHitBy { get; private set; }

    private float endTime;
    private bool ended;

    /// <summary>
    /// Raised once a kill or a visited room is counted
    /// </summary>
    public event System.Action Changed;

    private void Awake()
    {
        PlayerName = playerNames[Random.Range(0, playerNames.Count)];
        startTime = Time.time;
        if (RunSave.Resumed != null) Restore(RunSave.Resumed);
    }

    /// <summary>
    /// Writes the progress into a run's save
    /// </summary>
    public void Save(RunSave.Data save)
    {
        save.playerName = PlayerName;
        save.score = Score;
        save.visitedRoomCount = VisitedRoomCount;
        foreach (KeyValuePair<string, int> kill in kills)
        {
            save.killFamilies.Add(kill.Key);
            save.killCounts.Add(kill.Value);
        }
    }

    // A suspended run's progress
    private void Restore(RunSave.Data save)
    {
        if (!string.IsNullOrEmpty(save.playerName)) PlayerName = save.playerName;
        Score = save.score;
        VisitedRoomCount = save.visitedRoomCount;
        for (int i = 0; i < save.killFamilies.Count && i < save.killCounts.Count; i++)
        {
            kills[save.killFamilies[i]] = save.killCounts[i];
            KillCount += save.killCounts[i];
        }
    }

    private void OnEnable()
    {
        GameEvents.EntityDied += OnEntityDied;
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.DamageTaken += OnDamageTaken;
        GameEvents.RunEnded += OnRunEnded;
    }

    private void OnDisable()
    {
        GameEvents.EntityDied -= OnEntityDied;
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.DamageTaken -= OnDamageTaken;
        GameEvents.RunEnded -= OnRunEnded;
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (entity.type != EntityType.PLAYER || healthLost <= 0) return;
        EntityTurn attacker = TurnSystem.Instance != null ? TurnSystem.Instance.Current : null;
        if (attacker != null && attacker.TryGetComponent(out EntityStats stats) && stats != entity) LastHitBy = stats.Data;
    }

    private void OnRunEnded(bool isVictory)
    {
        if (ended) return;
        ended = true;
        endTime = Time.time;
    }

    private void OnEntityDied(EntityStats entity)
    {
        Score += entity.Data.score;
        if (entity is EnemyStats)
        {
            string family = entity.Data.KillFamily.ID;
            kills[family] = KillsOf(family) + 1;
            KillCount++;
            Changed?.Invoke();
        }
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (!firstVisit || room.type == RoomType.SPAWN) return;
        VisitedRoomCount++;
        Changed?.Invoke();
    }

    /// <summary>
    /// The enemies killed of a kill family, given by its entity ID: "Kameiko" counts the Great Kameikos too
    /// </summary>
    public int KillsOf(string family) => kills.TryGetValue(family, out int count) ? count : 0;
}
