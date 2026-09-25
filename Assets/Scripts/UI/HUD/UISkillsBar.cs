using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillsBar : MonoBehaviour
{
    private static readonly Color unusableColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private static readonly Color selectedSkillColor = new Color32(116, 89, 216, 255);
    private const string textStyle = "<i><font-weight=\"700\">";

    public Sprite skillBackgroundSprite;
    public GameObject skillCostPrefab;
    public GameObject skillSpritePrefab;
    public GameObject skillCooldownPrefab;
    public float skillSize = 0.025f;
    public float spacing = 1f;
    [SerializeField, Tooltip("Shows the effects of the hovered skill")] private GameObject tooltipContainer;

    private RectTransform skillsBarRectTransform;
    private PlayerTurn playerTurn;
    private InventoryManager inventory;
    private PlayerStats playerStats;
    private readonly List<Image> skillImages = new List<Image>();

    private void Awake()
    {
        skillsBarRectTransform = GetComponent<RectTransform>();
        playerTurn = GameScene.Player;
        inventory = playerTurn.Inventory;
        playerStats = playerTurn.Stats;
    }

    //The inventory fills the bar on its first update
    private void OnEnable()
    {
        playerStats.EnergyChanged += UpdateSkillBar;
        inventory.ArtifactsChanged += UpdateSkillBar;
        playerTurn.SelectedArtifactChanged += HighlightSelectedSkill;
    }

    private void OnDisable()
    {
        playerStats.EnergyChanged -= UpdateSkillBar;
        inventory.ArtifactsChanged -= UpdateSkillBar;
        playerTurn.SelectedArtifactChanged -= HighlightSelectedSkill;
    }

    /// <summary>
    /// Highlights the skill of the artifact the player attacks with, none if its index is -1
    /// </summary>
    private void HighlightSelectedSkill(int artifactIndex)
    {
        for (int i = 0; i < skillImages.Count; i++)
            skillImages[i].color = i == artifactIndex ? selectedSkillColor : Color.white;
    }

    private void UpdateSkillBar()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        skillImages.Clear();

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
            skillRectTransform.anchorMin = new Vector2(i * anchorXSize, 0);
            skillRectTransform.anchorMax = new Vector2((i + 1) * anchorXSize, 1f);
            skillRectTransform.offsetMin = Vector2.zero;
            skillRectTransform.offsetMax = Vector2.zero;

            Image skillBackgroundImage = skill.AddComponent<Image>();
            skillBackgroundImage.preserveAspect = true;
            skillBackgroundImage.sprite = skillBackgroundSprite;
            skillImages.Add(skillBackgroundImage);

            // SkillCost
            GameObject skillCost = Instantiate(skillCostPrefab, skill.transform);
            skillCost.layer = gameObject.layer;
            skillCost.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = textStyle + artifact.Cost;

            skill.AddComponent<SkillClickHandler>().Init(playerTurn, i, tooltipContainer);

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
