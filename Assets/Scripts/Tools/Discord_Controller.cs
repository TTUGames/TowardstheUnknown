using Discord;
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
    private Discord.Activity activity;

    void Start()
    {
        Debug.LogWarning("Discord - Create discord");
        // Log in with the Application ID
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.Default);

        Debug.LogWarning("Discord - Create activity");
        var activity = new Discord.Activity
        {
            Details = details,
            State = state,
            Timestamps =
            {
                Start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            },
            Assets =
            {
                LargeImage = largeImageName,
                LargeText = largeImageText,
                SmallImage = smallImageName,
                SmallText = smallImageText,
            },
        };

        Debug.LogWarning("Discord - Get activty manager");
        var activityManager = discord.GetActivityManager();

        /* activityManager.ClearActivity((result) =>
        {
            if (result == Discord.Result.Ok)
            {
                Debug.LogWarning("Discord - Clear success!");
            }
            else
            {
                Debug.LogWarning("Discord - Clear failed");
            }
        }); */

        Debug.LogWarning("Discord - Update activity");
        activityManager.UpdateActivity(activity, (result) =>
        {
            Debug.LogWarning("Discord - Result:");
            if (result == Discord.Result.Ok)
            {
                Debug.LogWarning("Discord - Success!");
            }
            else
            {
                Debug.LogWarning("Discord - Failed");
            }
        });
    }

    void Update() {
        discord.RunCallbacks();
    }
}