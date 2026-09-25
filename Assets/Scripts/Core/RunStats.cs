using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The run's progress shown on the character sheet and the results: the player's name, kills, visited rooms and score.
/// Counted from the game events: gameplay never writes to it
/// </summary>
public class RunStats : MonoBehaviour
{
    [SerializeField] private List<string> playerNames = new List<string>() { "Prénom1 Nom1", "Prénom2 Nom2"};

    // By entity ID
    private readonly Dictionary<string, int> kills = new Dictionary<string, int>();

    public string PlayerName { get; private set; }
    public int VisitedRoomCount { get; private set; }
    public int Score { get; private set; }

    private void Awake()
    {
        PlayerName = playerNames[Random.Range(0, playerNames.Count)];
    }

    private void OnEnable()
    {
        GameEvents.EntityDied += OnEntityDied;
        GameEvents.RoomEntered += OnRoomEntered;
    }

    private void OnDisable()
    {
        GameEvents.EntityDied -= OnEntityDied;
        GameEvents.RoomEntered -= OnRoomEntered;
    }

    private void OnEntityDied(EntityStats entity)
    {
        Score += entity.entityKilledScore;
        if (entity is EnemyStats)
            kills[entity.ID] = KillCount(entity.ID) + 1;
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (firstVisit && room.type != RoomType.SPAWN) VisitedRoomCount++;
    }

    private int KillCount(string id) => kills.TryGetValue(id, out int count) ? count : 0;

    /// <summary>
    /// The enemies killed whose ID contains <paramref name="family"/>: "Kameiko" counts the Great Kameikos too
    /// </summary>
    public int KillsOf(string family) => kills.Where(kill => kill.Key.Contains(family)).Sum(kill => kill.Value);
}
