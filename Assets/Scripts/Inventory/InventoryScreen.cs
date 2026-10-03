using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// The inventory menu (Assets/UI/Menus/Inventory.uxml): the player's grid, the character sheet or a chest's grid,
/// and the info of the hovered artifact
/// </summary>
public class InventoryScreen : MonoBehaviour
{
    // The chest's pieces coming in one by one (EditionProfile.chestReveal), in milliseconds: after the panel's opening, then between two
    private const long RevealDelay = 200;
    private const long RevealInterval = 160;
    // The UI keys of the rarities' names, by ArtifactRarity
    private const float MinRarityBrightness = 0.8f;
    private static readonly string[] RarityKeys = { "RarityCommon", "RarityRare", "RarityEpic", "RarityLegendary" };
    private const string WarningShownClassName = "chest-warning--shown";
    private static readonly CustomStyleProperty<string> confirmDurationProperty = new("--confirm-duration");

    [SerializeField] private UIDocument document;
    [SerializeField] private ChangeUI changeUI;
    [SerializeField] private UISounds sounds;
    [SerializeField, Tooltip("The rarities' colors of the artifact pieces")] private RarityPalette rarityPalette;
    [SerializeField, Tooltip("The kill families counted in the character sheet, each in its label <name>Count (UI key PlayerProgress<name>Count)")]
    private List<EntityData> killFamilies;

    private VisualElement screen;
    private VisualElement playerInfoPanel;
    private VisualElement chestPanel;
    private VisualElement chestWarning;
    private Label gridCount;
    private IVisualElementScheduledItem warningEnd;
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
        chestWarning = screen.Q("ChestWarning");
        MenuScreen.Setup(screen, gameObject, sounds);
        PlayerInventory.Show(GameScene.Player.Inventory.Data);
        PlayerInventory.Bind(screen.Q("PlayerGrid"), rarityPalette);
        Chest.Bind(screen.Q("ChestGrid"), rarityPalette);
        drag = new InventoryDrag(screen, screen.Q("Hand"), OpenInventories, ShowDescription, gameObject, sounds);
        gridCount = screen.Q<Label>("GridCount");
        drag.HandChanged += RefreshGridCount;
        GameScene.Player.Inventory.Data.Changed += RefreshGridCount;
        RefreshGridCount();
        GameEvents.ChestOpened += OpenChest;
    }

    private void OnDestroy()
    {
        GameEvents.ChestOpened -= OpenChest;
        if (GameScene.Player != null) GameScene.Player.Inventory.Data.Changed -= RefreshGridCount;
    }

    /// <summary>
    /// The player's grid filling: its slots taken, with the piece in hand counted as a preview, in the accent once full
    /// </summary>
    private void RefreshGridCount()
    {
        TetrisInventoryData data = GameScene.Player.Inventory.Data;
        int used = 0;
        foreach (TetrisInventoryItem item in data.Items)
            used += item.RotatedSlots().Count;
        int held = drag.ItemInHand != null ? drag.ItemInHand.RotatedSlots().Count : 0;
        int total = data.gridSize.x * data.gridSize.y;
        gridCount.text = string.Format(Localization.UI("InventoryGridCount"), used + held, total);
        gridCount.EnableInClassList("grid-count--preview", held > 0);
        gridCount.EnableInClassList("grid-count--full", used + held >= total);
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
        bool open = !IsOpen;
        // Closing loses the artifacts left in the chest: a first closing warns, the second one closes
        if (!open && AskBeforeLosingChest()) return;
        HideChestWarning();
        if (open)
        {
            // Back to the character sheet on opening, not on closing: the chest stays shown while the screen fades out
            ShowChest(false);
            RefreshPlayerInfo();
            RefreshGridCount();
            // Until the player presses one, the info shows the first artifact; the original's stayed empty
            // An artifact shown from a chest and left in it is lost: back to the first one (the original kept showing it)
            IReadOnlyList<Artifact> artifacts = GameScene.Player.Inventory.Data.Artifacts;
            bool lost = shownArtifact != null && !Contains(artifacts, shownArtifact);
            if ((shownArtifact == null || lost) && artifacts.Count > 0 && Edition.Profile.prefillArtifactInfo) ShowDescription(artifacts[0]);
        }
        screen.EnableInClassList("open", open);
        (open ? sounds.inventoryOpen : sounds.inventoryClose).Post(gameObject);
        changeUI.NotifyMenuChanged();
    }

    /// <summary>
    /// With artifacts left in the open chest and no warning shown, shakes the chest's grid and shows the warning until its
    /// --confirm-duration ends: true if it did, and the inventory stays open (EditionProfile.confirmations)
    /// </summary>
    private bool AskBeforeLosingChest()
    {
        if (!Edition.Profile.confirmations || !IsChestOpen || Chest.Artifacts.Count == 0) return false;
        if (chestWarning.ClassListContains(WarningShownClassName)) return false;
        chestWarning.AddToClassList(WarningShownClassName);
        RefuseShake.Play(screen.Q("ChestGrid"), "inventory-grid--refused");
        if (Edition.Profile.refusalFeedback) sounds.artifactRefused.Post(gameObject);
        warningEnd?.Pause();
        warningEnd = chestWarning.schedule.Execute(HideChestWarning).StartingIn(chestWarning.customStyle.Milliseconds(confirmDurationProperty, 3000));
        return true;
    }

    private void HideChestWarning()
    {
        warningEnd?.Pause();
        chestWarning?.RemoveFromClassList(WarningShownClassName);
    }

    private static bool Contains(IReadOnlyList<Artifact> artifacts, Artifact artifact)
    {
        for (int i = 0; i < artifacts.Count; i++)
            if (artifacts[i] == artifact) return true;
        return false;
    }

    /// <summary>
    /// Opens the inventory with a chest's grid of <paramref name="artifacts"/>, to drag from; the artifacts left in it are lost once closed
    /// </summary>
    private void OpenChest(IReadOnlyList<Artifact> artifacts)
    {
        if (!IsOpen) Toggle();
        ShowChest(true);
        Chest.Show(TetrisInventoryData.FromArtifacts(artifacts));
        if (Edition.Profile.chestReveal) Chest.Reveal(RevealDelay, RevealInterval, _ => sounds.artifactDrop.Post(gameObject));
    }

    // Shows the chest's grid instead of the character sheet
    private void ShowChest(bool open)
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
        Label rarity = description.Q<Label>("ArtifactRarity");
        rarity.text = Localization.UI(RarityKeys[(int)artifact.Data.rarity]);
        // The common accent, a dark grey, is brightened to read on the panel
        Color.RGBToHSV(rarityPalette.Get(artifact.Data.rarity, RarityPalette.Tone.Accent), out float hue, out float saturation, out float value);
        rarity.style.color = Color.HSVToRGB(hue, saturation, Mathf.Max(value, MinRarityBrightness));
        Label effects = description.Q<Label>("ArtifactEffects");
        effects.text = RichText.Highlight(effects, artifact.EffectDescription + "\n" + artifact.RangeDescription + "\n" + artifact.CooldownDescription);
        description.Q<CostTag>("ArtifactCost").value = artifact.Data.cost;
        description.Q<Label>("ArtifactCooldown").text = artifact.Data.cooldown.ToString();
        // An artifact without cooldown hides it (its text tells its uses per turn instead)
        description.Q(className: "artifact-info__cooldown").EnableInClassList("artifact-info__cooldown--none", artifact.Data.cooldown == 0);
        description.Q("ArtifactIcon").style.backgroundImage = artifact.Data.skillBarIcon != null ? new StyleBackground(artifact.Data.skillBarIcon) : StyleKeyword.Null;
    }

    private void RefreshPlayerInfo()
    {
        RunStats run = GameScene.Run;
        PlayerStats stats = GameScene.Player.Stats;
        void Set(string name, string text) => playerInfoPanel.Q<Label>(name).text = text;
        Set("PlayerName", run.PlayerName);
        bool readable = Edition.Profile.readableStats;
        Set("StatsHealth", !readable ? string.Format(Localization.UI("PlayerStatsHP"), stats.CurrentHealth, stats.Armor, stats.MaxHealth)
            : string.Format(Localization.UI(stats.Armor > 0 ? "PlayerStatsHealthArmor" : "PlayerStatsHealth"), stats.CurrentHealth, stats.MaxHealth, stats.Armor));
        Set("StatsEnergy", string.Format(Localization.UI("PlayerStatsEnergy"), stats.CurrentEnergy, stats.MaxEnergy));
        Set("StatsAttack", string.Format(Localization.UI("PlayerStatsAttack"), Mathf.RoundToInt((stats.DamageDealtMultiplier - 1) * 100)));
        Set("StatsDefense", string.Format(Localization.UI("PlayerStatsDefense"), Mathf.RoundToInt((1 - stats.DamageReceivedMultiplier) * 100)));
        foreach (EntityData family in killFamilies)
            Set(family.ID + "Count", string.Format(Localization.UI($"PlayerProgress{family.ID}Count"), run.KillsOf(family.ID)));
        Set("VisitedRooms", string.Format(Localization.UI("PlayerProgressVisitedRoom"), run.VisitedRoomCount));
        Set("Score", string.Format(Localization.UI("PlayerProgressScore"), readable ? Localization.Number(run.Score) : run.Score.ToString().PadLeft(6, '0')));

        //The antechamber and the boss room are Drareg's garden, the rest of the Rift is the absolute zero
        //The test scenes load a single room, without a map
        Room room = GameScene.Map != null ? GameScene.Map.CurrentRoom : null;
        bool inGarden = room != null && (room.type == RoomType.ANTECHAMBER || room.type == RoomType.BOSS);
        playerInfoPanel.Q<LocalizedLabel>("ZoneTitle").key = inGarden ? "ZoneInfoGardenHeader" : "ZoneInfoZeroHeader";
        playerInfoPanel.Q<LocalizedLabel>("ZoneText").key = inGarden ? "ZoneInfoGardenContent" : "ZoneInfoZeroContent";
    }
}
