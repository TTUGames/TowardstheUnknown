using UnityEngine;
using Steamworks;

public class SteamAchievements : MonoBehaviour
{
    private bool currentStatsRequested = false;

    void Update()
    {
        if (!SteamManager.Initialized)
            return;
        if (!currentStatsRequested) {
            currentStatsRequested = true;
            SteamUserStats.RequestCurrentStats();
        }

        // Debug shortcut resetting the player's stats and achievements, never available in release builds
        if (GameInput.Controls.Debug.ResetAchievements.WasPressedThisFrame()) {
            SteamUserStats.ResetAllStats(true);
            SteamUserStats.StoreStats();
        }
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
