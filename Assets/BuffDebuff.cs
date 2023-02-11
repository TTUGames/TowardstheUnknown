using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuffDebuff : MonoBehaviour
{
    public GameObject AttackUp;
    public GameObject AttackDown;
    public GameObject DefenseUp;
    public GameObject DefenseDown;
    public EntityStats entityStats;
    public TextMeshProUGUI attTurn;
    public TextMeshProUGUI defTurn;

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

    void Update()
    {
        DisplayBuffDebuff();
    }
}
