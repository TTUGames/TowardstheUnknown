using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// What the platforms show under the player's name (<see cref="SteamPresence"/>, <see cref="Discord_Controller"/>): at the
/// menu, the room of the run and what happens there, Drareg's phase, the run's end, from the start of each room's exploration
/// or combat. Each scene loaded starts it again. The names of <see cref="Display"/> and <see cref="Kind"/> are the platforms'
/// keys: the tokens of Steamworks/rich_presence.vdf (<c>#Room</c>, <c>Combat</c>) and the UI keys (<c>DiscordRoom</c>,
/// <c>DiscordCombat</c>)
/// </summary>
public static class RunPresence
{
    public enum Display { None, Menu, Room, Boss, Victory, Defeat }
    public enum Kind { Exploring, Treasure, Antechamber, Combat }

    /// <summary>
    /// None in a run until its first room starts
    /// </summary>
    public static Display Shown { get; private set; }

    /// <summary>
    /// What happens in the room, for <see cref="Display.Room"/>
    /// </summary>
    public static Kind RoomKind { get; private set; }

    /// <summary>
    /// The room's number in the run for <see cref="Display.Room"/>, Drareg's phase for <see cref="Display.Boss"/>
    /// </summary>
    public static int Number { get; private set; }

    /// <summary>
    /// A scene loaded: the time shown starts again
    /// </summary>
    public static event System.Action SceneStarted;
    public static event System.Action Changed;

    private static int bossPhase;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneStarted = null;
        Changed = null;
        Shown = Display.None;
        RoomKind = Kind.Exploring;
        Number = 0;
        bossPhase = 0;
    }

    // After the game events are cleared for the session
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.CombatStarted += ShowRoom;
        GameEvents.ExplorationStarted += ShowRoom;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
        GameEvents.RunEnded += OnRunEnded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bossPhase = 0;
        SceneStarted?.Invoke();
        Show(scene.buildIndex == GameFlow.MainMenuScene ? Display.Menu : Display.None);
    }

    private static void OnBossPhaseChanged(int phase)
    {
        bossPhase = phase;
        ShowRoom();
    }

    private static void OnRunEnded(bool isVictory) => Show(isVictory ? Display.Victory : Display.Defeat);

    // "Room 6 · Fighting", "Facing Drareg · phase 2"
    private static void ShowRoom()
    {
        // The run's end stays: the exploration starts again once Drareg's combat ends
        if (Shown is Display.Victory or Display.Defeat) return;
        Room room = GameScene.Map != null ? GameScene.Map.CurrentRoom : null;
        if (room == null || GameScene.Run == null) return;
        if (room.type == RoomType.BOSS)
        {
            Show(Display.Boss, Mathf.Max(bossPhase, 1));
            return;
        }
        RoomKind = TurnSystem.Instance != null && TurnSystem.Instance.IsCombat ? Kind.Combat : room.type switch {
            RoomType.TREASURE => Kind.Treasure,
            RoomType.ANTECHAMBER => Kind.Antechamber,
            _ => Kind.Exploring,
        };
        Show(Display.Room, GameScene.Run.VisitedRoomCount + 1);
    }

    private static void Show(Display display, int number = 0)
    {
        Shown = display;
        Number = number;
        Changed?.Invoke();
    }
}
