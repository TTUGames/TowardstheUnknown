using UnityEngine;

public class Discord_Controller : MonoBehaviour
{
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

        var activity = new Discord.Activity
        {
            Details = details,
            State = state,
            Timestamps = { Start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() },
            Assets =
            {
                LargeImage = largeImageName,
                LargeText = largeImageText,
                SmallImage = smallImageName,
                SmallText = smallImageText,
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
        }
    }

    private void OnDestroy()
    {
        discord?.Dispose();
    }
}
