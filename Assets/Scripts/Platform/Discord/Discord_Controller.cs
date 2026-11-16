using Discord.Sdk;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the game in the player's Discord status, in the game's language: at the menu, or in a run with the room and what
/// happens there (<see cref="RunPresence"/>), the edition as the small image, the time since the scene loaded (a new run
/// starts it again). The first instance lives across the scenes; those of the scenes loaded afterwards destroy themselves.
/// Through the Discord Social SDK (<c>Packages/com.discord.partnersdk</c>), without connecting an account: on desktop the
/// presence goes to the Discord client running on the machine, and nothing shows without one. The SDK runs its callbacks
/// from the player loop
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

    private Client discord;
    private long startTime;

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
        RunPresence.SceneStarted += OnSceneStarted;
        RunPresence.Changed += ShowActivity;
        Edition.Changed += OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    void Start()
    {
        if (instance != this) return;
        try
        {
            discord = new Client();
            discord.SetApplicationId((ulong)applicationID);
        }
        catch (System.Exception e)
        {
            // The native library missing for this platform
            Debug.LogWarning("Discord - Unavailable: " + e.Message);
            discord = null;
            enabled = false;
            return;
        }
        OnSceneStarted();
        ShowActivity();
    }

    // The time starts again with each scene: the menu, a new run
    private void OnSceneStarted() => startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

    private void OnEditionChanged(GameEdition edition) => ShowActivity();

    private void OnLocaleChanged(Locale locale) => ShowActivity();

    /// <summary>
    /// The details (the menu or the run), the state, the edition's image, the time since the scene loaded
    /// </summary>
    private string Details => Localization.UI(SceneManager.GetActiveScene().buildIndex == GameFlow.MainMenuScene ? "DiscordMenu" : "DiscordRun");

    // "Room 6 · Fighting", "Facing Drareg · phase 2", none at the menu and in a run until its first room starts
    private string State => RunPresence.Shown is RunPresence.Display.None or RunPresence.Display.Menu ? ""
        : string.Format(Localization.UI("Discord" + RunPresence.Shown), RunPresence.Number, Localization.UI("Discord" + RunPresence.RoomKind));

    private void ShowActivity()
    {
        if (discord == null) return;

        using var activity = new Activity();
        activity.SetType(ActivityTypes.Playing);
        activity.SetDetails(Details);
        if (!string.IsNullOrEmpty(State)) activity.SetState(State);

        using var timestamps = new ActivityTimestamps();
        timestamps.SetStart((ulong)startTime);
        activity.SetTimestamps(timestamps);

        using var assets = new ActivityAssets();
        assets.SetLargeImage(largeImageName);
        // The version shows on the image's hover
        assets.SetLargeText(string.IsNullOrEmpty(largeImageText) ? $"v{Application.version}" : $"{largeImageText} · v{Application.version}");
        assets.SetSmallImage(Edition.Current == GameEdition.Classic ? classicImageName : anniversaryImageName);
        assets.SetSmallText(Localization.UI("Edition" + Edition.Current));
        activity.SetAssets(assets);

        // Fails when the Discord client is not running: the next update tries again
        discord.UpdateRichPresence(activity, result =>
        {
            if (!result.Successful()) Debug.LogWarning("Discord - Activity update failed: " + result.Error());
        });
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        RunPresence.SceneStarted -= OnSceneStarted;
        RunPresence.Changed -= ShowActivity;
        Edition.Changed -= OnEditionChanged;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        discord?.Dispose();
        discord = null;
    }
}
