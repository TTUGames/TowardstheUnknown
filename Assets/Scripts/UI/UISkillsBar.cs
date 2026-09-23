using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillsBar : MonoBehaviour
{
    private static readonly Color unusableColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private const string textStyle = "<i><font-weight=\"700\">";

    public Sprite skillBackgroundSprite;
    public GameObject skillCostPrefab;
    public GameObject skillSpritePrefab;
    public GameObject skillCooldownPrefab;
    public float skillSize = 0.025f;
    public float spacing = 1f;

    private RectTransform skillsBarRectTransform;
    private InventoryManager inventory;
    private PlayerStats playerStats;

    private void Awake()
    {
        inventory = FindAnyObjectByType<InventoryManager>();
        skillsBarRectTransform = GetComponent<RectTransform>();
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
    }

    public void UpdateSkillBar()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        List<Artifact> artifacts = inventory.GetPlayerArtifacts();
        int count = artifacts.Count;
        if (count <= 2) spacing = .02f;
        else if (count <= 3) spacing = .05f;
        else if (count <= 4) spacing = .1f;
        else if (count <= 5) spacing = .3f;
        else if (count <= 6) spacing = .5f;

        float halfWidth = skillSize * count * spacing;
        skillsBarRectTransform.anchorMin = new Vector2(0.5f - halfWidth, skillsBarRectTransform.anchorMin.y);
        skillsBarRectTransform.anchorMax = new Vector2(0.5f + halfWidth, skillsBarRectTransform.anchorMax.y);

        float anchorXSize = 1f / count;
        for (int i = 0; i < count; i++)
        {
            Artifact artifact = artifacts[i];

            //Creating the Skill borders
            GameObject skill = new GameObject(i.ToString(), typeof(RectTransform));
            skill.layer = gameObject.layer;
            skill.transform.SetParent(transform);

            RectTransform skillRectTransform = (RectTransform)skill.transform;
            skillRectTransform.localScale = Vector3.one;
            skillRectTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
            skillRectTransform.anchorMin = new Vector2(i * anchorXSize, 0);
            skillRectTransform.anchorMax = new Vector2((i + 1) * anchorXSize, 1f);
            skillRectTransform.offsetMin = Vector2.zero;
            skillRectTransform.offsetMax = Vector2.zero;

            Image skillBackgroundImage = skill.AddComponent<Image>();
            skillBackgroundImage.preserveAspect = true;
            skillBackgroundImage.sprite = skillBackgroundSprite;

            // SkillCost
            GameObject skillCost = Instantiate(skillCostPrefab, skill.transform);
            skillCost.layer = gameObject.layer;
            skillCost.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = textStyle + artifact.Cost;

            skill.AddComponent<SkillClickHandler>().artifactIndex = i;

            //Creating the sprite container of the Skill
            if (artifact.SkillBarIcon != null)
            {
                GameObject skillSprite = Instantiate(skillSpritePrefab, skill.transform);
                skillSprite.layer = gameObject.layer;

                Image skillSpriteImage = skillSprite.GetComponent<Image>();
                skillSpriteImage.sprite = artifact.SkillBarIcon;
                skillSpriteImage.color = artifact.CanUse(playerStats) ? Color.white : unusableColor;
            }

            // SkillCooldown
            GameObject skillCooldown = Instantiate(skillCooldownPrefab, skill.transform);
            skillCooldown.layer = gameObject.layer;
            skillCooldown.transform.SetAsLastSibling();
            skillCooldown.GetComponent<TextMeshProUGUI>().text = textStyle + (artifact.RemainingCooldown == 0 ? "" : artifact.RemainingCooldown.ToString());
        }
    }
}
