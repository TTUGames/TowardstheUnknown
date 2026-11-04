using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The run's seed in a corner of the pause and the results, with a button copying it, to replay the run or share it (the
/// main menu's seed field takes it back); says when the run doesn't count for the scores (<see cref="Map.IsRanked"/>)
/// </summary>
[UxmlElement]
public partial class SeedTag : VisualElement
{
    // How long the copy button reads "Copied" (UssTime), set on the tag by Common.uss
    private static readonly CustomStyleProperty<string> copiedDurationProperty = new("--copied-duration");

    private readonly Label text;
    private readonly SlantedButton copy;
    private IVisualElementScheduledItem reset;

    public SeedTag()
    {
        AddToClassList("seed-tag");
        text = new Label { pickingMode = PickingMode.Ignore };
        text.AddToClassList("seed-tag__text");
        copy = new SlantedButton { key = "SeedCopy", corners = Corners.TopLeft | Corners.BottomRight };
        copy.AddToClassList("outline-button");
        copy.AddToClassList("panel");
        copy.AddToClassList("seed-tag__copy");
        copy.clicked += Copy;
        Add(text);
        Add(copy);
    }

    /// <summary>
    /// Reads the run's seed again: hidden outside a run
    /// </summary>
    public void Refresh()
    {
        Map map = GameScene.Map;
        EnableInClassList("hidden", map == null);
        if (map == null) return;
        string line = string.Format(Localization.UI("SeedValue"), map.Seed);
        // A seed chosen at launch, or a test map: neither the leaderboard nor the achievements count it
        if (!map.IsRanked) line += " · " + Localization.UI("SeedUnranked");
        text.text = line;
        reset?.Pause();
        copy.key = "SeedCopy";
    }

    private void Copy()
    {
        if (GameScene.Map == null) return;
        GUIUtility.systemCopyBuffer = GameScene.Map.Seed.ToString();
        copy.key = "SeedCopied";
        reset?.Pause();
        reset = schedule.Execute(() => copy.key = "SeedCopy").StartingIn(customStyle.Milliseconds(copiedDurationProperty, 0));
    }
}
