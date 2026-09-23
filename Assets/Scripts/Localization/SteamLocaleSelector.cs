using Steamworks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Starts the game in the language chosen in Steam: French or English
/// </summary>
[System.Serializable]
public class SteamLocaleSelector : IStartupLocaleSelector
{
    public Locale GetStartupLocale(ILocalesProvider availableLocales)
    {
        if (!SteamManager.Initialized) return null;
        return availableLocales.GetLocale(SteamApps.GetCurrentGameLanguage() == "french" ? "fr" : "en");
    }
}
