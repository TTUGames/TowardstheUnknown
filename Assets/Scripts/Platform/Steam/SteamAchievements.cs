using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

/// <summary>
/// Updates the Steam stats and achievements from the run's progress (<see cref="RunStats"/>) and its end, only in a randomly
/// generated run: the test maps push nothing. At the run's end, the score goes to the best scores' leaderboard (kept if it
/// is the player's best), whose world rank <see cref="GameEvents.ScoreRanked"/> gives to the results; not from the editor,
/// whose test runs would rank
/// </summary>
public class SteamAchievements : MonoBehaviour
{
    private const int MaxScore = 50000;
    private const string Leaderboard = "best_score";

    // The run's counts already added to the Steam stats
    private int pushedKills;
    private int pushedRooms;
    // The stats can be read and changed once Steam has sent them (UserStatsReceived_t): the counts wait until then
    private bool statsReceived;
    private Callback<UserStatsReceived_t> statsReceivedCallback;
    private CallResult<LeaderboardFindResult_t> leaderboardFound;
    private CallResult<LeaderboardScoreUploaded_t> scoreUploaded;

    void Start()
    {
        if (!SteamManager.Initialized) return;
        statsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
        SteamUserStats.RequestCurrentStats();
    }

    private void OnStatsReceived(UserStatsReceived_t received)
    {
        if (received.m_nGameID != (ulong)SteamUtils.GetAppID().m_AppId || received.m_eResult != EResult.k_EResultOK) return;
        statsReceived = true;
        // What the run counted meanwhile
        OnRunChanged();
    }

    private void OnDestroy()
    {
        statsReceivedCallback?.Dispose();
        leaderboardFound?.Dispose();
        scoreUploaded?.Dispose();
        Store();
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

    private bool Counts => SteamManager.Initialized && statsReceived && GameScene.Map != null && GameScene.Map.IsRandomRun;

    // A count is pushed once Steam took it; the stats are sent to Steam on each new room rather than on each kill
    private void OnRunChanged()
    {
        if (!Counts) return;
        RunStats run = GameScene.Run;
        if (AddToStat("entity_killed", run.KillCount - pushedKills)) pushedKills = run.KillCount;
        if (AddToStat("explored_rooms", run.VisitedRoomCount - pushedRooms))
        {
            pushedRooms = run.VisitedRoomCount;
            Store();
        }
    }

    // The run ends on the death of the player or of Drareg
    private void OnRunEnded(bool isVictory)
    {
        if (!Counts) return;
        OnRunChanged();
        if (isVictory) SetAchievement("ACH_KILL_DRAREG");
        else AddToStat("death", 1);
        if (GameScene.Run.Score >= MaxScore) SetAchievement("ACH_MAXSCORE");
        Store();
        if (!Application.isEditor) UploadScore(GameScene.Run.Score);
    }

    // Finds the leaderboard (made on its first use), then sends the score, the best one kept
    private void UploadScore(int score)
    {
        leaderboardFound = CallResult<LeaderboardFindResult_t>.Create((found, failure) => {
            if (failure || found.m_bLeaderboardFound == 0) return;
            scoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnScoreUploaded);
            scoreUploaded.Set(SteamUserStats.UploadLeaderboardScore(found.m_hSteamLeaderboard,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0));
        });
        leaderboardFound.Set(SteamUserStats.FindOrCreateLeaderboard(Leaderboard,
            ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric));
    }

    private static void OnScoreUploaded(LeaderboardScoreUploaded_t uploaded, bool failure)
    {
        if (failure || uploaded.m_bSuccess == 0 || uploaded.m_nGlobalRankNew <= 0) return;
        GameEvents.RankScore(uploaded.m_nGlobalRankNew, uploaded.m_bScoreChanged != 0);
    }

    private void OnResetAchievements(InputAction.CallbackContext context)
    {
        if (!SteamManager.Initialized) return;
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.StoreStats();
    }

    private static bool SetAchievement(string pchName) {
        return SteamManager.Initialized && SteamUserStats.SetAchievement(pchName);
    }

    // Changes the stat locally: Store sends the changes to Steam
    private static bool AddToStat(string pchName, int amount) {
        return amount > 0
            && SteamManager.Initialized
            && SteamUserStats.GetStat(pchName, out int previousValue)
            && SteamUserStats.SetStat(pchName, previousValue + amount);
    }

    private void Store() {
        if (SteamManager.Initialized && statsReceived) SteamUserStats.StoreStats();
    }
}
