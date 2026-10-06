using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The pause menu (Assets/UI/Menus/PauseMenu.uxml) and its options
/// </summary>
public class UIPause : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private UISounds sounds;
    [SerializeField] private ChangeUI changeUI;

    public bool IsPaused { get; private set; }

    private VisualElement screen;
    private VisualElement main;
    private VisualElement panel;
    private OptionsView options;
    private VisualElement summary;

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Pause");
        main = screen.Q("Main");
        panel = main.parent;
        summary = screen.Q("RunSummary");
        options = new OptionsView(screen.Q("Options").parent, BackOptions);
        MenuScreen.Setup(screen, gameObject, sounds);

        screen.Q<Button>("OpenOptions").clicked += OpenOptions;
        screen.Q<Button>("Resume").clicked += () => ToggleOptions(false);
        ConfirmOnSecondClick(screen.Q<MenuButton>("NewRun"), GameFlow.StartRun);
        ConfirmOnSecondClick(screen.Q<MenuButton>("MainMenu"), GameFlow.LoadMainMenu);
        ConfirmOnSecondClick(screen.Q<MenuButton>("Quit"), GameFlow.Quit);
    }

    private void OnDestroy()
    {
        if (IsPaused) GameTime.Paused = false;
        // Their page keys stop with the scene
        options?.Show(false);
    }

    public void ChangeStateOptions()
    {
        if (changeUI.Inventory.IsOpen)
            changeUI.Inventory.Toggle();
        else if (IsPaused && options.IsShown)
            BackOptions();
        else
            ToggleOptions(!IsPaused);
    }

    public void ToggleOptions(bool state)
    {
        // The original's pause was silent
        if (state != IsPaused && Edition.Profile.extraUISounds) (state ? sounds.pauseOpen : sounds.pauseClose).Post(gameObject);
        IsPaused = state;
        //Freezes the actions, the enemy turns and the animations behind the menu
        GameTime.Paused = state;
        screen.EnableInClassList("open", state);
        BackOptions();
        if (!state)
            screen.focusController?.focusedElement?.Blur();
        else
        {
            MenuScreen.FocusFirst(main);
            MenuScreen.ShowDevSeed(screen.Q<Label>("DevSeed"));
            RefreshSummary();
        }
        changeUI.NotifyMenuChanged();
    }

    private const string WidePanelClassName = "side-panel--wide";

    /// <summary>
    /// Leaving the run (a new one, the main menu, the desktop) asks for a second click: the button reads "Confirm?" in between
    /// </summary>
    private static void ConfirmOnSecondClick(MenuButton button, System.Action onConfirmed)
    {
        var confirm = new SecondClick(button, "MenuConfirm");
        button.clicked += () => {
            if (!Edition.Profile.confirmations || confirm.Confirm()) onConfirmed();
        };
    }

    private void OpenOptions()
    {
        main.AddToClassList("hidden");
        // The panel widens for the options
        panel.AddToClassList(WidePanelClassName);
        options.Show(true);
        summary.AddToClassList("hidden");
        MenuScreen.FocusFirst(screen.Q("Options"));
    }

    private void BackOptions()
    {
        bool wasShown = options.IsShown;
        options.Show(false);
        summary.RemoveFromClassList("hidden");
        // In the language the options may have just changed
        if (IsPaused) RefreshSummary();
        panel.RemoveFromClassList(WidePanelClassName);
        main.RemoveFromClassList("hidden");
        if (wasShown && IsPaused) MenuScreen.FocusFirst(main);
    }

    /// <summary>
    /// The run so far: rooms visited, enemies defeated, time played and score
    /// </summary>
    private void RefreshSummary()
    {
        RunStats run = GameScene.Run;
        if (run == null) return;
        int seconds = Mathf.FloorToInt(run.PlayTime);
        summary.Q<Label>("SummaryRooms").text = string.Format(Localization.UI("PlayerProgressVisitedRoom"), run.VisitedRoomCount);
        summary.Q<Label>("SummaryKills").text = string.Format(Localization.UI("PauseSummaryKills"), run.KillCount);
        summary.Q<Label>("SummaryTime").text = string.Format(Localization.UI("PauseSummaryTime"), $"{seconds / 60}:{seconds % 60:00}");
        summary.Q<Label>("SummaryScore").text = string.Format(Localization.UI("PlayerProgressScore"), Localization.Number(run.Score));
    }
}
