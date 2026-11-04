using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The run's seed in the pause and the results, a text that copies it when clicked, to replay the run or share it (the main
/// menu's seed field takes it back); says when the run doesn't count for the scores (<see cref="Map.IsRanked"/>)
/// </summary>
[UxmlElement]
public partial class SeedTag : VisualElement
{
    // How long the text reads "Copied" (UssTime), set on the tag by Common.uss
    private static readonly CustomStyleProperty<string> copiedDurationProperty = new("--copied-duration");

    private readonly Button text;
    private string line;
    private IVisualElementScheduledItem reset;

    public SeedTag()
    {
        AddToClassList("seed-tag");
        text = new Button(Copy);
        text.ClearClassList();
        text.AddToClassList("seed-tag__text");
        Add(text);
    }

    /// <summary>
    /// Reads the run's seed again: hidden outside a run
    /// </summary>
    public void Refresh()
    {
        Map map = GameScene.Map;
        EnableInClassList("hidden", map == null);
        if (map == null) return;
        line = string.Format(Localization.UI("SeedValue"), map.Seed);
        // A seed chosen at launch, or a test map: neither the leaderboard nor the achievements count it
        if (!map.IsRanked) line += " · " + Localization.UI("SeedUnranked");
        reset?.Pause();
        Show(false);
    }

    private void Show(bool copied)
    {
        text.text = copied ? string.Format(Localization.UI("SeedValue"), GameScene.Map.Seed) + " · " + Localization.UI("SeedCopied") : line;
        text.EnableInClassList("seed-tag__text--copied", copied);
    }

    private void Copy()
    {
        if (GameScene.Map == null) return;
        GUIUtility.systemCopyBuffer = GameScene.Map.Seed.ToString();
        Show(true);
        reset?.Pause();
        reset = schedule.Execute(() => Show(false)).StartingIn(customStyle.Milliseconds(copiedDurationProperty, 0));
    }
}
