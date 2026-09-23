using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    [Header("Global Settings")]
    [SerializeField] private TMP_Text statsName;
    [SerializeField] private TMP_Text statsHealth;
    [SerializeField] private TMP_Text statsEnergy;
    [SerializeField] private TMP_Text statsAtt;
    [SerializeField] private TMP_Text statsDef;
    [SerializeField] private TMP_Text nanukoKilledNumber;
    [SerializeField] private TMP_Text kameikoKilledNumber;
    [SerializeField] private TMP_Text golemKilledNumber;
    [SerializeField] private TMP_Text visitedRoomDisplay;
    [SerializeField] private TMP_Text scoreNumber;
    private string playerName;
    
    [Space]
    [Header("Player Names")]
    [SerializeField] private List<string> playerNames = new List<string>() { "Prénom1 Nom1", "Prénom2 Nom2"};

    [Space]
    [Header("Stats to load")]
    [SerializeField] private PlayerStats playerStats;
    [Space]
    [Header("Entity killed")]
    [HideInInspector] public int kameikoKilled;
    [HideInInspector] public int nanukoKilled;
    [HideInInspector] public int golemKilled;
    [HideInInspector] public int visitedRoomCount;
    [HideInInspector] public int score;

    void Start()
    {
        kameikoKilled = 0;
        nanukoKilled = 0;
        golemKilled = 0;
        visitedRoomCount = 0;
        score = 0;
        
        int randomIndex = UnityEngine.Random.Range(0, playerNames.Count);
        playerName = playerNames[randomIndex];
        UpdatePlayerInfo();
    }

    public void UpdatePlayerInfo()
    {
        statsName.text = playerName;
        statsHealth.text = string.Format(Localization.UI("PlayerStatsHP"), playerStats.CurrentHealth, playerStats.Armor, playerStats.MaxHealth);
        statsEnergy.text = string.Format(Localization.UI("PlayerStatsEnergy"), playerStats.CurrentEnergy, playerStats.MaxEnergy);
        statsAtt.text = string.Format(Localization.UI("PlayerStatsAttack"), (playerStats.DamageDealtMultiplier - 1) * 100);
        statsDef.text = string.Format(Localization.UI("PlayerStatsDefense"), (1 - playerStats.DamageReceivedMultiplier) * 100);

        kameikoKilledNumber.text = string.Format(Localization.UI("PlayerProgressKameikoCount"), kameikoKilled);
        nanukoKilledNumber.text = string.Format(Localization.UI("PlayerProgressNanukoCount"), nanukoKilled);
        golemKilledNumber.text = string.Format(Localization.UI("PlayerProgressGolemCount"), golemKilled);
        visitedRoomDisplay.text = string.Format(Localization.UI("PlayerProgressVisitedRoom"), visitedRoomCount);

        scoreNumber.text = string.Format(Localization.UI("PlayerProgressScore"), score.ToString().PadLeft(6, '0'));
    }
}
