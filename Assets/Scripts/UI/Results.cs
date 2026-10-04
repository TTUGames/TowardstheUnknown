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

        screen.Q<Label>("Score").text = string.Format(Localization.UI("EndScreenScore"),
            Edition.Profile.readableStats ? Localization.Number(GameScene.Run.Score) : GameScene.Run.Score.ToString());

        screen.Q<Label>("Summary").text = Summary(isVictory);

        Label message = screen.Q<Label>("Message");
        message.text = Localization.UI(isVictory ? "EndScreenVictory" : "EndScreenDefeat");
        message.EnableInClassList("victory", isVictory);
        message.EnableInClassList("defeat", !isVictory);
        MenuScreen.ShowDevSeed(screen.Q<Label>("DevSeed"));
        GameScene.UI.NotifyMenuChanged();
    }

    /// <summary>
    /// The run in a few lines: its length, the rooms crossed, the enemies killed by family, what dealt the fatal blow after a
    /// defeat, the edition played. The Classic's sheet hides it: the original showed the score alone
    /// </summary>
    private static string Summary(bool isVictory)
    {
        RunStats run = GameScene.Run;
        var lines = new System.Collections.Generic.List<string>();
        int seconds = Mathf.FloorToInt(run.Duration);
        lines.Add(string.Format(Localization.UI("ResultsDuration"), $"{seconds / 60}:{seconds % 60:00}"));
        lines.Add(string.Format(Localization.UI("ResultsRooms"), run.VisitedRoomCount));
        string families = string.Join(", ", System.Linq.Enumerable.Select(run.Kills, kill => $"{Localization.Entity(kill.Key)} \u00D7{kill.Value}"));
        lines.Add(string.Format(Localization.UI("ResultsKills"), run.KillCount) + (families.Length > 0 ? $" ({families})" : ""));
        if (!isVictory && run.LastHitBy != null) lines.Add(string.Format(Localization.UI("ResultsKilledBy"), Localization.Entity(run.LastHitBy.ID)));
        lines.Add(string.Format(Localization.UI("ResultsEdition"), Localization.UI("Edition" + Edition.Current)));
        return string.Join("\n", lines);
    }
}
