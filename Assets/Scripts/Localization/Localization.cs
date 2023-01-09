using System.Collections.Generic;
using UnityEngine;

public static class Localization {
    private static readonly string localizationPath = "Localization/";

    private static Dictionary<string, ArtifactDescription> itemDescriptions;
    private static Dictionary<string, SimpleLocalizedText> UIStrings;

    /// <summary>
    /// Initializes all dictionaries
    /// </summary>
    static Localization() {
        Init();
    }

    public static void Init() {
        InitItemDescriptions("EN");
        InitUIStrings("EN");
    }

    private static void InitItemDescriptions(string lang) {
        itemDescriptions = JsonUtility.FromJson<LocalizedTextList<ArtifactDescription>>(Resources.Load<TextAsset>(localizationPath + lang + "/ArtifactDescriptions").text).ToDictionary();
    }

    private static void InitUIStrings(string lang) {
        UIStrings = JsonUtility.FromJson<LocalizedTextList<SimpleLocalizedText>>(Resources.Load<TextAsset>(localizationPath + lang + "/UIStrings").text).ToDictionary();
    }

    /// <summary>
    /// Returns a specific item's description
    /// </summary>
    /// <param name="ID">The item's ID</param>
    /// <returns>The corresponding ItemDescription</returns>
    public static ArtifactDescription GetArtifactDescription(string ID) {
        if (itemDescriptions.ContainsKey(ID))
            return itemDescriptions[ID];

        return new ArtifactDescription();
    }

    public static SimpleLocalizedText GetUIString(string ID) {
        if (UIStrings.ContainsKey(ID))
            return UIStrings[ID];

        return new SimpleLocalizedText();
    }
}


