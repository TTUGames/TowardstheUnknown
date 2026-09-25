using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// The inventory menu (Assets/UI/Menus/Inventory.uxml): the player's grid, the character sheet or a chest's grid,
/// and the info of the last pressed artifact
/// </summary>
public class InventoryScreen : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private ChangeUI changeUI;

    private VisualElement screen;
    private VisualElement playerInfoPanel;
    private VisualElement chestPanel;
    private InventoryDrag drag;
    private Artifact shownArtifact;

    // Filled from the Start of the inventory manager and of the collectables, before or after the screen is built
    public TetrisInventory PlayerInventory { get; } = new();
    public TetrisInventory Chest { get; } = new();

    public bool IsOpen => screen != null && screen.ClassListContains("open");
    private bool IsChestOpen => chestPanel != null && !chestPanel.ClassListContains("hidden");

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Inventory");
        playerInfoPanel = screen.Q("PlayerInfo");
        chestPanel = screen.Q("Chest");
        MenuScreen.Setup(screen, gameObject);
        PlayerInventory.Bind(screen.Q("PlayerGrid"));
        Chest.Bind(screen.Q("ChestGrid"));
        drag = new InventoryDrag(screen, screen.Q("Hand"), OpenInventories, ShowDescription, gameObject);
    }

    private void OnEnable()
    {
        GameInput.Controls.Inventory.Rotate.performed += OnRotate;
    }

    private void OnDisable()
    {
        GameInput.Controls.Inventory.Rotate.performed -= OnRotate;
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        if (IsOpen) drag.Rotate();
    }

    private IEnumerable<TetrisInventory> OpenInventories()
    {
        yield return PlayerInventory;
        if (IsChestOpen) yield return Chest;
    }

    public void Toggle()
    {
        drag.CancelDrag();
        OpenChest(false);
        bool open = !IsOpen;
        if (open)
        {
            RefreshPlayerInfo();
            // Until the player presses one, the info shows the first artifact
            List<Artifact> artifacts = PlayerInventory.GetInventoryData().GetArtifacts();
            if (shownArtifact == null && artifacts.Count > 0) ShowDescription(artifacts[0]);
        }
        screen.EnableInClassList("open", open);
        changeUI.Hud.Minimap.SetVisible(!open && !changeUI.uIPause.isPaused);
        AkUnitySoundEngine.PostEvent(open ? "OpenInventory" : "CloseInventory", gameObject);
    }

    /// <summary>
    /// Shows the chest's grid instead of the character sheet
    /// </summary>
    public void OpenChest(bool open)
    {
        playerInfoPanel.EnableInClassList("hidden", open);
        chestPanel.EnableInClassList("hidden", !open);
    }

    /// <summary>
    /// Shows the info of an artifact
    /// </summary>
    public void ShowDescription(Artifact artifact)
    {
        shownArtifact = artifact;
        VisualElement description = screen.Q("Description");
        description.Q<Label>("ArtifactTitle").text = artifact.Title;
        description.Q<Label>("ArtifactText").text = artifact.Description;
        description.Q<Label>("ArtifactEffects").text = artifact.EffectDescription + "\n" + artifact.RangeDescription + "\n" + artifact.CooldownDescription;
        description.Q<CostTag>("ArtifactCost").value = artifact.Cost;
        description.Q<Label>("ArtifactCooldown").text = Mathf.Max(0, artifact.Cooldown - 1).ToString();
        description.Q("ArtifactIcon").style.backgroundImage = artifact.SkillBarIcon != null ? new StyleBackground(artifact.SkillBarIcon) : StyleKeyword.Null;
    }

    private void RefreshPlayerInfo()
    {
        PlayerInfo info = changeUI.PlayerInfo;
        PlayerStats stats = GameScene.Player.Stats;
        void Set(string name, string text) => playerInfoPanel.Q<Label>(name).text = text;
        Set("PlayerName", info.PlayerName);
        Set("StatsHealth", string.Format(Localization.UI("PlayerStatsHP"), stats.CurrentHealth, stats.Armor, stats.MaxHealth));
        Set("StatsEnergy", string.Format(Localization.UI("PlayerStatsEnergy"), stats.CurrentEnergy, stats.MaxEnergy));
        Set("StatsAttack", string.Format(Localization.UI("PlayerStatsAttack"), Mathf.RoundToInt((stats.DamageDealtMultiplier - 1) * 100)));
        Set("StatsDefense", string.Format(Localization.UI("PlayerStatsDefense"), Mathf.RoundToInt((1 - stats.DamageReceivedMultiplier) * 100)));
        Set("KameikoCount", string.Format(Localization.UI("PlayerProgressKameikoCount"), info.kameikoKilled));
        Set("NanukoCount", string.Format(Localization.UI("PlayerProgressNanukoCount"), info.nanukoKilled));
        Set("GolemCount", string.Format(Localization.UI("PlayerProgressGolemCount"), info.golemKilled));
        Set("VisitedRooms", string.Format(Localization.UI("PlayerProgressVisitedRoom"), info.visitedRoomCount));
        Set("Score", string.Format(Localization.UI("PlayerProgressScore"), info.score.ToString().PadLeft(6, '0')));
    }
}
