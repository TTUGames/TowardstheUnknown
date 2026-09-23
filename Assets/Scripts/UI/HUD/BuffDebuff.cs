using UnityEngine;
using TMPro;

public class BuffDebuff : MonoBehaviour
{
    [SerializeField] protected GameObject AttackUp;
    [SerializeField] protected GameObject AttackDown;
    [SerializeField] protected GameObject DefenseUp;
    [SerializeField] protected GameObject DefenseDown;
    [SerializeField] protected EntityStats entityStats;
    [SerializeField] protected TextMeshProUGUI attTurn;
    [SerializeField] protected TextMeshProUGUI defTurn;

    private void OnEnable()
    {
        entityStats.StatsChanged += DisplayBuffDebuff;
        DisplayBuffDebuff();
    }

    private void OnDisable()
    {
        entityStats.StatsChanged -= DisplayBuffDebuff;
    }

    private void DisplayBuffDebuff()
    {
        DisplayBuffDebuff("Attack", AttackUp, AttackDown, attTurn);
        DisplayBuffDebuff("Defense", DefenseUp, DefenseDown, defTurn);
    }

    private void DisplayBuffDebuff(string statName, GameObject buffObject, GameObject debuffObject, TextMeshProUGUI turnText)
    {
        if (entityStats.HasStatusEffect(statName + "Up"))
        {
            buffObject.SetActive(true);
            debuffObject.SetActive(false);
            turnText.text = entityStats.GetStatusEffect(statName + "Up").Duration.ToString();
        }
        else if (entityStats.HasStatusEffect(statName + "Down"))
        {
            debuffObject.SetActive(true);
            buffObject.SetActive(false);
            turnText.text = entityStats.GetStatusEffect(statName + "Down").Duration.ToString();
        }
        else
        {
            debuffObject.SetActive(false);
            buffObject.SetActive(false);
            turnText.text = "";
        }
    }
}
