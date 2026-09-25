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

    public bool isPaused = false;

    private VisualElement screen;
    private VisualElement main;
    private OptionsView options;

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Pause");
        main = screen.Q("Main");
        options = new OptionsView(screen.Q("Options").parent, BackOptions);
        MenuScreen.Setup(screen, gameObject, sounds);

        screen.Q<Button>("OpenOptions").clicked += OpenOptions;
        screen.Q<Button>("Resume").clicked += () => ToggleOptions(false);
        screen.Q<Button>("MainMenu").clicked += GameFlow.LoadMainMenu;
        screen.Q<Button>("Quit").clicked += GameFlow.Quit;
    }

    private void OnDestroy()
    {
        if (isPaused) GameTime.Paused = false;
    }

    public void ChangeStateOptions()
    {
        if (changeUI.Inventory.IsOpen)
            changeUI.Inventory.Toggle();
        else if (isPaused && options.IsShown)
            BackOptions();
        else
            ToggleOptions(!isPaused);
    }

    public void ToggleOptions(bool state)
    {
        isPaused = state;
        //Freezes the actions, the enemy turns and the animations behind the menu
        GameTime.Paused = state;
        screen.EnableInClassList("open", state);
        changeUI.Hud.Minimap.SetVisible(!state && !changeUI.Inventory.IsOpen);
        BackOptions();
        if (!state)
            screen.focusController?.focusedElement?.Blur();
    }

    private void OpenOptions()
    {
        main.AddToClassList("hidden");
        options.Show(true);
    }

    private void BackOptions()
    {
        options.Show(false);
        main.RemoveFromClassList("hidden");
    }
}
