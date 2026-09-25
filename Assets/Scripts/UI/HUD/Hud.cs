using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// The in game HUD (Assets/UI/Hud/Hud.uxml): player status and status effects, action button, timeline, skills bar,
/// bag button, hovered enemy info, damage indicators, minimap and screen fade
/// </summary>
public class Hud : MonoBehaviour
{
    public enum ActionState { Combat, Exploration }

    private const long PulseDuration = 250;

    [SerializeField] private UIDocument document;
    [SerializeField] private ChangeUI changeUI;

    private Button actionButton;
    private string actionTextKey = "ExplorationButton";
    private Action action;
    private StatusPanel status;
    private TimelinePanel timeline;
    private SkillsBar skills;
    private StatusEffectsPanel statusEffects;
    private DamageIndicators damageIndicators;

    public EntityInfoPanel EntityInfo { get; private set; }
    // Used by the map from its Awake, before the HUD is built
    public MinimapPanel Minimap { get; } = new();
    public ScreenFade Fade { get; } = new();

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        MenuScreen.Setup(root, gameObject);

        PlayerTurn player = GameScene.Player;
        status = new StatusPanel(root.Q("Status"), player.Stats);
        timeline = new TimelinePanel(root.Q("Timeline"));
        skills = new SkillsBar(root.Q("Skills"), root.Q<Label>("Tooltip"), player);
        statusEffects = new StatusEffectsPanel(root.Q("StatusEffects"), player.Stats);
        damageIndicators = new DamageIndicators(root.Q("DamageIndicators"));
        EntityInfo = new EntityInfoPanel(root.Q("EntityInfo"));
        Minimap.Bind(root.Q("Minimap"));
        Fade.Bind(root.Q("Fade"));

        actionButton = root.Q<Button>("Action");
        actionButton.clicked += () => action?.Invoke();
        TurnSystem.Instance.TurnChanged += RefreshActionButton;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        RefreshActionButton();
        root.Q<Button>("Bag").clicked += changeUI.Inventory.Toggle;
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
        damageIndicators?.Dispose();
    }

    /// <summary>
    /// Shows the exploration state, or the button ending the player's turn in combat
    /// </summary>
    public void EnterActionState(ActionState state)
    {
        bool combat = state == ActionState.Combat;
        SetAction(combat ? "EndTurnButton" : "ExplorationButton", combat ? TurnSystem.Instance.EndPlayerTurn : null);
    }

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
