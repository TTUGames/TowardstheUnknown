using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

/// <summary>
/// Updates the Steam stats and achievements from the game events, only in a randomly generated run: the test maps push nothing
/// </summary>
public class SteamAchievements : MonoBehaviour
{
    private const int MaxScore = 50000;

    void Start()
    {
        if (SteamManager.Initialized)
            SteamUserStats.RequestCurrentStats();
    }

    //Debug shortcut resetting the player's stats and achievements, the Debug controls are never enabled in release builds
    private void OnEnable()
    {
        GameInput.Controls.Debug.ResetAchievements.performed += OnResetAchievements;
        GameEvents.EntityDied += OnEntityDied;
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.RunEnded += OnRunEnded;
    }

    private void OnDisable()
    {
        GameInput.Controls.Debug.ResetAchievements.performed -= OnResetAchievements;
        GameEvents.EntityDied -= OnEntityDied;
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.RunEnded -= OnRunEnded;
    }

    private static bool Counts => SteamManager.Initialized && GameScene.Map != null && GameScene.Map.IsRandomRun;

    private void OnEntityDied(EntityStats entity)
    {
        if (!Counts) return;
        if (entity is PlayerStats)
        {
            IncrementStat("death");
            return;
        }
        IncrementStat("entity_killed");
        if (entity is DraregStats) SetAchievement("ACH_KILL_DRAREG");
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (Counts && firstVisit && room.type != RoomType.SPAWN) IncrementStat("explored_rooms");
    }

    private void OnRunEnded(bool isVictory)
    {
        if (Counts && GameScene.Run.Score >= MaxScore) SetAchievement("ACH_MAXSCORE");
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

    private static bool IncrementStat(string pchName) {
        return SteamManager.Initialized
            && SteamUserStats.GetStat(pchName, out int previousValue)
            && SteamUserStats.SetStat(pchName, previousValue + 1)
            && SteamUserStats.StoreStats();
    }
}
