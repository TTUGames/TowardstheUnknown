using UnityEngine;
using TMPro;
using System;

public class DisplayStats : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public GameObject player;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = player.GetComponent<PlayerStats>();
    }

    private void Update()
    {
        textMeshPro.text = "Points de vie : " + playerStats.currentHealth.ToString() + "/" + playerStats.MaxHealth.ToString() + 
        "\nBonus de défense : " + Math.Abs(((playerStats.DamageReceivedMultiplier * 100)-100)).ToString() + "%"+ 
        "\nBonus d'attaque : " + Math.Abs(((playerStats.DamageDealtMultiplier * 100)-100)).ToString() + "%";
    }
}
