using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private TMP_Text zoneHeader;
    [SerializeField] private TMP_Text zoneContent;
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
        statsHealth.text = string.Format(Localization.GetUIString("PlayerStatsHP").TEXT, playerStats.currentHealth, playerStats.Armor, playerStats.MaxHealth);
        statsEnergy.text = string.Format(Localization.GetUIString("PlayerStatsEnergy").TEXT, playerStats.CurrentEnergy, playerStats.MaxEnergy);
        statsAtt.text = string.Format(Localization.GetUIString("PlayerStatsAttack").TEXT, (playerStats.DamageDealtMultiplier - 1) * 100);
        statsDef.text = string.Format(Localization.GetUIString("PlayerStatsDefense").TEXT, (1 - playerStats.DamageReceivedMultiplier) * 100);

        kameikoKilledNumber.text = string.Format(Localization.GetUIString("PlayerProgressKameikoCount").TEXT, kameikoKilled);
        nanukoKilledNumber.text = string.Format(Localization.GetUIString("PlayerProgressNanukoCount").TEXT, nanukoKilled);
        golemKilledNumber.text = string.Format(Localization.GetUIString("PlayerProgressGolemCount").TEXT, golemKilled);
        visitedRoomDisplay.text = string.Format(Localization.GetUIString("PlayerProgressVisitedRoom").TEXT, visitedRoomCount);

        scoreNumber.text = string.Format(Localization.GetUIString("PlayerProgressScore").TEXT, score.ToString().PadLeft(6, '0'));

        /*if (FindObjectOfType<Map>() != null && FindObjectOfType<Map>().CurrentRoom != null) {
            RoomType currentRoomType = FindObjectOfType<Map>().CurrentRoom.type;
            if (currentRoomType == RoomType.ANTECHAMBER || currentRoomType == RoomType.BOSS) {
                zoneHeader.text = Localization.GetUIString("ZoneInfoGardenHeader").TEXT;
                zoneContent.text = Localization.GetUIString("ZoneInfoGardenContent").TEXT;
            }
            else {
                zoneHeader.text = Localization.GetUIString("ZoneInfoZeroHeader").TEXT;
                zoneContent.text = Localization.GetUIString("ZoneInfoZeroContent").TEXT;
            }
        }*/
    }
}
