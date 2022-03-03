using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System to handle the health of <c>Player</c> and <c>Enemy</c>
/// </summary>
public class HealthSystem : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    /// <summary>
    /// Increase the maximum health
    /// </summary>
    /// <param name="healthAdded">Health to add</param>
    public void IncreaseMaxHealth(int healthAdded)
    {
        maxHealth += healthAdded;
    }

    /// <summary>
    /// Decrease the maximum health
    /// </summary>
    /// <param name="healthRemoved">Health to remove</param>
    public void DecreaseMaxHealth(int healthRemoved)
    {
        maxHealth -= healthRemoved;
    }

    /// <summary>
    /// Increase the health
    /// </summary>
    /// <param name="healthAdded">Health to add</param>
    public void GainHealth(int healthAdded)
    {
        currentHealth += healthAdded;

        if(currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    /// <summary>
    /// Decrease the health
    /// </summary>
    /// <param name="healthRemoved">Health to remove</param>
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
