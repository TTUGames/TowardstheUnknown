using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// The in game HUD (Assets/UI/Hud/Hud.uxml): player status and status effects, action button, timeline, skills bar,
/// bag button, hovered enemy info, combat popups, combat banners, boss health bar, minimap and screen fade
/// </summary>
public class Hud : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private ChangeUI changeUI;
    [SerializeField] private UISounds sounds;

    private const string EndTurnKey = "EndTurnButton";
    private const string DeployKey = "DeployButton";

    private SlantedButton actionButton;
    private string actionTextKey = "ExplorationButton";
    private Action action;
    private SecondClick endTurnConfirm;
    private IVisualElementScheduledItem endTurnBeat;
    private PlayerTurn player;
    // Between two beats of the end turn button (UssTime), set on it by Hud.uss
    private static readonly CustomStyleProperty<string> beatIntervalProperty = new("--beat-interval");
    // How long the button pulses when the player's turn starts (UssTime), set on it by Hud.uss
    private static readonly CustomStyleProperty<string> pulseDurationProperty = new("--pulse-duration");
    private TimelinePanel timeline;
    // The panels, disposed with the HUD
    private readonly List<IDisposable> panels = new();
    // The skills', status effects' and stats' tooltips (hovered stats and timeline items)
    private HudTooltip[] tooltips;

    // Used by the map from its Awake, before the HUD is built
    public MinimapPanel Minimap { get; } = new();
    private SlantedWipe fade;
    /// <summary>
    /// The room transition's wipe, in the 16:9 area. Read from the document, which the map can use before the HUD's Start
    /// </summary>
    public SlantedWipe Fade => fade ??= document.rootVisualElement.Q<SlantedWipe>("Fade");

    /// <summary>
    /// The shield and heal colors of the UI, for the feedbacks in the world (<c>--world-shield-color</c>, <c>--world-heal-color</c>
    /// of <c>.hud</c>, set from the tokens)
    /// </summary>
    public static readonly CustomStyleProperty<Color> ShieldColor = new("--world-shield-color");
    public static readonly CustomStyleProperty<Color> HealColor = new("--world-heal-color");

    /// <summary>
    /// A color the HUD's style gives the world (<see cref="ShieldColor"/>, <see cref="HealColor"/>): false before the HUD is styled
    /// </summary>
    public bool TryGetColor(CustomStyleProperty<Color> property, out Color color)
    {
        color = default;
        VisualElement hud = document != null && document.rootVisualElement != null ? document.rootVisualElement.Q(className: "hud") : null;
        return hud != null && hud.customStyle.TryGetValue(property, out color);
    }

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        // The original's HUD buttons were silent
        MenuScreen.Setup(root, gameObject, sounds, originalSounds: false);

        player = GameScene.Player;
        var hoverTooltip = root.Q<HudTooltip>("HoverTooltip");
        var skillTooltip = root.Q<HudTooltip>("Tooltip");
        var statusTooltip = root.Q<HudTooltip>("StatusTooltip");
        tooltips = new[] { hoverTooltip, skillTooltip, statusTooltip };
        panels.AddRange(new IDisposable[] {
            new StatusPanel(root.Q("Status"), hoverTooltip, player.Stats, player.playerAttack),
            timeline = new TimelinePanel(root.Q("Timeline"), hoverTooltip, sounds.timelineHover),
            new SkillsBar(root.Q("Skills"), skillTooltip, player, gameObject, sounds),
            new RefusalSounds(player, gameObject, sounds),
            new StatusEffectsPanel(root.Q("StatusEffects"), statusTooltip, player.Stats),
            new LowHealthPanel(root.Q("LowHealth"), player.Stats),
            new CombatPopups(root.Q("Popups")),
            new BannerPanel(root.Q<SlantedLabel>("Banner"), gameObject, sounds),
            new BossBar(root.Q("BossBar"), player.playerAttack),
            new DamagePreview(root.Q("Popups"), player.playerAttack),
            new QueuedCastMarkers(root.Q("Popups"), player.playerAttack),
            new EntityInfoPanel(root.Q("EntityInfo")),
        });
        Minimap.Bind(root.Q("Minimap"));

        actionButton = root.Q<SlantedButton>("Action");
        endTurnConfirm = new SecondClick(actionButton, "EndTurnConfirm");
        actionButton.clicked += OnAction;
        TurnSystem.Instance.TurnChanged += RefreshActionButton;
        player.Stats.EnergyChanged += RefreshEndTurnBeat;
        Edition.Changed += OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        RefreshActionButton();
        root.Q<Button>("Bag").clicked += changeUI.Inventory.Toggle;
        Fade.Covering += OnWipeCovering;
        Fade.Revealing += OnWipeRevealing;
    }

    private void OnEnable()
    {
        GameEvents.CombatStarted += EnterCombatState;
        GameEvents.ExplorationStarted += EnterExplorationState;
        GameEvents.DeployChoiceShown += EnterDeployState;
        GameInput.Controls.Gameplay.EndTurn.performed += OnActionKey;
        changeUI.MenuChanged += OnMenuChanged;
        BoardPointer.TileHovered += OnTileHovered;
        BoardPointer.TileClicked += OnTileClicked;
    }

    private void OnDisable()
    {
        GameEvents.CombatStarted -= EnterCombatState;
        GameEvents.ExplorationStarted -= EnterExplorationState;
        GameEvents.DeployChoiceShown -= EnterDeployState;
        GameInput.Controls.Gameplay.EndTurn.performed -= OnActionKey;
        changeUI.MenuChanged -= OnMenuChanged;
        BoardPointer.TileHovered -= OnTileHovered;
        BoardPointer.TileClicked -= OnTileClicked;
    }

    /// <summary>
    /// A menu covering the HUD hides the minimap and the tooltips, which ignore the pointer until it closes
    /// </summary>
    private void OnMenuChanged()
    {
        Minimap.SetVisible(!changeUI.IsMenuOpen);
        if (tooltips == null) return;
        foreach (HudTooltip tooltip in tooltips)
            tooltip.Blocked = changeUI.IsMenuOpen;
    }

    // The key presses the action button: end of turn, deployment
    private void OnActionKey(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (actionButton != null && actionButton.enabledInHierarchy && !GameScene.IsGameplayBlocked && Edition.Profile.endTurnKey) OnAction();
    }

    /// <summary>
    /// Ending the turn while an artifact can still be cast asks for a second press
    /// </summary>
    private void OnAction()
    {
        if (action == null) return;
        if (actionTextKey == EndTurnKey && Edition.Profile.confirmations && !endTurnConfirm.Pending && CanStillCast())
        {
            endTurnConfirm.Ask();
            return;
        }
        endTurnConfirm.Cancel();
        action();
    }

    // The deploy phase's tiles: the original's were silent
    private void OnTileHovered(Tile tile)
    {
        if (actionTextKey == DeployKey && tile != null && tile.Selection == Tile.SelectionType.DEPLOY) PlayExtraSound(sounds.deployHover);
    }

    private void OnTileClicked(Tile tile)
    {
        if (actionTextKey == DeployKey && tile != null && tile.Selection == Tile.SelectionType.DEPLOY) PlayExtraSound(sounds.deploySelect);
    }

    // The room changes' wipe, silent in the original
    private void OnWipeCovering() => PlayExtraSound(sounds.wipeCover);

    private void OnWipeRevealing() => PlayExtraSound(sounds.wipeReveal);

    /// <summary>
    /// The sounds the original didn't have, in an edition with <see cref="EditionProfile.extraUISounds"/>
    /// </summary>
    private void PlayExtraSound(AK.Wwise.Event sound)
    {
        if (Edition.Profile.extraUISounds) sound.Post(gameObject);
    }

    private static bool CanStillCast()
    {
        PlayerTurn player = GameScene.Player;
        foreach (Artifact artifact in player.Inventory.GetPlayerArtifacts())
            if (artifact.CanUse(player.Stats)) return true;
        return false;
    }

    private void OnDestroy()
    {
        //The turn system may be destroyed first when the scene unloads
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= RefreshActionButton;
        if (player != null) player.Stats.EnergyChanged -= RefreshEndTurnBeat;
        Edition.Changed -= OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        Minimap.Unbind();
        if (fade != null)
        {
            fade.Covering -= OnWipeCovering;
            fade.Revealing -= OnWipeRevealing;
        }
        foreach (IDisposable panel in panels) panel.Dispose();
    }

    /// <summary>
    /// In combat, the button ends the player's turn
    /// </summary>
    private void EnterCombatState() => SetAction(EndTurnKey, TurnSystem.Instance.EndPlayerTurn);

    private void EnterExplorationState() => SetAction("ExplorationButton", null);

    /// <summary>
    /// Enters the deploy state, the button calling <paramref name="endDeploy"/>
    /// </summary>
    private void EnterDeployState(CombatPlayerDeploy deploy) => SetAction(DeployKey, deploy.EndDeployPhase);

    /// <summary>
    /// Rewrites the texts built by code in the new language (the keyed texts follow by themselves)
    /// </summary>
    private void OnLocaleChanged(Locale locale) => timeline.Refresh();

    // Gameplay can set the state before the HUD is built
    private void SetAction(string textKey, Action onClick)
    {
        actionTextKey = textKey;
        action = onClick;
        RefreshActionButton();
    }

    private void RefreshActionButton()
    {
        if (actionButton == null) return;
        endTurnConfirm.Cancel();
        if (actionButton.key != actionTextKey) actionButton.key = actionTextKey;
        // During the enemies' turns, the button waits for the player's turn
        TurnSystem turnSystem = TurnSystem.Instance;
        bool waiting = turnSystem.IsCombat && !turnSystem.IsPlayerTurn;
        // The button pulses when the player's turn starts
        if (turnSystem.IsCombat && !waiting && (actionButton.ClassListContains("waiting") || !actionButton.enabledSelf))
        {
            actionButton.AddToClassList("pulse");
            actionButton.schedule.Execute(() => actionButton.RemoveFromClassList("pulse")).StartingIn(actionButton.customStyle.Milliseconds(pulseDurationProperty, 0));
        }
        actionButton.EnableInClassList("waiting", waiting);
        actionButton.SetEnabled(action != null && !waiting);
        RefreshEndTurnBeat();
    }

    private void OnEditionChanged(GameEdition edition)
    {
        RefreshEndTurnBeat();
        Minimap.Redraw();
    }

    /// <summary>
    /// The end turn button beats once the player has no energy left to move or cast, until the turn ends
    /// </summary>
    private void RefreshEndTurnBeat()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        bool suggested = Edition.Profile.endTurnBeat && actionTextKey == EndTurnKey && turnSystem.IsCombat && turnSystem.IsPlayerTurn && player.Stats.CurrentEnergy <= 0;
        if (suggested) endTurnBeat ??= actionButton.schedule.Execute(() => actionButton.ToggleInClassList("beat")).Every(actionButton.customStyle.Milliseconds(beatIntervalProperty, 0));
        else
        {
            endTurnBeat?.Pause();
            endTurnBeat = null;
            actionButton.RemoveFromClassList("beat");
        }
    }

    /// <summary>
    /// Checks if the pointer is over an element of the UI Toolkit screens, which then gets the click instead of the game
    /// </summary>
    /// <param name="screenPosition">In pixels, from the bottom left corner</param>
    public bool IsPointerOver(Vector2 screenPosition)
    {
        IPanel panel = document.rootVisualElement?.panel;
        if (panel == null) return false;
        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        return panel.Pick(panelPosition) != null;
    }
}
