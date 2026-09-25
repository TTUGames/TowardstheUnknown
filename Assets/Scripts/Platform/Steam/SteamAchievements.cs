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
            IncrementStat("death", 1);
            return;
        }
        IncrementStat("entity_killed", 1);
        if (entity is DraregStats) SetAchievement("ACH_KILL_DRAREG");
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (firstVisit && room.type != RoomType.SPAWN) IncrementStat("explored_rooms", 1);
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

    public static bool SetAchievement(string pchName) {
        return SteamManager.Initialized && SteamUserStats.SetAchievement(pchName) && SteamUserStats.StoreStats();
    }

    public static bool IncrementStat(string pchName, int value) {
        return SteamManager.Initialized
            && SteamUserStats.GetStat(pchName, out int previousValue)
            && SteamUserStats.SetStat(pchName, previousValue + value)
            && SteamUserStats.StoreStats();
    }
}
