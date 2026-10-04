using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// What the player's Steam friends read under their name: at the menu, the room of the run and what happens there, Drareg's
/// phase, from the start of each room's exploration or combat. Steam writes it from the tokens of Steamworks/rich_presence.vdf, uploaded in the app's Steamworks settings
/// (Rich Presence localization): <c>steam_display</c> names the token, the other keys fill it
/// </summary>
public static class SteamPresence
{
    private static int bossPhase;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        bossPhase = 0;
    }

    // After the game events are cleared for the session
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.CombatStarted += ShowCurrentRoom;
        GameEvents.ExplorationStarted += ShowCurrentRoom;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
        GameEvents.RunEnded += OnRunEnded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bossPhase = 0;
        if (scene.buildIndex == GameFlow.MainMenuScene) Show("#Menu");
    }

    private static void ShowCurrentRoom() => ShowRoom(GameScene.Map != null ? GameScene.Map.CurrentRoom : null);

    private static void OnBossPhaseChanged(int phase)
    {
        bossPhase = phase;
        ShowCurrentRoom();
    }

    private static void OnRunEnded(bool isVictory) => Show(isVictory ? "#Victory" : "#Defeat");

    // "Room 6 · Fighting", "Facing Drareg · phase 2"
    private static void ShowRoom(Room room)
    {
        if (room == null || GameScene.Run == null) return;
        if (room.type == RoomType.BOSS)
        {
            Set("phase", Mathf.Max(bossPhase, 1).ToString());
            Show("#Boss");
            return;
        }
        Set("room", (GameScene.Run.VisitedRoomCount + 1).ToString());
        Set("kind", TurnSystem.Instance != null && TurnSystem.Instance.IsCombat ? "Combat" : room.type switch {
            RoomType.TREASURE => "Treasure",
            RoomType.ANTECHAMBER => "Antechamber",
            _ => "Exploring",
        });
        Show("#Room");
    }

    private static void Show(string token) => Set("steam_display", token);

    private static void Set(string key, string value)
    {
        if (SteamManager.Initialized) SteamFriends.SetRichPresence(key, value);
    }
}
