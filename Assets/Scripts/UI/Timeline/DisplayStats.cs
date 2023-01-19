using UnityEngine;
using TMPro;
using System;

public class DisplayStats : MonoBehaviour
{
    public TMP_Text textMeshPro;
    private EntityStats entityStats;
    private void Update()
    {
        if (entityStats == null)
            return;
        
        textMeshPro.text =
        "Points de vie : " + Math.Max(0, entityStats.currentHealth) + "/" + entityStats.MaxHealth + "\n" +
        "Bonus de défense : " + (1 - entityStats.DamageReceivedMultiplier) * 100 + "%\n" + 
        "Bonus d'attaque : " + (entityStats.DamageDealtMultiplier - 1) * 100 + "%";
    }

    public void SetEntityStats(GameObject entity)
    {
        entityStats = entity.GetComponent<EnemyStats>();
        
        if (entityStats == null)
            entityStats = entity.GetComponent<PlayerStats>();
    }
}
