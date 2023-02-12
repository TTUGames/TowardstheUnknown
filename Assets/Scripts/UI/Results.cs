using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Results : MonoBehaviour
{
    [SerializeField] private GameObject DeathCanvasObject;
    [SerializeField] private TMP_Text timeObject;
    [SerializeField] private TMP_Text deathMessage;
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

    public void DisplayResultCanvas(bool isVictory)
    {
        changeUI.ChangeBlur(true);
        DeathCanvasObject.SetActive(true);
        int entitykilled = playerInfo.nanukoKilled + playerInfo.kameikoKilled + playerInfo.golemKilled;
        timeObject.text = "Durée de la partie : " + (Time.time - startTime).ToString("F2") + " secondes";
        entitykilledObject.text = "Entités tués : " + entitykilled.ToString();
        scoreObject.text = "Score : " + playerInfo.score.ToString();
        if (playerInfo.score >= 100)
            SteamAchievements.SetAchievement("ACH_MAXSCORE");

        if (isVictory)
        {
            deathMessage.text = "Vous avez vaincu Drareg";
            deathMessage.color = new Color32(25, 207, 21, 255);
        }
        else
        {
            deathMessage.text = "La faille vous a emporté";
            deathMessage.color = new Color32(232, 42, 104, 255);
        }
    }

}
