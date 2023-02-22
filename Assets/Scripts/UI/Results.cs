using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Results : MonoBehaviour
{
    [SerializeField] private GameObject DeathCanvasObject;
    [SerializeField] private TMP_Text deathMessage;
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
        DeathCanvasObject.SetActive(true);
        changeUI.UIInformation();
        changeUI.ChangeBlur(true);
        int entitykilled = playerInfo.nanukoKilled + playerInfo.kameikoKilled + playerInfo.golemKilled;

        scoreObject.text = string.Format(Localization.GetUIString("EndScreenScore").TEXT, playerInfo.score.ToString());
        if (playerInfo.score >= 50000)
            SteamAchievements.SetAchievement("ACH_MAXSCORE");

        if (isVictory)
        {
            deathMessage.text = Localization.GetUIString("EndScreenVictory").TEXT;
            deathMessage.color = new Color32(25, 207, 21, 255);
        }
        else
        {
            deathMessage.text = Localization.GetUIString("EndScreenDefeat").TEXT;
            deathMessage.color = new Color32(232, 42, 104, 255);
        }
    }

}
