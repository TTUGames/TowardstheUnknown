using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private TMP_Text healthAndShieldText;
    [SerializeField] private TMP_Text maxHealthText;
    
    private PlayerStats playerStats;
       
    private void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        maxHealthText.text = playerStats.MaxHealth.ToString();
    }

    private int lastCurrentHealth = 0;
    private int lastShield = 0;
    private void Update()
    {
        if (lastCurrentHealth == playerStats.CurrentHealth && lastShield == playerStats.Armor)
            return;
        lastCurrentHealth = playerStats.CurrentHealth;
        lastShield = playerStats.Armor;

        healthSlider.value = (float) (lastCurrentHealth / 100f);
        shieldSlider.value = (float) (lastShield / 100f);

        healthAndShieldText.text = playerStats.CurrentHealth + " (" + playerStats.Armor + ")";
    }

}
