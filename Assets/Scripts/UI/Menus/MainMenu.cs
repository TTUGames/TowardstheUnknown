using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// The main menu (Assets/UI/Menus/MainMenu.uxml): home, options, credits and the disclaimer shown on the first launch
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string DisclaimerSeenKey = "DisclaimerSeen";

    [SerializeField] private UIDocument document;
    [SerializeField] private FilterFunctionDefinition slantedBlur;

    private VisualElement home;
    private VisualElement optionsScreen;
    private VisualElement credits;
    private VisualElement disclaimer;
    private OptionsView options;

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        home = root.Q("Home");
        optionsScreen = root.Q("OptionsScreen");
        credits = root.Q("Credits");
        disclaimer = root.Q("Disclaimer");
        options = new OptionsView(optionsScreen.Q("Options").parent, ShowHome);
        MenuScreen.Setup(root, gameObject, slantedBlur);

        home.Q<Button>("Play").clicked += GameFlow.StartRun;
        home.Q<Button>("OpenOptions").clicked += ShowOptions;
        home.Q<Button>("OpenCredits").clicked += ShowCredits;
        home.Q<Button>("Quit").clicked += GameFlow.Quit;
        credits.Q<Button>("CloseCredits").clicked += ShowHome;
        disclaimer.Q<Button>("CloseDisclaimer").clicked += CloseDisclaimer;

        ShowHome();
        if (PlayerPrefs.GetInt(DisclaimerSeenKey, 0) == 0)
            Show(disclaimer, disclaimer.Q<Button>("CloseDisclaimer"));
    }

    private void OnEnable()
    {
        GameInput.Controls.Menus.Back.performed += OnBack;
    }

    private void OnDisable()
    {
        GameInput.Controls.Menus.Back.performed -= OnBack;
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        if (disclaimer.ClassListContains("open"))
            CloseDisclaimer();
        else if (!home.ClassListContains("open"))
            ShowHome();
    }

    private void ShowHome() => Show(home, home.Q<Button>("Play"));

    private void ShowOptions()
    {
        Show(optionsScreen, null);
        options.Show(true);
    }

    private void ShowCredits() => Show(credits, credits.Q<Button>("CloseCredits"));

    /// <summary>
    /// Opens one screen and closes the others
    /// </summary>
    private void Show(VisualElement screen, Button focused)
    {
        foreach (VisualElement other in new[] { home, optionsScreen, credits, disclaimer })
            other.EnableInClassList("open", other == screen);
        focused?.Focus();
    }

    private void CloseDisclaimer()
    {
        PlayerPrefs.SetInt(DisclaimerSeenKey, 1);
        PlayerPrefs.Save();
        ShowHome();
    }
}
