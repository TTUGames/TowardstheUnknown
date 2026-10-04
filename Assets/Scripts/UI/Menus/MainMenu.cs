using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// The main menu (Assets/UI/Menus/MainMenu.uxml): home, options, credits and the disclaimer shown on the first launch.
/// A badge in a corner of the home gives the version and the edition played, and opens the options where it is chosen
/// With a suspended run (<see cref="RunSave"/>, <see cref="EditionProfile.suspendRun"/>), Continue goes on with it and Play
/// asks for a second click before dropping it
/// </summary>
public class MainMenu : MonoBehaviour
{
    private const string DisclaimerSeenKey = "DisclaimerSeen";

    [SerializeField] private UIDocument document;
    [SerializeField] private UISounds sounds;

    private VisualElement home;
    private VisualElement optionsScreen;
    private VisualElement credits;
    private VisualElement disclaimer;
    private OptionsView options;
    private SlantedButton badge;
    private MenuButton continueButton;
    private SecondClick newRunConfirm;

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        home = root.Q("Home");
        optionsScreen = root.Q("OptionsScreen");
        credits = root.Q("Credits");
        disclaimer = root.Q("Disclaimer");
        options = new OptionsView(optionsScreen.Q("Options").parent, ShowHome);
        MenuScreen.Setup(root, gameObject, sounds);

        continueButton = home.Q<MenuButton>("Continue");
        continueButton.clicked += GameFlow.ContinueRun;
        var play = home.Q<MenuButton>("Play");
        newRunConfirm = new SecondClick(play, "MenuConfirm");
        play.clicked += () => {
            if (!CanContinue || !Edition.Profile.confirmations || newRunConfirm.Confirm()) GameFlow.StartRun();
        };
        Edition.Changed += OnEditionChanged;
        home.Q<Button>("OpenOptions").clicked += ShowOptions;
        badge = home.Q<SlantedButton>("EditionBadge");
        badge.clicked += ShowOptions;
        RefreshBadge(Edition.Current);
        Edition.Changed += RefreshBadge;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        home.Q<Button>("OpenCredits").clicked += ShowCredits;
        home.Q<Button>("Quit").clicked += GameFlow.Quit;
        credits.Q<Button>("CloseCredits").clicked += ShowHome;
        disclaimer.Q<Button>("CloseDisclaimer").clicked += CloseDisclaimer;

        ShowHome();
        if (PlayerPrefs.GetInt(DisclaimerSeenKey, 0) == 0)
            Show(disclaimer);
    }

    private void OnDestroy()
    {
        Edition.Changed -= RefreshBadge;
        Edition.Changed -= OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale) => RefreshBadge(Edition.Current);

    // "v2.0.0 · Anniversary"
    private void RefreshBadge(GameEdition edition) => badge.text = $"v{Application.version} · {Localization.UI("Edition" + edition)}";

    private static bool CanContinue => RunSave.Exists && Edition.Profile.suspendRun;

    private void OnEditionChanged(GameEdition edition) => RefreshContinue();

    // Continue shows with a suspended run, in an edition that has them
    private void RefreshContinue()
    {
        bool shown = CanContinue;
        continueButton.EnableInClassList("hidden", !shown);
        continueButton.parent.Q(className: "main-menu__continue-separator").EnableInClassList("hidden", !shown);
        newRunConfirm.Cancel();
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

    private void ShowHome()
    {
        RefreshContinue();
        Show(home);
    }

    private void ShowOptions()
    {
        Show(optionsScreen);
        options.Show(true);
    }

    private void ShowCredits() => Show(credits);

    /// <summary>
    /// Opens one screen and closes the others
    /// </summary>
    private void Show(VisualElement screen)
    {
        foreach (VisualElement other in new[] { home, optionsScreen, credits, disclaimer })
            other.EnableInClassList("open", other == screen);
        MenuScreen.FocusFirst(screen);
    }

    private void CloseDisclaimer()
    {
        PlayerPrefs.SetInt(DisclaimerSeenKey, 1);
        PlayerPrefs.Save();
        ShowHome();
    }
}
