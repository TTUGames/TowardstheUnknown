using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillsBar : MonoBehaviour
{
    public Sprite skillBackgroundSprite;
    public TextMeshProUGUI skillTextPrefab;
    public Image skillSpritePrefab;
    public float skillSize = 0.025f;
    public float spacing = 1f;
    
    private RectTransform skillsBarRectTransform;
    private Inventory inventory;

    private void Awake()
    {
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        skillsBarRectTransform = GetComponent<RectTransform>();
    }   
    
    /* public void Update()
    {
        skillsBarRectTransform.anchorMin = new Vector2(0.5f - skillSize * inventory.LArtifacts.Count * spacing, skillsBarRectTransform.anchorMin.y);
        skillsBarRectTransform.anchorMax = new Vector2(0.5f + skillSize * inventory.LArtifacts.Count * spacing, skillsBarRectTransform.anchorMax.y);
    } */

    public void UpdateSkillBar()
    {
        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);
        
        skillsBarRectTransform.anchorMin = new Vector2(0.5f - skillSize * inventory.LArtifacts.Count * spacing, skillsBarRectTransform.anchorMin.y);
        skillsBarRectTransform.anchorMax = new Vector2(0.5f + skillSize * inventory.LArtifacts.Count * spacing, skillsBarRectTransform.anchorMax.y);

        float previousAnchorMaxPoint = 0f;
        float anchorMaxXSize = (1f - previousAnchorMaxPoint) / inventory.LArtifacts.Count;
        for (int i = 0; i < inventory.LArtifacts.Count; i++)
        {
            //Creating the Skill borders
            GameObject skill = new GameObject();
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

            TextMeshProUGUI skillText = Instantiate(skillTextPrefab, skill.transform);
            skillText.text = "<font-weight=\"100\">" + inventory.LArtifacts[i].GetCost();

            skill.AddComponent<SkillClickHandler>();
            skill.GetComponent<SkillClickHandler>().ArtifactIndex = i;

            //Creating the sprite container of the Skill
            if (inventory.LArtifacts[i].GetIcon() != null)
            {
                Image skillSprite = Instantiate(skillSpritePrefab, skill.transform);
                skillSprite.name = "SkillSprite";
                skillSprite.sprite = inventory.LArtifacts[i].GetIcon();
                
                if (!inventory.LArtifacts[i].CanUse(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>()))
                    skillSprite.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                else
                    skillSprite.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}