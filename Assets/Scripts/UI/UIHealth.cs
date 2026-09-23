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
    private int lastCurrentHealth = -1;
    private int lastShield = -1;

    private void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        maxHealthText.text = playerStats.MaxHealth.ToString();
    }

    private void Update()
    {
        if (lastCurrentHealth == playerStats.CurrentHealth && lastShield == playerStats.Armor)
            return;
        lastCurrentHealth = playerStats.CurrentHealth;
        lastShield = playerStats.Armor;

        healthSlider.value = (float)lastCurrentHealth / playerStats.MaxHealth;
        shieldSlider.value = (float)lastShield / playerStats.MaxHealth;
        healthAndShieldText.text = lastCurrentHealth + " (" + lastShield + ")";
    }
}
