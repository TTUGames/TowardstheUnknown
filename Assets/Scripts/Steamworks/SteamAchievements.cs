using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamAchievements : MonoBehaviour
{
    private bool currentStatsRequested = false;

    void Update()
    {
        if (!SteamManager.Initialized)
            return;
        else if (!currentStatsRequested) {
            currentStatsRequested = true;
            SteamUserStats.RequestCurrentStats();
        }
        
        // TODO Remove
        if (Input.GetKeyDown(KeyCode.End)) {
            SteamUserStats.ResetAllStats(true);
            SteamUserStats.StoreStats();
        }
    }

    public static bool SetAchievement(string pchName) {
        if (!SteamManager.Initialized)
            return false;

        if (!SteamUserStats.SetAchievement(pchName))
            return false;
        return SteamUserStats.StoreStats();
    }

    public static bool IncrementStat(string pchName, int value) {
        int previousValue;

        if (!SteamManager.Initialized)
            return false;

        if (!SteamUserStats.GetStat(pchName, out previousValue))
            return false;
        if (!SteamUserStats.SetStat(pchName, previousValue + value))
            return false;
        return SteamUserStats.StoreStats();
    }
}
