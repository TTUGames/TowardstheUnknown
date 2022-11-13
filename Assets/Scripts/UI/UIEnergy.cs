using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergy : MonoBehaviour
{
    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
    }

    private void Update()
    {
        UpdateEnergyUI();
    }

    public void UpdateEnergyUI()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        float previousAnchorMaxPoint = 0f;
        float anchorMaxXSize = (1f - previousAnchorMaxPoint) / playerStats.MaxEnergy;

        for (int i = 0; i < playerStats.MaxEnergy; i++)
        {
            GameObject energy = new GameObject();
            energy.transform.SetParent(transform);
            energy.name = i.ToString();
            energy.AddComponent<RectTransform>();

            energy.AddComponent<Image>();

            energy.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            energy.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
            energy.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.25f, 0.5f);
            energy.GetComponent<RectTransform>().anchorMin = new Vector2(previousAnchorMaxPoint, 0);
            energy.GetComponent<RectTransform>().anchorMax = new Vector2(previousAnchorMaxPoint + anchorMaxXSize, 1f);
            previousAnchorMaxPoint = energy.GetComponent<RectTransform>().anchorMax.x;
            energy.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            energy.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

            if (i < playerStats.CurrentEnergy)
                energy.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/Energy");
            else
                energy.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/Empty Energy");
        }
    }
}
