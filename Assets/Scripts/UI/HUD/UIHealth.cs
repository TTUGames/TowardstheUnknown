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
        playerStats = GameScene.Player.Stats;
        maxHealthText.text = playerStats.MaxHealth.ToString();
    }

    private void OnEnable()
    {
        playerStats.StatsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        playerStats.StatsChanged -= Refresh;
    }

    private void Refresh()
    {
        healthSlider.value = (float)playerStats.CurrentHealth / playerStats.MaxHealth;
        shieldSlider.value = (float)playerStats.Armor / playerStats.MaxHealth;
        healthAndShieldText.text = playerStats.CurrentHealth + " (" + playerStats.Armor + ")";
    }
}
