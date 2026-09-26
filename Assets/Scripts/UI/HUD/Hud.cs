using System;
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
    private const long PulseDuration = 250;
    // The second press ending the turn must come within this delay
    private const long ConfirmDuration = 2500;

    [SerializeField] private UIDocument document;
    [SerializeField] private ChangeUI changeUI;
    [SerializeField] private UISounds sounds;

    private Button actionButton;
    private string actionTextKey = "ExplorationButton";
    private Action action;
    private StatusPanel status;
    private TimelinePanel timeline;
    private SkillsBar skills;
    private StatusEffectsPanel statusEffects;
    private CombatPopups popups;
    private BannerPanel banner;
    private BossBar bossBar;
    private DamagePreview damagePreview;
    private bool confirmingEndTurn;
    private IVisualElementScheduledItem cancelConfirm;

    public EntityInfoPanel EntityInfo { get; private set; }
    // Used by the map from its Awake, before the HUD is built
    public MinimapPanel Minimap { get; } = new();
    public ScreenFade Fade { get; } = new();

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        MenuScreen.Setup(root, gameObject, sounds);

        PlayerTurn player = GameScene.Player;
        status = new StatusPanel(root.Q("Status"), player.Stats);
        timeline = new TimelinePanel(root.Q("Timeline"), sounds.timelineHover);
        skills = new SkillsBar(root.Q("Skills"), root.Q<Label>("Tooltip"), player);
        statusEffects = new StatusEffectsPanel(root.Q("StatusEffects"), root.Q<Label>("StatusTooltip"), player.Stats);
        popups = new CombatPopups(root.Q("Popups"));
        banner = new BannerPanel(root.Q<SlantedLabel>("Banner"));
        bossBar = new BossBar(root.Q("BossBar"));
        damagePreview = new DamagePreview(root.Q("Popups"), player.playerAttack);
        EntityInfo = new EntityInfoPanel(root.Q("EntityInfo"), player);
        Minimap.Bind(root.Q("Minimap"));
        Fade.Bind(root.Q("Fade"));

        actionButton = root.Q<Button>("Action");
        actionButton.clicked += OnAction;
        TurnSystem.Instance.TurnChanged += RefreshActionButton;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        RefreshActionButton();
        root.Q<Button>("Bag").clicked += changeUI.Inventory.Toggle;
    }

    private void OnEnable()
    {
        GameEvents.CombatStarted += EnterCombatState;
        GameEvents.ExplorationStarted += EnterExplorationState;
        GameInput.Controls.Gameplay.EndTurn.performed += OnActionKey;
    }

    private void OnDisable()
    {
        GameEvents.CombatStarted -= EnterCombatState;
        GameEvents.ExplorationStarted -= EnterExplorationState;
        GameInput.Controls.Gameplay.EndTurn.performed -= OnActionKey;
    }

    // The key presses the action button: end of turn, deployment
    private void OnActionKey(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (actionButton != null && actionButton.enabledInHierarchy && !GameScene.IsGameplayBlocked) OnAction();
    }

    /// <summary>
    /// Ending the turn while an artifact can still be cast asks for a second press
    /// </summary>
    private void OnAction()
    {
        if (action == null) return;
        if (actionTextKey == "EndTurnButton" && !confirmingEndTurn && CanStillCast())
        {
            confirmingEndTurn = true;
            actionButton.text = Localization.UI("EndTurnConfirm");
            actionButton.AddToClassList("confirm");
            cancelConfirm = actionButton.schedule.Execute(CancelConfirm).StartingIn(ConfirmDuration);
            return;
        }
        CancelConfirm();
        action();
    }

    private void CancelConfirm()
    {
        cancelConfirm?.Pause();
        if (!confirmingEndTurn) return;
        confirmingEndTurn = false;
        actionButton.RemoveFromClassList("confirm");
        actionButton.text = Localization.UI(actionTextKey);
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
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        status?.Dispose();
        timeline?.Dispose();
        skills?.Dispose();
        statusEffects?.Dispose();
        popups?.Dispose();
        banner?.Dispose();
        bossBar?.Dispose();
        damagePreview?.Dispose();
        EntityInfo?.Dispose();
    }

    /// <summary>
    /// In combat, the button ends the player's turn
    /// </summary>
    private void EnterCombatState() => SetAction("EndTurnButton", TurnSystem.Instance.EndPlayerTurn);

    private void EnterExplorationState() => SetAction("ExplorationButton", null);

    /// <summary>
    /// Enters the deploy state, the button calling <paramref name="endDeploy"/>
    /// </summary>
    public void EnterDeployState(Action endDeploy) => SetAction("DeployButton", endDeploy);

    /// <summary>
    /// Rewrites the texts built by code in the new language (the UXML texts follow by themselves)
    /// </summary>
    private void OnLocaleChanged(Locale locale)
    {
        RefreshActionButton();
        timeline.Refresh();
    }

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
        CancelConfirm();
        actionButton.text = Localization.UI(actionTextKey);
        // During the enemies' turns, the button waits for the player's turn
        TurnSystem turnSystem = TurnSystem.Instance;
        bool waiting = turnSystem.IsCombat && !turnSystem.IsPlayerTurn;
        // The button pulses when the player's turn starts
        if (turnSystem.IsCombat && !waiting && (actionButton.ClassListContains("waiting") || !actionButton.enabledSelf))
        {
            actionButton.AddToClassList("pulse");
            actionButton.schedule.Execute(() => actionButton.RemoveFromClassList("pulse")).StartingIn(PulseDuration);
        }
        actionButton.EnableInClassList("waiting", waiting);
        actionButton.SetEnabled(action != null && !waiting);
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
