using UnityEngine;
using TMPro;

public class Results : MonoBehaviour
{
    [SerializeField] private GameObject DeathCanvasObject;
    [SerializeField] private TMP_Text deathMessage;
    [SerializeField] private TMP_Text scoreObject;
    private PlayerInfo playerInfo;
    private ChangeUI changeUI;

    void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
        changeUI = GetComponent<ChangeUI>();
    }

    public void DisplayResultCanvas(bool isVictory)
    {
        DeathCanvasObject.SetActive(true);
        changeUI.UIInformation();
        changeUI.ChangeBlur();

        scoreObject.text = string.Format(Localization.UI("EndScreenScore"), playerInfo.score.ToString());
        if (playerInfo.score >= 50000)
            SteamAchievements.SetAchievement("ACH_MAXSCORE");

        if (isVictory)
        {
            deathMessage.text = Localization.UI("EndScreenVictory");
            deathMessage.color = new Color32(25, 207, 21, 255);
        }
        else
        {
            deathMessage.text = Localization.UI("EndScreenDefeat");
            deathMessage.color = new Color32(232, 42, 104, 255);
        }
    }

}
