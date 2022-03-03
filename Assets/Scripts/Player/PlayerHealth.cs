using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public void IncreaseMaxHealth(int healthAdded)
    {
        maxHealth += healthAdded;
    }

    public void DecreaseMaxHealth(int healthRemoved)
    {
        maxHealth -= healthRemoved;
    }

    public void GainHealth(int healthAdded)
    {
        currentHealth += healthAdded;

        if(currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void LoseHealth(int healthRemoved)
    {
        currentHealth -= healthRemoved;

        if (currentHealth < 0)
        {
            currentHealth = 0;
            //TODO DEAD
        }
    }
}
