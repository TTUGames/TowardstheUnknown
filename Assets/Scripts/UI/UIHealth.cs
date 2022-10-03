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
    
    private TempUITestStats tempUITestStats;
       
    private void Awake()
    {
        tempUITestStats = GameObject.FindGameObjectWithTag("Player").GetComponent<TempUITestStats>();
    }

    private void Update()
    {
        totalHealthText.text = (tempUITestStats.lifeCurrent + " (" + tempUITestStats.armor + ")");
        maxHealthText.text   = tempUITestStats.lifeMax.ToString();
        ResizeBars();
    }

    /// <summary>
    /// Resize the 2 bars to fit the current health and armor percentages
    /// </summary>
    private void ResizeBars()
    {
        float healthPercent = (float)tempUITestStats.lifeCurrent / tempUITestStats.lifeMax;
        float armorPercent  = (float)tempUITestStats.armor / tempUITestStats.lifeMax;
        healthBar.GetComponent<RectTransform>().anchorMax = new Vector2(healthPercent, 1);
        armorBar.GetComponent<RectTransform>().anchorMin  = new Vector2(1-armorPercent, 0);
    }
}
