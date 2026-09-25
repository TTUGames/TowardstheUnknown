using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Starts the game in the language chosen in the options, if the player chose one (<see cref="Localization.SelectLanguage"/>)
/// </summary>
[System.Serializable]
public class SavedLocaleSelector : IStartupLocaleSelector
{
    public Locale GetStartupLocale(ILocalesProvider availableLocales)
    {
        string code = Localization.SavedLanguage;
        return string.IsNullOrEmpty(code) ? null : availableLocales.GetLocale(code);
    }
}
