using UnityEngine.Localization.Settings;

/// <summary>
/// Reads the texts of the Unity Localization string tables in the selected locale
/// </summary>
public static class Localization
{
    private const string HighlightColor = "#e82a65";

    public const string ArtifactsTable = "Artifacts";
    public const string UITable = "UI";
    public const string EntitiesTable = "Entities";

    /// <summary>
    /// Gets a text, formatted with the arguments if it is a smart string
    /// </summary>
    public static string Get(string table, string key, object arguments = null)
    {
        string text = arguments == null
            ? LocalizationSettings.StringDatabase.GetLocalizedString(table, key)
            : LocalizationSettings.StringDatabase.GetLocalizedString(table, key, new[] { arguments });
        return Highlight(text);
    }

    /// <summary>
    /// Gets a field (Title, Description, Effects, Range, Cooldown) of an artifact's texts
    /// </summary>
    public static string Artifact(string id, string field, object arguments = null) => Get(ArtifactsTable, id + "." + field, arguments);

    public static string UI(string id) => Get(UITable, id);

    /// <summary>
    /// Gets an entity's name from its prefab name
    /// </summary>
    public static string Entity(string id) => Get(EntitiesTable, id);

    /// <summary>
    /// Replaces the damage (D) and block (B) tags of a text with colors
    /// </summary>
    public static string Highlight(string text)
    {
        return text?.Replace("<D>", "<color=" + HighlightColor + ">").Replace("</D>", "</color>")
                    .Replace("<B>", "<color=" + HighlightColor + ">").Replace("</B>", "</color>");
    }
}
