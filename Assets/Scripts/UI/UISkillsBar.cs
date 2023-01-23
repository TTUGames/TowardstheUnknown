using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillsBar : MonoBehaviour
{
    public Sprite skillBackgroundSprite;
    public GameObject skillCostPrefab;
    public GameObject skillSpritePrefab;
    public float skillSize = 0.025f;
    public float spacing = 1f;

    private RectTransform skillsBarRectTransform;
    private InventoryManager inventory;

    private void Awake()
    {
        inventory = FindObjectOfType<InventoryManager>();
        skillsBarRectTransform = GetComponent<RectTransform>();
    }

    public void Update()
    {
        skillsBarRectTransform.anchorMin = new Vector2(0.5f - skillSize * inventory.GetPlayerArtifacts().Count * spacing, skillsBarRectTransform.anchorMin.y);
        skillsBarRectTransform.anchorMax = new Vector2(0.5f + skillSize * inventory.GetPlayerArtifacts().Count * spacing, skillsBarRectTransform.anchorMax.y);
    }

    public void UpdateSkillBar()
    {

        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);

        skillsBarRectTransform.anchorMin = new Vector2(0.5f - skillSize * inventory.GetPlayerArtifacts().Count * spacing, skillsBarRectTransform.anchorMin.y);
        skillsBarRectTransform.anchorMax = new Vector2(0.5f + skillSize * inventory.GetPlayerArtifacts().Count * spacing, skillsBarRectTransform.anchorMax.y);

        float previousAnchorMaxPoint = 0f;
        float anchorMaxXSize = (1f - previousAnchorMaxPoint) / inventory.GetPlayerArtifacts().Count;
        for (int i = 0; i < inventory.GetPlayerArtifacts().Count; i++)
        {
            //Creating the Skill borders
            GameObject skill = new GameObject();
            skill.layer = gameObject.layer;
            skill.transform.SetParent(transform);
            skill.name = i.ToString();

            RectTransform skillRectTransform = skill.AddComponent<RectTransform>();
            skillRectTransform.localScale = new Vector3(1, 1, 1);
            skillRectTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
            skillRectTransform.anchorMin = new Vector2(previousAnchorMaxPoint, 0);
            skillRectTransform.anchorMax = new Vector2(previousAnchorMaxPoint + anchorMaxXSize, 1f);
            previousAnchorMaxPoint = skillRectTransform.anchorMax.x;
            skillRectTransform.offsetMin = new Vector2(0, 0);
            skillRectTransform.offsetMax = new Vector2(0, 0);

            Image skillBackgroundImage = skill.AddComponent<Image>();
            skillBackgroundImage.preserveAspect = true;
            skillBackgroundImage.sprite = skillBackgroundSprite;

            GameObject skillCost = Instantiate(skillCostPrefab, skill.transform);
            skillCost.layer = gameObject.layer;

            TextMeshProUGUI skillText = skillCost.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            if (skillText != null)
                skillText.text = "<i><font-weight=\"700\">" + inventory.GetPlayerArtifacts()[i].GetCost();
            else
                Debug.LogError("No TextMeshProUGUI component of the child at index 1 in skillCost prefab");

            skill.AddComponent<SkillClickHandler>();
            skill.GetComponent<SkillClickHandler>().ArtifactIndex = i;

            //Creating the sprite container of the Skill
            if (inventory.GetPlayerArtifacts()[i].GetIcon() != null)
            {
                GameObject skillSprite = Instantiate(skillSpritePrefab, skill.transform);
                skillSprite.layer = gameObject.layer;

                Image skillSpriteImageComponent = skillSprite.GetComponent<Image>();
                if (skillSpriteImageComponent != null)
                    skillSpriteImageComponent.sprite = inventory.GetPlayerArtifacts()[i].GetIcon();
                else
                    Debug.LogError("No Image component in skillSprite prefab");

                if (!inventory.GetPlayerArtifacts()[i].CanUse(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>()))
                    skillSpriteImageComponent.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                else
                    skillSpriteImageComponent.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}