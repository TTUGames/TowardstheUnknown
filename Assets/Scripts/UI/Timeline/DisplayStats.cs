using UnityEngine;
using TMPro;

public class DisplayStats : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public GameObject entity;
    public string entityName;

    private EntityStats entityStats;
    private (int health, float attack, float defense) displayedStats;

    private void Update()
    {
        if (entityStats == null)
            return;

        var stats = (Mathf.Max(0, entityStats.currentHealth), entityStats.DamageDealtMultiplier, entityStats.DamageReceivedMultiplier);
        if (stats == displayedStats)
            return;
        displayedStats = stats;

        textMeshPro.text =
            "<font-weight=\"700\"><size=\"28\">" + entityName + "</font-weight><size=\"18\">\n\n" +
            "<color=#CCCCCC>PV : " + stats.Item1 + "/" + entityStats.MaxHealth + "\n\n" +
            "ATT : " + (stats.Item2 - 1) * 100 + "%\n\n" +
            "DEF : " + (1 - stats.Item3) * 100 + "%";
    }

    public void SetEntityStats(EntityStats stats)
    {
        entityStats = stats;
    }
}
