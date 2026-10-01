using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

/// <summary>
/// Updates the Steam stats and achievements from the run's progress (<see cref="RunStats"/>) and its end, only in a randomly
/// generated run: the test maps push nothing
/// </summary>
public class SteamAchievements : MonoBehaviour
{
    private const int MaxScore = 50000;

    // The run's counts already added to the Steam stats
    private int pushedKills;
    private int pushedRooms;

    void Start()
    {
        if (SteamManager.Initialized)
            SteamUserStats.RequestCurrentStats();
    }

    //Debug shortcut resetting the player's stats and achievements, the Debug controls are never enabled in release builds
    private void OnEnable()
    {
        GameInput.Controls.Debug.ResetAchievements.performed += OnResetAchievements;
        GameScene.Run.Changed += OnRunChanged;
        GameEvents.RunEnded += OnRunEnded;
    }

    private void OnDisable()
    {
        GameInput.Controls.Debug.ResetAchievements.performed -= OnResetAchievements;
        if (GameScene.Run != null) GameScene.Run.Changed -= OnRunChanged;
        GameEvents.RunEnded -= OnRunEnded;
    }

    private static bool Counts => SteamManager.Initialized && GameScene.Map != null && GameScene.Map.IsRandomRun;

    private void OnRunChanged()
    {
        if (!Counts) return;
        RunStats run = GameScene.Run;
        AddToStat("entity_killed", run.KillCount - pushedKills);
        AddToStat("explored_rooms", run.VisitedRoomCount - pushedRooms);
        pushedKills = run.KillCount;
        pushedRooms = run.VisitedRoomCount;
    }

    // The run ends on the death of the player or of Drareg
    private void OnRunEnded(bool isVictory)
    {
        if (!Counts) return;
        if (isVictory) SetAchievement("ACH_KILL_DRAREG");
        else AddToStat("death", 1);
        if (GameScene.Run.Score >= MaxScore) SetAchievement("ACH_MAXSCORE");
    }

    private void OnResetAchievements(InputAction.CallbackContext context)
    {
        if (!SteamManager.Initialized) return;
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.StoreStats();
    }

    private static bool SetAchievement(string pchName) {
        return SteamManager.Initialized && SteamUserStats.SetAchievement(pchName) && SteamUserStats.StoreStats();
    }

    private static bool AddToStat(string pchName, int amount) {
        return amount > 0
            && SteamManager.Initialized
            && SteamUserStats.GetStat(pchName, out int previousValue)
            && SteamUserStats.SetStat(pchName, previousValue + amount)
            && SteamUserStats.StoreStats();
    }
}
