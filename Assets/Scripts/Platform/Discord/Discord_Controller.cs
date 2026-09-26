using UnityEngine;

/// <summary>
/// Shows the game in the player's Discord status. The first instance lives across the scenes and keeps the play time;
/// the instance of each scene loaded afterwards only passes its texts to it
/// </summary>
public class Discord_Controller : MonoBehaviour
{
    private static Discord_Controller instance;

    public long applicationID;

    [Space]
    public string details;
    public string state;

    [Space]
    public string largeImageName;
    public string largeImageText;
    public string smallImageName;
    public string smallImageText;

    private Discord.Discord discord;
    private long startTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null)
        {
            instance.ShowActivity(this);
            Destroy(gameObject);
            return;
        }
        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
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

        startTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        ShowActivity(this);
    }

    /// <summary>
    /// Shows the texts and images of <paramref name="source"/>, keeping the time elapsed since the launch
    /// </summary>
    private void ShowActivity(Discord_Controller source)
    {
        if (discord == null) return;

        var activity = new Discord.Activity
        {
            Details = source.details,
            State = source.state,
            Timestamps = { Start = startTime },
            Assets =
            {
                LargeImage = source.largeImageName,
                LargeText = source.largeImageText,
                SmallImage = source.smallImageName,
                SmallText = source.smallImageText,
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
        Release();
    }
}
