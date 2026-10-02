using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Reads the texts of the Unity Localization string tables in the selected locale
/// </summary>
public static class Localization
{
    public const string ArtifactsTable = "Artifacts";
    public const string UITable = "UI";
    public const string EntitiesTable = "Entities";

    private const string LanguageKey = "Language";

    /// <summary>
    /// The code of the language chosen in the options, empty if the player never chose one
    /// </summary>
    public static string SavedLanguage => PlayerPrefs.GetString(LanguageKey, "");

    /// <summary>
    /// Switches the texts to <paramref name="locale"/> and keeps it for the next launches
    /// </summary>
    public static void SelectLanguage(Locale locale)
    {
        LocalizationSettings.SelectedLocale = locale;
        PlayerPrefs.SetString(LanguageKey, locale.Identifier.Code);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Gets a text, formatted with the arguments if it is a smart string. Its damage and block tags are colored by the
    /// UI showing it (<see cref="RichText.Highlight"/>)
    /// </summary>
    public static string Get(string table, string key, object arguments = null)
    {
        return arguments == null
            ? LocalizationSettings.StringDatabase.GetLocalizedString(table, key)
            : LocalizationSettings.StringDatabase.GetLocalizedString(table, key, new[] { arguments });
    }

    /// <summary>
    /// Gets a field (Title, Description, Effects, Range, Cooldown) of an artifact's texts
    /// </summary>
    public static string Artifact(string id, string field, object arguments = null) => Get(ArtifactsTable, id + "." + field, arguments);

    public static string UI(string id) => Get(UITable, id);

    /// <summary>
    /// An integer with its thousands grouped as the selected language writes them: "12,500" in English, "12 500" in French
    /// (a plain space: the fonts may lack the narrow no-break one)
    /// </summary>
    public static string Number(int value)
    {
        string grouped = value.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
        string language = LocalizationSettings.SelectedLocale != null ? LocalizationSettings.SelectedLocale.Identifier.Code : "en";
        return language.StartsWith("fr") ? grouped.Replace(',', ' ') : grouped;
    }

    /// <summary>
    /// Gets an entity's name from its <c>EntityData</c> asset name
    /// </summary>
    public static string Entity(string id) => Get(EntitiesTable, id);
}
