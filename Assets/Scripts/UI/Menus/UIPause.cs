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

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Pause");
        main = screen.Q("Main");
        panel = main.parent;
        options = new OptionsView(screen.Q("Options").parent, BackOptions);
        MenuScreen.Setup(screen, gameObject, sounds);

        screen.Q<Button>("OpenOptions").clicked += OpenOptions;
        screen.Q<Button>("Resume").clicked += () => ToggleOptions(false);
        ConfirmOnSecondClick(screen.Q<MenuButton>("MainMenu"), GameFlow.LoadMainMenu);
        ConfirmOnSecondClick(screen.Q<MenuButton>("Quit"), GameFlow.Quit);
    }

    private void OnDestroy()
    {
        if (IsPaused) GameTime.Paused = false;
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
        IsPaused = state;
        //Freezes the actions, the enemy turns and the animations behind the menu
        GameTime.Paused = state;
        screen.EnableInClassList("open", state);
        BackOptions();
        if (!state)
            screen.focusController?.focusedElement?.Blur();
        changeUI.NotifyMenuChanged();
    }

    private const string WidePanelClassName = "side-panel--wide";

    // The second click must come within this delay
    private const long ConfirmDuration = 3000;

    /// <summary>
    /// Leaving the run asks for a second click: the button reads "Confirm?" in between
    /// </summary>
    private static void ConfirmOnSecondClick(MenuButton button, System.Action onConfirmed)
    {
        var confirm = new SecondClick(button, "MenuConfirm", ConfirmDuration);
        button.clicked += () => {
            if (confirm.Confirm()) onConfirmed();
        };
    }

    private void OpenOptions()
    {
        main.AddToClassList("hidden");
        // The panel widens for the options
        panel.AddToClassList(WidePanelClassName);
        options.Show(true);
    }

    private void BackOptions()
    {
        options.Show(false);
        panel.RemoveFromClassList(WidePanelClassName);
        main.RemoveFromClassList("hidden");
    }
}
