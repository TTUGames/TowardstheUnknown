using System.Collections;
using System.Collections.Generic;
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

    public void Start()
    {
        attTurn.text = "";
        defTurn.text = "";
    }

    public void DisplayBuffDebuff()
    {
        DisplayBuffDebuff("Attack", entityStats.DamageDealtMultiplier, 1.2f, 0.8f, AttackUp, AttackDown, attTurn);
        DisplayBuffDebuff("Defense", entityStats.DamageReceivedMultiplier, 0.8f, 1.2f, DefenseUp, DefenseDown, defTurn);
    }

    private void DisplayBuffDebuff(string statName, float statMultiplier, float buffMultiplier, float debuffMultiplier, GameObject buffObject, GameObject debuffObject, TextMeshProUGUI turnText)
    {
        if (statMultiplier == buffMultiplier)
        {
            buffObject.SetActive(true);
            debuffObject.SetActive(false);
            turnText.text = entityStats.GetStatusEffect(statName + "Up").Duration.ToString();
        }
        else if (statMultiplier == debuffMultiplier)
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
