using System;
using UnityEngine.UIElements;

/// <summary>
/// At the first entry of a zone (the run's start in the frozen caves, the antechamber of Drareg's garden), a card at the top
/// of the screen gives its title and the first sentence of its text, which the character sheet holds whole; it fades in,
/// stays, then fades out, under the room wipe and once it has revealed the room. The Classic's sheet hides it: the original kept the text in the sheet
/// </summary>
public class ZoneIntro : IDisposable
{
    private const string ShownClassName = "zone-intro--shown";
    // How long the card stays (--show-duration of .zone-intro), in milliseconds
    private static readonly CustomStyleProperty<string> showDurationProperty = new("--show-duration");

    private readonly VisualElement card;
    private readonly Label title;
    private readonly Label text;
    private readonly SlantedWipe wipe;
    private IVisualElementScheduledItem hide;
    // The wipe covers the screen: the card waits for it to reveal the room
    private bool covered;
    private (string title, string text)? pending;

    /// <param name="wipe">The room wipe, drawn over the card</param>
    public ZoneIntro(VisualElement root, SlantedWipe wipe)
    {
        this.wipe = wipe;
        card = new SlantedPanel { pickingMode = PickingMode.Ignore, corners = Corners.TopLeft | Corners.BottomRight };
        card.AddToClassList("zone-intro");
        card.AddToClassList("panel");
        title = new Label { pickingMode = PickingMode.Ignore };
        title.AddToClassList("zone-intro__title");
        title.AddToClassList("caps");
        text = new Label { pickingMode = PickingMode.Ignore };
        text.AddToClassList("zone-intro__text");
        card.Add(title);
        card.Add(text);
        wipe.parent.Insert(wipe.parent.IndexOf(wipe), card);
        wipe.Covering += OnCovering;
        wipe.Revealing += OnRevealing;
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.RoomLeft += Hide;
        // The run's first room may be entered before the HUD is built
        Room current = GameScene.Map != null ? GameScene.Map.CurrentRoom : null;
        if (current != null && current.type == RoomType.SPAWN && GameScene.Run != null && GameScene.Run.VisitedRoomCount == 0)
            OnRoomEntered(current, true);
    }

    public void Dispose()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.RoomLeft -= Hide;
        wipe.Covering -= OnCovering;
        wipe.Revealing -= OnRevealing;
    }

    private void OnCovering() => covered = true;

    private void OnRevealing()
    {
        covered = false;
        if (pending is not { } shown) return;
        pending = null;
        Show(shown.title, shown.text);
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (!firstVisit) return;
        if (room.type == RoomType.SPAWN) Show("ZoneInfoZeroHeader", "ZoneInfoZeroContent");
        else if (room.type == RoomType.ANTECHAMBER) Show("ZoneInfoGardenHeader", "ZoneInfoGardenContent");
    }

    private void Show(string titleKey, string textKey)
    {
        if (covered)
        {
            pending = (titleKey, textKey);
            return;
        }
        title.text = Localization.UI(titleKey);
        text.text = FirstSentence(Localization.UI(textKey));
        card.AddToClassList(ShownClassName);
        card.customStyle.TryGetSeconds(showDurationProperty, out float show);
        hide?.Pause();
        hide = card.schedule.Execute(Hide).StartingIn((long)(show > 0 ? show * 1000 : 5000));
    }

    private void Hide()
    {
        pending = null;
        card.RemoveFromClassList(ShownClassName);
    }

    // Up to its first full stop, or its first line
    private static string FirstSentence(string text)
    {
        int end = text.Length;
        foreach (char stop in new[] { '.', '!', '?', '\n' })
        {
            int index = text.IndexOf(stop);
            if (index >= 0 && index + 1 < end) end = stop == '\n' ? index : index + 1;
        }
        return text.Substring(0, end).Trim();
    }
}
