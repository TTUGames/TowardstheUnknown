using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject armorBar;
    [SerializeField] private Text totalHealthText;
    [SerializeField] private Text maxHealthText;
    
    private PlayerStats playerStats;
       
    private void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
    }

    private void Update()
    {
        totalHealthText.text = (playerStats.CurrentHealth + " (" + playerStats.Armor + ")");
        maxHealthText.text   = playerStats.MaxHealth.ToString();
        ResizeBars();
    }
    
    /// <summary>
    /// Resize the 2 bars to fit the current health and armor percentages
    /// </summary>
    public void ResizeBars()
    {
        float healthPercent = 0f;
        if(playerStats.CurrentHealth != 0)
            healthPercent = (float)playerStats.CurrentHealth / playerStats.MaxHealth;
        
        float armorPercent = 0f;
        if(playerStats.Armor != 0)
            armorPercent = (float)playerStats.Armor / playerStats.MaxHealth;
        
        healthBar.GetComponent<RectTransform>().anchorMax = new Vector2(healthPercent, 1);
        armorBar.GetComponent<RectTransform>().anchorMax  = new Vector2(armorPercent , 1);
    }
}
