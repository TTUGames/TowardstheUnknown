using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public GameObject miniMap;
    public UIPause uIPause;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private GameObject chestInventory;
    [SerializeField] private EntityInfoPanel entityInfoPanel;
    [SerializeField] private Hud hud;

    public bool IsInventoryOpened => inventoryMenu.activeSelf;

    public PlayerInfo PlayerInfo => GetComponent<PlayerInfo>();
    public UIFade Fade => GetComponent<UIFade>();
    public Minimap Minimap => miniMap.GetComponent<Minimap>();
    public EntityInfoPanel EntityInfoPanel => entityInfoPanel;
    public Hud Hud => hud;
    public TetrisInventory Chest => chestInventory.GetComponent<TetrisInventory>();
    private Results Results => GetComponent<Results>();

    private void OnEnable()
    {
        GameInput.Controls.Menus.ToggleInventory.performed += OnToggleInventory;
        GameInput.Controls.Menus.Back.performed += OnBack;
    }

    private void OnDisable()
    {
        GameInput.Controls.Menus.ToggleInventory.performed -= OnToggleInventory;
        GameInput.Controls.Menus.Back.performed -= OnBack;
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (!uIPause.isPaused && !Results.IsShown)
            ChangeStateInventory();
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        PlayerTurn player = GameScene.Player;
        if (player != null && player.Stats.CurrentHealth > 0 && !Results.IsShown)
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
            PlayerInfo.UpdatePlayerInfo();
            inventoryMenu.SetActive(true);
            PlayerInventory.Open();
        }
        else
        {
            PlayerInventory.Close();
            Chest.Close();
            inventoryMenu.SetActive(false);
        }
        miniMap.SetActive(!open && !uIPause.isPaused);
        AkUnitySoundEngine.PostEvent(open ? "OpenInventory" : "CloseInventory", gameObject);
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
    public bool IsMenuOpen => uIPause.isPaused || inventoryMenu.activeInHierarchy || Results.IsShown;

    public void OpenChestInterface(bool open)
    {
        playerInfo.SetActive(!open);
        chestInventory.SetActive(open);
    }
}
