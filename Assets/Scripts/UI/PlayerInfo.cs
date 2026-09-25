using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The run's progress shown on the character sheet and the results: the player's name, kills, rooms and score
/// </summary>
public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private List<string> playerNames = new List<string>() { "Prénom1 Nom1", "Prénom2 Nom2"};

    [HideInInspector] public int kameikoKilled;
    [HideInInspector] public int nanukoKilled;
    [HideInInspector] public int golemKilled;
    [HideInInspector] public int visitedRoomCount;
    [HideInInspector] public int score;

    public string PlayerName { get; private set; }

    private void Awake()
    {
        PlayerName = playerNames[Random.Range(0, playerNames.Count)];
    }
}
