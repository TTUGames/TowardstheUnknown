using Steamworks;
using UnityEngine;

/// <summary>
/// What the player's Steam friends read under their name (<see cref="RunPresence"/>). Steam writes it from the tokens of Steamworks/rich_presence.vdf, uploaded in the app's Steamworks settings
/// (Rich Presence localization): <c>steam_display</c> names the token, the other keys fill it
/// </summary>
public static class SteamPresence
{
    // After RunPresence's events are cleared for the session
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() => RunPresence.Changed += Show;

    // "Room 6 · Fighting", "Facing Drareg · phase 2"; what a run shows stays until its first room starts
    private static void Show()
    {
        if (RunPresence.Shown == RunPresence.Display.None) return;
        if (RunPresence.Shown == RunPresence.Display.Boss) Set("phase", RunPresence.Number.ToString());
        if (RunPresence.Shown == RunPresence.Display.Room)
        {
            Set("room", RunPresence.Number.ToString());
            Set("kind", RunPresence.RoomKind.ToString());
        }
        Set("steam_display", "#" + RunPresence.Shown);
    }

    private static void Set(string key, string value)
    {
        if (SteamManager.Initialized) SteamFriends.SetRichPresence(key, value);
    }
}
