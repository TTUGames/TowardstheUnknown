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
    [SerializeField] private UISounds sounds;
    [SerializeField, Tooltip("The rarities' colors of the artifact pieces")] private RarityPalette rarityPalette;

    private VisualElement screen;
    private VisualElement playerInfoPanel;
    private VisualElement chestPanel;
    private InventoryDrag drag;
    private Artifact shownArtifact;

    // The player's grid, and the grid of the collectable picked up
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
        MenuScreen.Setup(screen, gameObject, sounds);
        PlayerInventory.Show(GameScene.Player.Inventory.Data);
        PlayerInventory.Bind(screen.Q("PlayerGrid"), rarityPalette);
        Chest.Bind(screen.Q("ChestGrid"), rarityPalette);
        drag = new InventoryDrag(screen, screen.Q("Hand"), OpenInventories, ShowDescription, gameObject, sounds);
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
            IReadOnlyList<Artifact> artifacts = GameScene.Player.Inventory.Data.Artifacts;
            if (shownArtifact == null && artifacts.Count > 0) ShowDescription(artifacts[0]);
        }
        screen.EnableInClassList("open", open);
        changeUI.Hud.Minimap.SetVisible(!open && !changeUI.uIPause.isPaused);
        (open ? sounds.inventoryOpen : sounds.inventoryClose).Post(gameObject);
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
        RunStats run = GameScene.Run;
        PlayerStats stats = GameScene.Player.Stats;
        void Set(string name, string text) => playerInfoPanel.Q<Label>(name).text = text;
        Set("PlayerName", run.PlayerName);
        Set("StatsHealth", string.Format(Localization.UI("PlayerStatsHP"), stats.CurrentHealth, stats.Armor, stats.MaxHealth));
        Set("StatsEnergy", string.Format(Localization.UI("PlayerStatsEnergy"), stats.CurrentEnergy, stats.MaxEnergy));
        Set("StatsAttack", string.Format(Localization.UI("PlayerStatsAttack"), Mathf.RoundToInt((stats.DamageDealtMultiplier - 1) * 100)));
        Set("StatsDefense", string.Format(Localization.UI("PlayerStatsDefense"), Mathf.RoundToInt((1 - stats.DamageReceivedMultiplier) * 100)));
        Set("KameikoCount", string.Format(Localization.UI("PlayerProgressKameikoCount"), run.KillsOf("Kameiko")));
        Set("NanukoCount", string.Format(Localization.UI("PlayerProgressNanukoCount"), run.KillsOf("Nanuko")));
        Set("GolemCount", string.Format(Localization.UI("PlayerProgressGolemCount"), run.KillsOf("Golem")));
        Set("VisitedRooms", string.Format(Localization.UI("PlayerProgressVisitedRoom"), run.VisitedRoomCount));
        Set("Score", string.Format(Localization.UI("PlayerProgressScore"), run.Score.ToString().PadLeft(6, '0')));

        //The antechamber and the boss room are Drareg's garden, the rest of the Rift is the absolute zero
        //The test scenes load a single room, without a map
        Room room = GameScene.Map != null ? GameScene.Map.CurrentRoom : null;
        bool inGarden = room != null && (room.type == RoomType.ANTECHAMBER || room.type == RoomType.BOSS);
        playerInfoPanel.Q<LocalizedLabel>("ZoneTitle").key = inGarden ? "ZoneInfoGardenHeader" : "ZoneInfoZeroHeader";
        playerInfoPanel.Q<LocalizedLabel>("ZoneText").key = inGarden ? "ZoneInfoGardenContent" : "ZoneInfoZeroContent";
    }
}
