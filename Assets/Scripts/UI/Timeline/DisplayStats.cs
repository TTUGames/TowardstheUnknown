using UnityEngine;
using TMPro;
using System;

public class DisplayStats : MonoBehaviour
{
    public TMP_Text textMeshPro;
    private EntityStats entityStats;
    public GameObject entity;
    public string entityName;

    private void Update()
    {
        if (entityStats == null)
            return;
        
        textMeshPro.text =
        "<font-weight=\"700\"><size=\"28\">" + entityName + "</font-weight><size=\"18\">" + "\n" + "\n" +
        "PV : " + Math.Max(0, entityStats.currentHealth) + "/" + entityStats.MaxHealth + "\n" + "\n" +
        "ATT : " + (entityStats.DamageDealtMultiplier - 1) * 100 + "%\n" + "\n" +
        "DEF : " + (1 - entityStats.DamageReceivedMultiplier) * 100 + "%";
    }

    public void SetEntityStats(GameObject entity)
    {
        entityStats = entity.GetComponent<EnemyStats>();
        
        if (entityStats == null)
            entityStats = entity.GetComponent<PlayerStats>();
    }
}
