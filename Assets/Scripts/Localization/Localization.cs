using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public static class Localization {
    private static readonly string localizationPath = "Localization/";

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

        if (SteamManager.Initialized) {
            switch(SteamApps.GetCurrentGameLanguage()) {
                case "french":
                    lang = "fr";
                    break;
                case "english":
                    lang = "en";
                    break;
                default:
                    lang = "en";
                    break;
			}
		}
        else {
            Debug.LogError("Cannot check steam language. Is Steam working and SteamManager instantiated ?");
		}

        InitArtifactDescriptions(lang);
        InitUIStrings(lang);
        InitEntityDescriptions(lang);
    }

    private static void InitArtifactDescriptions(string lang) {
        itemDescriptions = JsonUtility.FromJson<LocalizedTextList<ArtifactDescription>>(Resources.Load<TextAsset>(localizationPath + lang + "/ArtifactDescriptions").text).ToDictionary();
    }

    private static void InitUIStrings(string lang) {
        UIStrings = JsonUtility.FromJson<LocalizedTextList<SimpleLocalizedText>>(Resources.Load<TextAsset>(localizationPath + lang + "/UIStrings").text).ToDictionary();
    }
    private static void InitEntityDescriptions(string lang) {
        entityDescriptions = JsonUtility.FromJson<LocalizedTextList<EntityDescription>>(Resources.Load<TextAsset>(localizationPath + lang + "/EntityDescriptions").text).ToDictionary();
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

    public static EntityDescription GetEntityDescription(string ID) {
        if (entityDescriptions.ContainsKey(ID))
            return entityDescriptions[ID];

        return new EntityDescription();
	}
}


