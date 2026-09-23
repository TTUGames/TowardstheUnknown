using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ChangeUI : MonoBehaviour
{
    [Header("Item Description")]
    [SerializeField] private Image infoImage;
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoBody;
    [SerializeField] private TMP_Text effectBody;
    [SerializeField] private TMP_Text costBody;
    [SerializeField] private TMP_Text cooldownBody;

    [Header("Global")]
    public TetrisInventory PlayerInventory;
    public InventoryManager inventoryManager;
    public GameObject miniMap;
    public GameObject pauseMenu;
    public UIPause uIPause;
    private PlayerStats playerStats;
    private PlayerInfo scriptPlayerInfo;
    public bool uIIsOpen;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private GameObject chestInventory;
    [SerializeField] private GameObject resultsCanvas;

    private void Start()
    {
        scriptPlayerInfo = GetComponent<PlayerInfo>();
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        uIIsOpen = false;
    }

    public bool IsInventoryOpened => inventoryMenu.activeSelf;

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab)) && !uIPause.isPaused && !resultsCanvas.activeSelf)
            ChangeStateInventory();
        else if (Input.GetKeyDown(KeyCode.Escape) && playerStats.currentHealth > 0 && !resultsCanvas.activeSelf)
            uIPause.ChangeStateOptions();
    }

    public void ChangeStateInventory()
    {
        inventoryMenu.GetComponent<TetrisInventoryMove>().CancelDrag();
        OpenChestInterface(false);
        bool open = !IsInventoryOpened;
        //The inventories must be (de)activated while the menu is active
        if (open)
        {
            scriptPlayerInfo.UpdatePlayerInfo();
            inventoryMenu.SetActive(true);
            PlayerInventory.Open();
        }
        else
        {
            PlayerInventory.Close();
            inventoryManager.chest.Close();
            inventoryMenu.SetActive(false);
        }
        miniMap.SetActive(!open && !pauseMenu.activeSelf);
        AkUnitySoundEngine.PostEvent(open ? "OpenInventory" : "CloseInventory", gameObject);
        UIInformation();
        ChangeBlur();
        foreach (Transform child in transform.GetChild(0))
            if (child.name == "BackPanel")
                child.gameObject.SetActive(open);
    }

    /// <summary>
    /// Displays the artifact's information in the inventory
    /// </summary>
    public void ChangeDescription(Artifact artifact)
    {
        infoTitle.text = artifact.Title;
        infoBody.text = artifact.Description;
        effectBody.text = artifact.EffectDescription + "\n" + artifact.RangeDescription + "\n" + artifact.CooldownDescription;
        costBody.text = artifact.Cost.ToString();
        cooldownBody.text = Mathf.Max(0, artifact.Cooldown - 1).ToString();
        infoImage.sprite = artifact.SkillBarIcon;
        infoImage.color = artifact.SkillBarIcon != null ? Color.white : Color.clear;
    }

    /// <summary>
    /// Checks if a menu covering the game is open
    /// </summary>
    public void UIInformation()
    {
        uIIsOpen = IsMenuOpen;
    }

    public bool IsMenuOpen => uIPause.isPaused || inventoryMenu.activeInHierarchy || resultsCanvas.activeInHierarchy;

    /// <summary>
    /// Blurs the game when a menu is open
    /// </summary>
    public void ChangeBlur()
    {
        Volume volume = Camera.main.GetComponent<Volume>();
        if (volume.profile.TryGet(out DepthOfField dof))
            dof.active = uIIsOpen;
    }

    public void OpenChestInterface(bool open)
    {
        playerInfo.SetActive(!open);
        chestInventory.SetActive(open);
    }
}
