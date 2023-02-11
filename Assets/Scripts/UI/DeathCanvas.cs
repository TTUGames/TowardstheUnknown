using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathCanvas : MonoBehaviour
{
    [SerializeField] private GameObject DeathCanvasObject;
    [SerializeField] private TMP_Text timeObject;
    [SerializeField] private TMP_Text entitykilledObject;
    private PlayerInfo playerInfo;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        playerInfo = GetComponent<PlayerInfo>();
    }

    public void DisplayDeathCanvas()
    {
        int entitykilled = playerInfo.nanukoKilled + playerInfo.kameikoKilled + playerInfo.golemKilled;
        DeathCanvasObject.SetActive(true);
        timeObject.text = "Durée de la partie : " + (Time.time - startTime).ToString("F2") + " secondes";
        entitykilledObject.text = "Entités tués : " + entitykilled.ToString();
    }
}

