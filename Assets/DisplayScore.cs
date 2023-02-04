using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayScore : MonoBehaviour
{
    public int score = 0;
    public TMP_Text textMeshPro;
    
    void Start()
    {
        textMeshPro.text = "Score : " + score ;

        if (score >= 100)
            SteamAchievements.SetAchievement("ACH_MAXSCORE");
    }

}
