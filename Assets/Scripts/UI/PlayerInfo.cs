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
    [SerializeField] private TMP_Text scoreNumber;
    private string playerName;
    
    [Space]
    [Header("Player Names")]
    [SerializeField] private List<string> playerNames = new List<string>() { "Jorick le con", "Lucas dans le sac", "ABAB" };

    [Space]
    [Header("Stats to load")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private DisplayScore displayScore;

    [Space]
    [Header("Entity killed")]
    public int kameikoKilled;
    public int nanukoKilled;
    public int golemKilled;

    void Start()
    {
        kameikoKilled = 0;
        nanukoKilled = 0;
        golemKilled = 0;
        displayScore.score = 0;
        
        int randomIndex = UnityEngine.Random.Range(0, playerNames.Count);
        playerName = playerNames[randomIndex];
        UpdatePlayerInfo();
    }

    public void UpdatePlayerInfo()
    {
        statsName.text = playerName;
        statsHealth.text = "PV : " + playerStats.CurrentHealth + " (" + playerStats.Armor + ") /" + playerStats.MaxHealth ;
        statsEnergy.text = "PE : " + playerStats.CurrentEnergy + "/" + playerStats.MaxEnergy;
        statsAtt.text = "ATT : " + (playerStats.DamageDealtMultiplier - 1) * 100 + "%";
        statsDef.text = "DEF : " + (1 - playerStats.DamageReceivedMultiplier) * 100 + "%";

        kameikoKilledNumber.text = "Kameiko tués : " + kameikoKilled;
        nanukoKilledNumber.text = "Nanuko tués : " + nanukoKilled;
        golemKilledNumber.text = "Golem tués : " + golemKilled;

        scoreNumber.text = "Score : " + displayScore.score.ToString().PadLeft(6, '0');
    }
}
