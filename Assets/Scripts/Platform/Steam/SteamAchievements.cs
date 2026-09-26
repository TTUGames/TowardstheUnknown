using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

/// <summary>
/// Updates the Steam stats and achievements from the game events
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

    private void OnEntityDied(EntityStats entity)
    {
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
        if (firstVisit && room.type != RoomType.SPAWN) IncrementStat("explored_rooms");
    }

    private void OnRunEnded(bool isVictory)
    {
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

    private static bool IncrementStat(string pchName) {
        return SteamManager.Initialized
            && SteamUserStats.GetStat(pchName, out int previousValue)
            && SteamUserStats.SetStat(pchName, previousValue + 1)
            && SteamUserStats.StoreStats();
    }
}
