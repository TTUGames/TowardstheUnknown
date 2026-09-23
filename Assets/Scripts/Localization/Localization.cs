using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public static class Localization {
    private static Dictionary<string, ArtifactDescription> itemDescriptions;
    private static Dictionary<string, SimpleLocalizedText> UIStrings;
    private static Dictionary<string, EntityDescription> entityDescriptions;

    /// <summary>
    /// Initializes all dictionaries
    /// </summary>
    static Localization() {
        Init();
    }

    public static void Init() {
        string lang = "fr";
        if (SteamManager.Initialized)
            lang = SteamApps.GetCurrentGameLanguage() == "french" ? "fr" : "en";
        else
            Debug.LogError("Cannot check steam language. Is Steam working and SteamManager instantiated ?");

        itemDescriptions = Load<ArtifactDescription>(lang, "ArtifactDescriptions");
        UIStrings = Load<SimpleLocalizedText>(lang, "UIStrings");
        entityDescriptions = Load<EntityDescription>(lang, "EntityDescriptions");
    }

    private static Dictionary<string, T> Load<T>(string lang, string file) where T : LocalizedText {
        return JsonUtility.FromJson<LocalizedTextList<T>>(Resources.Load<TextAsset>("Localization/" + lang + "/" + file).text).ToDictionary();
    }

    /// <summary>
    /// Returns a specific item's description
    /// </summary>
    /// <param name="ID">The item's ID</param>
    /// <returns>The corresponding ItemDescription</returns>
    public static ArtifactDescription GetArtifactDescription(string ID) {
        return itemDescriptions.TryGetValue(ID, out ArtifactDescription text) ? text : new ArtifactDescription();
    }

    public static SimpleLocalizedText GetUIString(string ID) {
        return UIStrings.TryGetValue(ID, out SimpleLocalizedText text) ? text : new SimpleLocalizedText();
    }

    public static EntityDescription GetEntityDescription(string ID) {
        return entityDescriptions.TryGetValue(ID, out EntityDescription text) ? text : new EntityDescription();
	}
}
