using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the game in the player's Discord status, in the game's language: at the menu, or in a run with the room and what
/// happens there (Drareg's phase, the run's end), the edition as the small image, the time since the scene loaded (a new run
/// starts it again). The first instance lives across the scenes; those of the scenes loaded afterwards destroy themselves
/// </summary>
public class Discord_Controller : MonoBehaviour
{
    private static Discord_Controller instance;

    public long applicationID;

    [Space]
    public string largeImageName;
    public string largeImageText;
    [Tooltip("The small images of the editions, keys of the Rich Presence assets of the Discord application")]
    public string anniversaryImageName = "edition_anniversary";
    public string classicImageName = "edition_classic";

    private Discord.Discord discord;
    private long startTime;
    private int bossPhase;
    // The state's UI key and its arguments, rewritten in a new language
    private string stateKey;
    private object[] stateArgs = { };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.CombatStarted += ShowRoom;
        GameEvents.ExplorationStarted += ShowRoom;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
        GameEvents.RunEnded += OnRunEnded;
        Edition.Changed += OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    void Start()
    {
        if (instance != this) return;
        try
        {
            // Fails when the Discord client is not running
            discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Discord - Unavailable: " + e.Message);
            enabled = false;
            return;
        }
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // The time starts again with each scene: the menu, a new run
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        bossPhase = 0;
        SetState(null);
    }

    private void OnBossPhaseChanged(int phase)
    {
        bossPhase = phase;
        ShowRoom();
    }

    private void OnRunEnded(bool isVictory) => SetState(isVictory ? "DiscordVictory" : "DiscordDefeat");

    private void OnEditionChanged(GameEdition edition) => ShowActivity();

    private void OnLocaleChanged(Locale locale) => ShowActivity();

    // "Room 6 · Fighting", "Facing Drareg · phase 2"
    private void ShowRoom()
    {
        Room room = GameScene.Map != null ? GameScene.Map.CurrentRoom : null;
        if (room == null || GameScene.Run == null) return;
        if (room.type == RoomType.BOSS)
        {
            SetState("DiscordBoss", Mathf.Max(bossPhase, 1));
            return;
        }
        string kind = TurnSystem.Instance != null && TurnSystem.Instance.IsCombat ? "DiscordCombat" : room.type switch {
            RoomType.TREASURE => "DiscordTreasure",
            RoomType.ANTECHAMBER => "DiscordAntechamber",
            _ => "DiscordExploring",
        };
        SetState("DiscordRoom", GameScene.Run.VisitedRoomCount + 1, kind);
    }

    /// <param name="key">The state's UI key, null for none</param>
    /// <param name="args">Its arguments: a string argument is itself a UI key</param>
    private void SetState(string key, params object[] args)
    {
        stateKey = key;
        stateArgs = args;
        ShowActivity();
    }

    /// <summary>
    /// The details (the menu or the run), the state, the edition's image, the time since the scene loaded
    /// </summary>
    public string Details => Localization.UI(SceneManager.GetActiveScene().buildIndex == GameFlow.MainMenuScene ? "DiscordMenu" : "DiscordRun");

    public string State => stateKey == null ? "" : string.Format(Localization.UI(stateKey),
        System.Array.ConvertAll(stateArgs, arg => arg is string key ? Localization.UI(key) : arg));

    private void ShowActivity()
    {
        if (discord == null) return;

        var activity = new Discord.Activity
        {
            Details = Details,
            State = State,
            Timestamps = { Start = startTime },
            Assets =
            {
                LargeImage = largeImageName,
                // The version shows on the image's hover
                LargeText = string.IsNullOrEmpty(largeImageText) ? $"v{Application.version}" : $"{largeImageText} · v{Application.version}",
                SmallImage = Edition.Current == GameEdition.Classic ? classicImageName : anniversaryImageName,
                SmallText = Localization.UI("Edition" + Edition.Current),
            },
        };

        discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            if (result != Discord.Result.Ok) Debug.LogWarning("Discord - Activity update failed: " + result);
        });
    }

    void Update()
    {
        try
        {
            discord.RunCallbacks();
        }
        catch (System.Exception e)
        {
            // The Discord client was closed
            Debug.LogWarning("Discord - Disconnected: " + e.Message);
            enabled = false;
            Release();
        }
    }

    /// <summary>
    /// Disposes the SDK, which can itself throw once the client is gone
    /// </summary>
    private void Release()
    {
        Discord.Discord closed = discord;
        discord = null;
        try
        {
            closed?.Dispose();
        }
        catch (System.Exception) { }
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.CombatStarted -= ShowRoom;
        GameEvents.ExplorationStarted -= ShowRoom;
        GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
        GameEvents.RunEnded -= OnRunEnded;
        Edition.Changed -= OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        Release();
    }
}
