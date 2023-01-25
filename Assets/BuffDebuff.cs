using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffDebuff : MonoBehaviour
{
    public GameObject AttackUp;
    public GameObject AttackDown;
    public GameObject DefenseUp;
    public GameObject DefenseDown;
    public EntityStats entityStats;
    public void DisplayBuffDebuff()
    {
        Debug.LogWarning("DisplayBuffDebuff");

        if (entityStats.DamageDealtMultiplier == 1.2f)
        {
            AttackUp.SetActive(true);
            AttackDown.SetActive(false);
        }
        else if (entityStats.DamageDealtMultiplier == 0.8f)
        {
            AttackDown.SetActive(true);
            AttackUp.SetActive(false);
        }
        else {
            AttackDown.SetActive(false);
            AttackUp.SetActive(false);
        }
        
        if (entityStats.DamageReceivedMultiplier == 0.8f)
        {
            DefenseUp.SetActive(true);
            DefenseDown.SetActive(false);
        }
        else if (entityStats.DamageReceivedMultiplier == 1.2f)
        {
            DefenseDown.SetActive(true);
            DefenseUp.SetActive(false);
        }
        else {
            DefenseDown.SetActive(false);
            DefenseUp.SetActive(false);
        }
    }
        void Update()
    {
        //DisplayBuffDebuff();
    }
}
