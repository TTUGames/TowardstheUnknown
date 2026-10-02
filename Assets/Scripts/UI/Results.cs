using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The end of run screen (Assets/UI/Menus/Results.uxml)
/// </summary>
public class Results : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private UISounds sounds;
    private VisualElement screen;

    public bool IsShown => screen != null && screen.ClassListContains("open");

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Results");
        // The original's death screen buttons were silent
        MenuScreen.Setup(screen, gameObject, sounds, originalSounds: false);
        ConfirmOnSecondClick(screen.Q<SlantedButton>("Restart"), GameFlow.StartRun);
        ConfirmOnSecondClick(screen.Q<SlantedButton>("MainMenu"), GameFlow.LoadMainMenu);
        // Not focused on opening: Space, which ends the turns, is also the submit key
        MenuScreen.FocusFirstOnNavigation(screen);
    }

    /// <summary>
    /// Leaving the results asks for a second click, so that a reflex double click doesn't skip them
    /// </summary>
    private static void ConfirmOnSecondClick(SlantedButton button, System.Action onConfirmed)
    {
        var confirm = new SecondClick(button, "MenuConfirm");
        button.clicked += () => {
            if (!Edition.Profile.confirmations || confirm.Confirm()) onConfirmed();
        };
    }

    private void OnEnable()
    {
        GameEvents.RunEnded += DisplayResultCanvas;
    }

    private void OnDisable()
    {
        GameEvents.RunEnded -= DisplayResultCanvas;
    }

    private void DisplayResultCanvas(bool isVictory)
    {
        screen.AddToClassList("open");

        screen.Q<Label>("Score").text = string.Format(Localization.UI("EndScreenScore"), GameScene.Run.Score.ToString());

        Label message = screen.Q<Label>("Message");
        message.text = Localization.UI(isVictory ? "EndScreenVictory" : "EndScreenDefeat");
        message.EnableInClassList("victory", isVictory);
        message.EnableInClassList("defeat", !isVictory);
        GameScene.UI.NotifyMenuChanged();
    }
}
