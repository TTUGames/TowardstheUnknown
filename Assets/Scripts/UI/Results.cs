using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The end of run screen (Assets/UI/Menus/Results.uxml)
/// </summary>
public class Results : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private UISounds sounds;
    [SerializeField, Tooltip("The rarities' colors of the artifact pieces")] private RarityPalette rarityPalette;
    private VisualElement screen;
    private IVisualElementScheduledItem scoreCount;

    // Between the screen opening and the score counting up; the slot size of the artifacts' grid
    private static readonly CustomStyleProperty<string> countDelayProperty = new("--count-delay");
    private static readonly CustomStyleProperty<string> tickIntervalProperty = new("--tick-interval");
    private static readonly CustomStyleProperty<float> slotSizeProperty = new("--slot-size");

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
        GameEvents.ScoreRanked += ShowRank;
    }

    private void OnDisable()
    {
        GameEvents.RunEnded -= DisplayResultCanvas;
        GameEvents.ScoreRanked -= ShowRank;
    }

    /// <summary>
    /// The score's world rank among every player's best, once the platform gave it (Steam). The Classic's sheet hides it: the
    /// original had no leaderboard
    /// </summary>
    private void ShowRank(int rank, bool newBest)
    {
        if (screen == null) return;
        Label label = screen.Q<Label>("Rank");
        string number = Edition.Profile.readableStats ? Localization.Number(rank) : rank.ToString();
        // The new record is said once: by the best score under it when this computer's best is beaten too
        bool said = screen.Q<Label>("Best").ClassListContains("results__best--new");
        label.text = string.Format(Localization.UI(newBest && !said ? "ResultsRankNewBest" : "ResultsRank"), number);
        label.RemoveFromClassList("hidden");
    }

    private void DisplayResultCanvas(bool isVictory)
    {
        screen.AddToClassList("open");
        // Until the platform ranks the score
        screen.Q<Label>("Rank").AddToClassList("hidden");

        ShowScore(GameScene.Run.Score, GameScene.Run.RecordBest());
        screen.Q<Label>("Summary").text = Summary(isVictory);
        ShowArtifacts(GameScene.Player.Inventory.Data);

        Label message = screen.Q<Label>("Message");
        message.text = Localization.UI(isVictory ? "EndScreenVictory" : "EndScreenDefeat");
        message.EnableInClassList("victory", isVictory);
        message.EnableInClassList("defeat", !isVictory);
        MenuScreen.ShowDevSeed(screen.Q<Label>("DevSeed"));
        GameScene.UI.NotifyMenuChanged();
    }

    private static string Number(int value) => Edition.Profile.readableStats ? Localization.Number(value) : value.ToString();

    private static string ScoreText(int score) => string.Format(Localization.UI("EndScreenScore"), Number(score));

    /// <summary>
    /// The score, counting up from 0 with a tick (<see cref="EditionProfile.scoreCount"/>), then the best score under it:
    /// "New record!" when this run beat it, the best one otherwise, nothing before the first
    /// </summary>
    private void ShowScore(int score, (int before, bool beaten) best)
    {
        Label label = screen.Q<Label>("Score");
        Label bestLabel = screen.Q<Label>("Best");
        bestLabel.text = best.beaten ? Localization.UI("ResultsNewBest")
            : best.before > 0 ? string.Format(Localization.UI("ResultsBest"), Number(best.before)) : "";
        bestLabel.EnableInClassList("results__best--new", best.beaten);
        scoreCount?.Pause();
        float duration = Edition.Profile.scoreCount;
        if (duration <= 0 || score <= 0)
        {
            label.text = ScoreText(score);
            bestLabel.RemoveFromClassList("results__best--waiting");
            return;
        }
        label.text = ScoreText(0);
        bestLabel.AddToClassList("results__best--waiting");
        label.customStyle.TryGetSeconds(countDelayProperty, out float delay);
        label.customStyle.TryGetSeconds(tickIntervalProperty, out float tickInterval);
        float start = Time.unscaledTime + delay, lastTick = float.NegativeInfinity;
        int shown = 0;
        scoreCount = label.schedule.Execute(() => {
            // Fast, then slowing down to the score
            float t = Mathf.Clamp01((Time.unscaledTime - start) / duration);
            int value = Mathf.RoundToInt(score * (1 - Mathf.Pow(1 - t, 3)));
            if (value != shown)
            {
                shown = value;
                label.text = ScoreText(value);
                if (Edition.Profile.extraUISounds && Time.unscaledTime - lastTick >= tickInterval)
                {
                    lastTick = Time.unscaledTime;
                    sounds.buttonHover.Post(gameObject);
                }
            }
            if (t < 1) return;
            scoreCount.Pause();
            bestLabel.RemoveFromClassList("results__best--waiting");
        }).Every(16);
    }

    /// <summary>
    /// The artifacts the player ended the run with, small, in their places in the grid of the inventory. The Classic's sheet
    /// hides them: the original showed the score alone
    /// </summary>
    private void ShowArtifacts(TetrisInventoryData data)
    {
        VisualElement grid = screen.Q("Artifacts");
        grid.Clear();
        float cell = grid.customStyle.TryGetValue(slotSizeProperty, out float size) ? size : 32;
        grid.style.width = data.gridSize.x * cell;
        grid.style.height = data.gridSize.y * cell;
        for (int x = 0; x < data.gridSize.x; x++)
            for (int y = 0; y < data.gridSize.y; y++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList("results__slot");
                slot.style.left = x * cell;
                slot.style.top = (data.gridSize.y - 1 - y) * cell;
                slot.style.width = cell;
                slot.style.height = cell;
                grid.Add(slot);
            }
        foreach (TetrisInventoryItem item in data.Items)
        {
            var piece = new ArtifactPiece(item.itemData, cell, rarityPalette);
            piece.AddToClassList("inventory-item");
            TetrisInventory.SetRotation(piece, item, cell);
            TetrisInventory.PlaceItemImage(piece, item, new Vector2(item.slot.x * cell, data.gridSize.y * cell - item.slot.y * cell), cell);
            grid.Add(piece);
        }
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
