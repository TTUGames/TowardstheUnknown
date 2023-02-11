using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathCanvas : MonoBehaviour
{
    [SerializeField] private GameObject DeathCanvasObject;
    [SerializeField] private TMP_Text timeObject;
    [SerializeField] private TMP_Text entitykilledObject;
    [SerializeField] private TMP_Text scoreObject;
    private PlayerInfo playerInfo;
    private float startTime;
    private ChangeUI changeUI;

    void Start()
    {
        startTime = Time.time;
        playerInfo = GetComponent<PlayerInfo>();
        changeUI = GetComponent<ChangeUI>();
    }

    public void DisplayDeathCanvas()
    {
        changeUI.ChangeBlur(true);
        int entitykilled = playerInfo.nanukoKilled + playerInfo.kameikoKilled + playerInfo.golemKilled;
        DeathCanvasObject.SetActive(true);
        timeObject.text = "Durée de la partie : " + (Time.time - startTime).ToString("F2") + " secondes";
        entitykilledObject.text = "Entités tués : " + entitykilled.ToString();
        scoreObject.text = "Score : " + playerInfo.score.ToString();
        if (playerInfo.score >= 100)
            SteamAchievements.SetAchievement("ACH_MAXSCORE");
    }
}

