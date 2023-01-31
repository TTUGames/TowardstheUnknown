using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergy : MonoBehaviour
{
    [SerializeField] private Sprite filledEnergySprite;
    [SerializeField] private Sprite emptyEnergySprite;
    [SerializeField] private Sprite previewedEnergySprite;

    public GameObject energyCellPrefab;

    public float xOffset;

    private PlayerStats playerStats;
    private GameObject[] energies;

    private void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        
        energies = new GameObject[playerStats.MaxEnergy];

        InitEnergies();
    }

    private void InitEnergies()
    {
        for (int i = 0; i < playerStats.MaxEnergy; i++)
            Destroy(energies[i]);

        for (int i = 0; i < playerStats.MaxEnergy; i++)
        {
            GameObject energy = Instantiate(energyCellPrefab, transform);
            energy.name = "EnergyCell" + i.ToString();
            
            Image imageComponent = energy.transform.GetChild(0).GetComponent<Image>();
            imageComponent.sprite = filledEnergySprite;

            RectTransform rectTransformComponent = energy.GetComponent<RectTransform>();
            rectTransformComponent.anchorMin = new Vector2(i * xOffset, 0);
            rectTransformComponent.anchorMax = new Vector2((i + 1) * xOffset, 1f);

            energies[i] = energy;
        }
    }

    private int lastCurrentEnergy = 0;
    private int lastPreviewedEnergy = 0;

    public void UpdateEnergyUI()
    {
        if (lastCurrentEnergy == playerStats.CurrentEnergy)
            return;
        lastCurrentEnergy = playerStats.CurrentEnergy;
        lastPreviewedEnergy = 0;
        for (int i = 0; i < playerStats.MaxEnergy; i++)
        {
            Image imageComponent = energies[i].transform.GetChild(0).GetComponent<Image>();
            if (i < playerStats.CurrentEnergy)
                imageComponent.sprite = filledEnergySprite;
            else
                imageComponent.sprite = emptyEnergySprite;
        }
    }


    public void SetPreviewedEnergy(int amount) {
        if (lastPreviewedEnergy == amount) {
            Debug.Log("avoided useless calculations");
            return;
		}

        lastPreviewedEnergy = amount;

        for (int i = 0; i < playerStats.CurrentEnergy; ++i) {
            Image imageComponent = energies[i].transform.GetChild(0).GetComponent<Image>();
            if (i < playerStats.CurrentEnergy - amount)
                imageComponent.sprite = filledEnergySprite;
            else
                imageComponent.sprite = previewedEnergySprite;
		}
	}
}
