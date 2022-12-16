using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergy : MonoBehaviour
{
    public Sprite filledEnergySprite;
    public Sprite emptyEnergySprite;

    public float xOffset;

    private PlayerStats playerStats;
    [SerializeField] private GameObject[] energies;

    private void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        
        InitEnergies();
    }

    private void InitEnergies()
    {
        energies = new GameObject[playerStats.MaxEnergy];

        for (int i = 0; i < playerStats.MaxEnergy; i++)
        {
            GameObject energy = new GameObject();
            energy.transform.SetParent(transform);
            energy.name = "EnergyCell" + i.ToString();
            
            RectTransform rectTransformComponent = energy.AddComponent<RectTransform>();

            Image imageComponent = energy.AddComponent<Image>();
            imageComponent.preserveAspect = true;
            imageComponent.sprite = filledEnergySprite;

            rectTransformComponent.localScale = new Vector3(1, 1, 1);
            rectTransformComponent.sizeDelta = new Vector2(10, 10);
            rectTransformComponent.anchoredPosition = new Vector2(0.25f, 0.5f);
            rectTransformComponent.offsetMin = new Vector2(0, 0);
            rectTransformComponent.offsetMax = new Vector2(0, 0);

            rectTransformComponent.anchorMin = new Vector2(i * xOffset, 0);
            rectTransformComponent.anchorMax = new Vector2((i + 1) * xOffset, 1f);

            energies[i] = energy;
        }
    }

    private void Update()
    {
        UpdateEnergyUI();
    }

    private int lastCurrentEnergy = 0;
    public void UpdateEnergyUI()
    {
        if (lastCurrentEnergy == playerStats.CurrentEnergy)
            return;
        
        lastCurrentEnergy = playerStats.CurrentEnergy;
        for (int i = 0; i < playerStats.MaxEnergy; i++)
        {
            Image imageComponent = energies[i].GetComponent<Image>();
            if (i < playerStats.CurrentEnergy)
                imageComponent.sprite = filledEnergySprite;
            else
                imageComponent.sprite = emptyEnergySprite;
        }
    }
}
