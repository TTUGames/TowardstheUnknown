using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Announces the steps of a combat in the middle of the screen: its start, a boss's name at its entrance, the player's turns with their number, the enemies' turns and the victory.
/// A banner slides in with its sound (EditionProfile.extraUISounds), stays, then fades out (transitions of Hud.uss); the next one waits for it.
/// A banner USS doesn't display (the Classic's) announces nothing
/// </summary>
public class BannerPanel : IDisposable
{
    private const long ShowDuration = 900;
    // The victory stays through the victory's beat (EditionProfile.victoryBeat)
    private const long VictoryShowDuration = 1500;
    private const long HideDuration = 250;

    private readonly SlantedLabel banner;
    private readonly GameObject soundEmitter;
    private readonly UISounds sounds;
    private readonly Queue<(string text, string style)> queue = new();
    private bool isShowing;
    private bool wasPlayerTurn;
    // The player's turns since the combat started
    private int playerTurns;

    public BannerPanel(SlantedLabel banner, GameObject soundEmitter, UISounds sounds)
    {
        this.banner = banner;
        this.soundEmitter = soundEmitter;
        this.sounds = sounds;
        GameEvents.CombatStarted += OnCombatStarted;
        GameEvents.BossIntroStarted += OnBossIntroStarted;
        GameEvents.CombatEnded += OnCombatEnded;
        GameEvents.RoomLeft += Clear;
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
    }

    public void Dispose()
    {
        GameEvents.CombatStarted -= OnCombatStarted;
        GameEvents.BossIntroStarted -= OnBossIntroStarted;
        GameEvents.CombatEnded -= OnCombatEnded;
        GameEvents.RoomLeft -= Clear;
        //The turn system may be destroyed first when the scene unloads
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
    }

    private void OnCombatStarted()
    {
        wasPlayerTurn = false;
        playerTurns = 0;
        Show(Localization.UI("BannerCombat"), "banner--combat");
    }

    private void OnBossIntroStarted(EntityStats boss, float seconds) => Show(Localization.Entity(boss.ID), "banner--boss");

    private void OnCombatEnded() => Show(Localization.UI("BannerVictory"), "banner--victory");

    /// <summary>
    /// The player's turn, then the enemies' turns once: not each enemy's
    /// </summary>
    private void OnTurnChanged()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        if (!turnSystem.IsCombat) return;
        bool isPlayerTurn = turnSystem.IsPlayerTurn;
        if (isPlayerTurn) Show(string.Format(Localization.UI("BannerPlayerTurnNumber"), ++playerTurns), "banner--player");
        else if (wasPlayerTurn) Show(Localization.UI("BannerEnemyTurn"), "banner--enemy");
        wasPlayerTurn = isPlayerTurn;
    }

    private void Show(string text, string style)
    {
        if (banner.resolvedStyle.display == DisplayStyle.None) return;
        queue.Enqueue((text, style));
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
        (string text, string style) = queue.Dequeue();
        banner.text = text;
        banner.RemoveFromClassList("banner--combat");
        banner.RemoveFromClassList("banner--victory");
        banner.RemoveFromClassList("banner--player");
        banner.RemoveFromClassList("banner--enemy");
        banner.AddToClassList(style);
        banner.AddToClassList("shown");
        if (Edition.Profile.extraUISounds) Sound(style).Post(soundEmitter);
        long showDuration = style == "banner--victory" ? VictoryShowDuration : ShowDuration;
        banner.schedule.Execute(() => banner.RemoveFromClassList("shown")).StartingIn(showDuration);
        banner.schedule.Execute(ShowNext).StartingIn(showDuration + HideDuration);
    }

    private AK.Wwise.Event Sound(string style) => style switch
    {
        "banner--combat" => sounds.combatStart,
        "banner--victory" => sounds.bannerVictory,
        "banner--player" => sounds.bannerPlayerTurn,
        _ => sounds.bannerEnemyTurn,
    };

    private void Clear()
    {
        queue.Clear();
        banner.RemoveFromClassList("shown");
    }
}
