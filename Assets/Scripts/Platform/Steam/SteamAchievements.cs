using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

public class SteamAchievements : MonoBehaviour
{
    void Start()
    {
        if (SteamManager.Initialized)
            SteamUserStats.RequestCurrentStats();
    }

    //Debug shortcut resetting the player's stats and achievements, the Debug controls are never enabled in release builds
    private void OnEnable() => GameInput.Controls.Debug.ResetAchievements.performed += OnResetAchievements;
    private void OnDisable() => GameInput.Controls.Debug.ResetAchievements.performed -= OnResetAchievements;

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
