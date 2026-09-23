using UnityEngine;
using TMPro;

public class DisplayStats : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public GameObject entity;
    public string entityName;

    private EntityStats entityStats;

    public void SetEntityStats(EntityStats stats)
    {
        if (entityStats != null) entityStats.StatsChanged -= Refresh;
        entityStats = stats;
        entityStats.StatsChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (entityStats != null) entityStats.StatsChanged -= Refresh;
    }

    private void Refresh()
    {
        textMeshPro.text =
            "<font-weight=\"700\"><size=\"28\">" + entityName + "</font-weight><size=\"18\">\n\n" +
            "<color=#CCCCCC>PV : " + entityStats.CurrentHealth + "/" + entityStats.MaxHealth + "\n\n" +
            "ATT : " + (entityStats.DamageDealtMultiplier - 1) * 100 + "%\n\n" +
            "DEF : " + (1 - entityStats.DamageReceivedMultiplier) * 100 + "%";
    }
}
