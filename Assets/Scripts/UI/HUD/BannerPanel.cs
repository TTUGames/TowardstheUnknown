using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// Announces the steps of a combat in the middle of the screen: its start, the player's turns, the enemies' turns and the victory.
/// A banner slides in, stays, then fades out (transitions of Hud.uss); the next one waits for it
/// </summary>
public class BannerPanel : IDisposable
{
    private const long ShowDuration = 900;
    private const long HideDuration = 250;

    private readonly SlantedLabel banner;
    private readonly Queue<(string key, string style)> queue = new();
    private bool isShowing;
    private bool wasPlayerTurn;

    public BannerPanel(SlantedLabel banner)
    {
        this.banner = banner;
        GameEvents.CombatStarted += OnCombatStarted;
        GameEvents.CombatEnded += OnCombatEnded;
        GameEvents.RoomLeft += Clear;
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
    }

    public void Dispose()
    {
        GameEvents.CombatStarted -= OnCombatStarted;
        GameEvents.CombatEnded -= OnCombatEnded;
        GameEvents.RoomLeft -= Clear;
        //The turn system may be destroyed first when the scene unloads
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
    }

    private void OnCombatStarted()
    {
        wasPlayerTurn = false;
        Show("BannerCombat", "banner--combat");
    }

    private void OnCombatEnded() => Show("BannerVictory", "banner--victory");

    /// <summary>
    /// The player's turn, then the enemies' turns once: not each enemy's
    /// </summary>
    private void OnTurnChanged()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        if (!turnSystem.IsCombat) return;
        bool isPlayerTurn = turnSystem.IsPlayerTurn;
        if (isPlayerTurn) Show("BannerPlayerTurn", "banner--player");
        else if (wasPlayerTurn) Show("BannerEnemyTurn", "banner--enemy");
        wasPlayerTurn = isPlayerTurn;
    }

    private void Show(string key, string style)
    {
        queue.Enqueue((key, style));
        if (!isShowing) ShowNext();
    }

    private void ShowNext()
    {
        if (queue.Count == 0)
        {
            isShowing = false;
            return;
        }
        isShowing = true;
        (string key, string style) = queue.Dequeue();
        banner.text = Localization.UI(key);
        banner.RemoveFromClassList("banner--combat");
        banner.RemoveFromClassList("banner--victory");
        banner.RemoveFromClassList("banner--player");
        banner.RemoveFromClassList("banner--enemy");
        banner.AddToClassList(style);
        banner.AddToClassList("shown");
        banner.schedule.Execute(() => banner.RemoveFromClassList("shown")).StartingIn(ShowDuration);
        banner.schedule.Execute(ShowNext).StartingIn(ShowDuration + HideDuration);
    }

    private void Clear()
    {
        queue.Clear();
        banner.RemoveFromClassList("shown");
    }
}
